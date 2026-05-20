using System.Globalization;
using ExpenseFlow.Application.Abstractions;
using ExpenseFlow.Application.ExpenseReports.Parsing;
using ExpenseFlow.Application.ExpenseReports.Pdf;
using ExpenseFlow.Application.ExpenseReports.Reporting;
using ExpenseFlow.Domain.Transactions;
using ExpenseFlow.Domain.Validation;

namespace ExpenseFlow.Application.ExpenseReports.PdfProcessing;

public sealed class DeterministicPdfExpenseReportProcessingService : IPdfExpenseReportProcessingService
{
    private const string SupportedReportCurrency = "ARS";

    private readonly IPdfStatementExtractor _extractor;
    private readonly IPdfStatementRowNormalizer _normalizer;
    private readonly ICategorizationRuleEngine _categorizationRuleEngine;
    private readonly IExpenseReportGenerator _reportGenerator;

    public DeterministicPdfExpenseReportProcessingService(
        IPdfStatementExtractor extractor,
        IPdfStatementRowNormalizer normalizer,
        ICategorizationRuleEngine categorizationRuleEngine,
        IExpenseReportGenerator reportGenerator)
    {
        _extractor = extractor;
        _normalizer = normalizer;
        _categorizationRuleEngine = categorizationRuleEngine;
        _reportGenerator = reportGenerator;
    }

    public async Task<PdfExpenseReportProcessingResult> ProcessAsync(
        PdfExpenseReportProcessingRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var extraction = await _extractor.ExtractAsync(
            new PdfStatementExtractionRequest(request.SourceName, request.PdfContent, request.StatementShapeHint),
            cancellationToken);
        var normalized = _normalizer.Normalize(extraction);
        var warnings = normalized.Warnings.ToList();
        var sourceRowCount = CalculateSourceRowCount(normalized);

        if (!normalized.IsProcessable)
        {
            return PdfExpenseReportProcessingResult.Failure(
                CreateMetadata(normalized, sourceRowCount, unprocessableNormalizedRowCount: 0),
                warnings);
        }

        var validCandidates = new List<ParsedTransactionCandidate>();
        var invalidRows = new List<InvalidTransactionRow>();
        var unprocessableNormalizedRowCount = 0;

        foreach (var row in normalized.ExtractedRows.OrderBy(row => row.ExtractionOrder))
        {
            if (!IsSupportedReportCurrency(row.CurrencyCode))
            {
                unprocessableNormalizedRowCount++;
                invalidRows.Add(CreateUnsupportedCurrencyInvalidRow(normalized, row));
                warnings.Add(new PdfExtractionWarning(
                    "unsupported_currency_for_report",
                    "PDF row currency cannot be processed by the current ARS deterministic report pipeline without conversion.",
                    row.SourcePage,
                    row.ExtractionOrder));
                continue;
            }

            validCandidates.Add(CreateParsedCandidate(normalized, row));
        }

        invalidRows.AddRange(normalized.InvalidRows
            .OrderBy(row => row.ExtractionOrder)
            .Select(row => CreateInvalidExtractedRow(normalized, row)));

        var transactions = _categorizationRuleEngine.Categorize(validCandidates);
        var report = _reportGenerator.Generate(new ExpenseReportGenerationInput(
            normalized.SourceName,
            sourceRowCount,
            transactions,
            invalidRows,
            request.ExpectedTotal));

        return PdfExpenseReportProcessingResult.Success(
            report,
            CreateMetadata(normalized, sourceRowCount, unprocessableNormalizedRowCount),
            warnings);
    }

    private static ParsedTransactionCandidate CreateParsedCandidate(
        PdfStatementExtractionResult extractionResult,
        PdfExtractedTransactionRow row)
    {
        var sourceRow = new TransactionSourceRow(
            row.ExtractionOrder,
            row.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            row.Code,
            row.Description,
            row.Amount.ToString("0.00", CultureInfo.InvariantCulture),
            row.Installment,
            row.SourceType,
            BuildTraceableNotes(extractionResult, row.SourcePage, row.ExtractionOrder, row.CurrencyCode, row.Notes));

        return new ParsedTransactionCandidate(sourceRow, row.Date, row.Description, row.Amount);
    }

    private static InvalidTransactionRow CreateInvalidExtractedRow(
        PdfStatementExtractionResult extractionResult,
        PdfInvalidExtractedRow row)
    {
        var sourceRow = new TransactionSourceRow(
            row.ExtractionOrder,
            row.RawFields.Date,
            row.RawFields.Code,
            row.RawFields.Description,
            row.RawFields.Amount,
            row.RawFields.Installment,
            row.RawFields.SourceType,
            BuildTraceableNotes(
                extractionResult,
                row.SourcePage,
                row.ExtractionOrder,
                row.RawFields.CurrencyCode,
                row.RawFields.Notes));

        var issues = row.Warnings.SelectMany(MapWarningToValidationIssues).ToArray();

        return new InvalidTransactionRow(
            sourceRow,
            issues.Length > 0
                ? issues
                : [new ValidationIssue(ReviewReason.NoMatchingRule, "PDF extracted row could not be normalized.")]);
    }

    private static InvalidTransactionRow CreateUnsupportedCurrencyInvalidRow(
        PdfStatementExtractionResult extractionResult,
        PdfExtractedTransactionRow row)
    {
        var sourceRow = new TransactionSourceRow(
            row.ExtractionOrder,
            row.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            row.Code,
            row.Description,
            row.Amount.ToString("0.00", CultureInfo.InvariantCulture),
            row.Installment,
            row.SourceType,
            BuildTraceableNotes(extractionResult, row.SourcePage, row.ExtractionOrder, row.CurrencyCode, row.Notes));

        return new InvalidTransactionRow(
            sourceRow,
            [new ValidationIssue(ReviewReason.UnsupportedCurrency, "PDF row currency is not supported by the current ARS report pipeline.")]);
    }

    private static IReadOnlyCollection<ValidationIssue> MapWarningToValidationIssues(PdfExtractionWarning warning) =>
        warning.Code switch
        {
            "missing_date_and_amount" =>
            [
                new ValidationIssue(ReviewReason.InvalidDate, "PDF extracted row is missing a date."),
                new ValidationIssue(ReviewReason.InvalidAmount, "PDF extracted row is missing an amount.")
            ],
            "malformed_date" => [new ValidationIssue(ReviewReason.InvalidDate, warning.Message)],
            "malformed_amount" => [new ValidationIssue(ReviewReason.InvalidAmount, warning.Message)],
            _ => [new ValidationIssue(ReviewReason.NoMatchingRule, warning.Message)]
        };

    private static string BuildTraceableNotes(
        PdfStatementExtractionResult extractionResult,
        int sourcePage,
        int extractionOrder,
        string? currencyCode,
        string? notes)
    {
        var trace = string.Join(
            "; ",
            $"pdf_source={extractionResult.SourceName}",
            $"statement_shape={extractionResult.StatementShapeId}",
            $"source_page={sourcePage.ToString(CultureInfo.InvariantCulture)}",
            $"extraction_order={extractionOrder.ToString(CultureInfo.InvariantCulture)}",
            $"currency={currencyCode ?? "unknown"}");

        return string.IsNullOrWhiteSpace(notes)
            ? trace
            : $"{notes}; {trace}";
    }

    private static bool IsSupportedReportCurrency(string? currencyCode) =>
        string.IsNullOrWhiteSpace(currencyCode) ||
        string.Equals(currencyCode, SupportedReportCurrency, StringComparison.OrdinalIgnoreCase);

    private static int CalculateSourceRowCount(PdfStatementExtractionResult extractionResult) =>
        extractionResult.ExtractedRows
            .Select(row => row.ExtractionOrder)
            .Concat(extractionResult.InvalidRows.Select(row => row.ExtractionOrder))
            .DefaultIfEmpty(0)
            .Max();

    private static PdfExpenseReportProcessingMetadata CreateMetadata(
        PdfStatementExtractionResult extractionResult,
        int sourceRowCount,
        int unprocessableNormalizedRowCount) =>
        new(
            extractionResult.SourceName,
            extractionResult.StatementShapeId,
            extractionResult.Status,
            extractionResult.ExtractedRows.Count,
            extractionResult.InvalidRows.Count,
            unprocessableNormalizedRowCount,
            sourceRowCount,
            AiUsed: false);
}
