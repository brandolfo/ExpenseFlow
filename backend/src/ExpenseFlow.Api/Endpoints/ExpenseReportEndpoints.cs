using ExpenseFlow.Api.Contracts;
using ExpenseFlow.Application.Abstractions;
using ExpenseFlow.Application.ExpenseReports.Parsing;
using ExpenseFlow.Application.ExpenseReports.Pdf;
using ExpenseFlow.Application.ExpenseReports.PdfProcessing;
using ExpenseFlow.Application.ExpenseReports.Processing;
using ExpenseFlow.Domain.ExpenseReports;
using ExpenseFlow.Domain.Transactions;

namespace ExpenseFlow.Api.Endpoints;

public static class ExpenseReportEndpoints
{
    private const int MaximumPdfBytes = 5 * 1024 * 1024;

    public static IEndpointRouteBuilder MapExpenseReportEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/expense-reports/process", ProcessAsync);
        app.MapPost("/api/expense-reports/process-pdf", ProcessPdfAsync);

        return app;
    }

    private static async Task<IResult> ProcessAsync(
        ProcessExpenseReportRequest? request,
        IExpenseReportProcessingService processingService,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var requestErrors = ValidateRequest(request);

        if (requestErrors.Count > 0)
        {
            return Results.BadRequest(new ProcessExpenseReportErrorResponse(
                "Expense report request is invalid.",
                requestErrors));
        }

        try
        {
            var result = await processingService.ProcessAsync(
                new ExpenseReportProcessingRequest(
                    request!.SourceName!.Trim(),
                    request.CsvText!,
                    request.ExpectedTotal),
                cancellationToken);

            if (!result.IsSuccess)
            {
                return Results.BadRequest(new ProcessExpenseReportErrorResponse(
                    "CSV input could not be processed.",
                    result.FileErrors.Select(MapFileError).ToArray()));
            }

            return Results.Ok(MapResponse(result.Report!));
        }
        catch (Exception exception)
        {
            var logger = loggerFactory.CreateLogger("ExpenseReportProcessing");
            logger.LogError(exception, "Unexpected error while processing expense report.");

            return Results.Problem(
                title: "Expense report processing failed.",
                detail: "An unexpected error occurred while processing the request.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> ProcessPdfAsync(
        ProcessPdfExpenseReportRequest? request,
        IPdfExpenseReportProcessingService processingService,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var requestErrors = ValidatePdfRequest(request, out var pdfContent);

        if (requestErrors.Count > 0)
        {
            return Results.BadRequest(new ProcessExpenseReportErrorResponse(
                "PDF expense report request is invalid.",
                requestErrors));
        }

        try
        {
            var result = await processingService.ProcessAsync(
                new PdfExpenseReportProcessingRequest(
                    request!.SourceName!.Trim(),
                    pdfContent!,
                    request.ExpectedTotal,
                    NormalizeOptional(request.StatementShapeHint)),
                cancellationToken);

            if (!result.IsSuccess)
            {
                return Results.BadRequest(new ProcessExpenseReportErrorResponse(
                    "PDF input could not be processed.",
                    MapPdfWarningsToFileErrors(result.ExtractionWarnings)));
            }

            return Results.Ok(MapPdfResponse(result));
        }
        catch (Exception exception)
        {
            var logger = loggerFactory.CreateLogger("PdfExpenseReportProcessing");
            logger.LogError(exception, "Unexpected error while processing PDF expense report.");

            return Results.Problem(
                title: "PDF expense report processing failed.",
                detail: "An unexpected error occurred while processing the request.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static IReadOnlyCollection<ApiFileError> ValidateRequest(ProcessExpenseReportRequest? request)
    {
        if (request is null)
        {
            return
            [
                new ApiFileError(
                    "invalid_request",
                    "Request body is required.",
                    [])
            ];
        }

        var errors = new List<ApiFileError>();

        if (string.IsNullOrWhiteSpace(request.SourceName))
        {
            errors.Add(new ApiFileError(
                "missing_source_name",
                "sourceName is required.",
                []));
        }

        if (string.IsNullOrWhiteSpace(request.CsvText))
        {
            errors.Add(new ApiFileError(
                FileValidationErrorCode.EmptyInput.ToString(),
                "csvText is required.",
                []));
        }

        return errors;
    }

    private static IReadOnlyCollection<ApiFileError> ValidatePdfRequest(
        ProcessPdfExpenseReportRequest? request,
        out byte[]? pdfContent)
    {
        pdfContent = null;

        if (request is null)
        {
            return
            [
                new ApiFileError(
                    "invalid_request",
                    "Request body is required.",
                    [])
            ];
        }

        var errors = new List<ApiFileError>();

        if (string.IsNullOrWhiteSpace(request.SourceName))
        {
            errors.Add(new ApiFileError(
                "missing_source_name",
                "sourceName is required.",
                []));
        }

        var statementShapeHint = NormalizeOptional(request.StatementShapeHint);

        if (statementShapeHint is not null && !IsSupportedStatementShapeHint(statementShapeHint))
        {
            errors.Add(new ApiFileError(
                "unsupported_statement_shape_hint",
                "statementShapeHint must be one of the supported synthetic statement shapes.",
                [PdfStatementShapeIds.IcbcVisaLikeV1, PdfStatementShapeIds.IcbcMastercardLikeV1]));
        }

        if (string.IsNullOrWhiteSpace(request.PdfBase64))
        {
            errors.Add(new ApiFileError(
                "missing_pdf_base64",
                "pdfBase64 is required.",
                []));
            return errors;
        }

        try
        {
            pdfContent = Convert.FromBase64String(request.PdfBase64.Trim());
        }
        catch (FormatException)
        {
            errors.Add(new ApiFileError(
                "invalid_pdf_base64",
                "pdfBase64 must be valid base64-encoded PDF content.",
                []));
            return errors;
        }

        if (pdfContent.Length > MaximumPdfBytes)
        {
            errors.Add(new ApiFileError(
                "pdf_too_large",
                "Decoded PDF content exceeds the 5 MB limit.",
                [$"maximumBytes={MaximumPdfBytes}"]));
        }

        return errors;
    }

    private static ProcessExpenseReportResponse MapResponse(ExpenseReport report) =>
        new(
            new ApiReportMetadata(
                report.Metadata.Product,
                report.Metadata.ReportType,
                report.Metadata.InputFormat,
                report.Metadata.SourceName,
                report.Metadata.GeneratedAtUtc,
                report.Metadata.UsesRealFinancialData),
            MapCounts(report.AuditSummary.Counts),
            new ApiTotals(
                report.Totals.ExpectedTotal,
                report.Totals.ProcessedTotal,
                report.Totals.CategoryTotal,
                report.Totals.ExcludedFromTotalsTotal),
            new ApiTotalValidation(
                report.ValidationResult.Status.ToString(),
                report.ValidationResult.ExpectedTotal,
                report.ValidationResult.ProcessedTotal,
                report.ValidationResult.Difference),
            report.CategoryTotals.Select(total => new ApiCategorySummary(
                total.Category.ToString(),
                total.TransactionCount,
                total.Total)).ToArray(),
            report.Transactions.Select(MapTransactionDetail).ToArray(),
            report.ReviewItems.Select(MapReviewItem).ToArray(),
            report.InvalidRows.Select(MapInvalidRow).ToArray(),
            report.ExcludedRows.Select(MapExcludedRow).ToArray(),
            new ApiAuditSummary(
                MapCounts(report.AuditSummary.Counts),
                report.AuditSummary.CompletenessStatus.ToString(),
                report.AuditSummary.AppliedDeterministicRuleCount,
                report.AuditSummary.ExpectedTotalValidationStatus.ToString(),
                report.AuditSummary.AiUsed,
                report.AuditSummary.Messages,
                report.AuditEntries.Select(entry => new ApiAuditEntry(
                    entry.SourceRowNumber,
                    entry.EventType.ToString(),
                    entry.Message,
                    entry.RuleId)).ToArray()));

    private static ProcessPdfExpenseReportResponse MapPdfResponse(PdfExpenseReportProcessingResult result) =>
        new(
            new ApiPdfExtractionMetadata(
                result.Metadata.SourceName,
                result.Metadata.StatementShapeId,
                result.Metadata.ExtractionStatus.ToString(),
                result.Metadata.NormalizedRowCount,
                result.Metadata.InvalidExtractedRowCount,
                result.Metadata.UnprocessableNormalizedRowCount,
                result.Metadata.SourceRowCount,
                result.Metadata.AiUsed),
            result.ExtractionWarnings.Select(warning => new ApiPdfExtractionWarning(
                warning.Code,
                warning.Message,
                warning.SourcePage,
                warning.ExtractionOrder)).ToArray(),
            MapResponse(result.Report!));

    private static ApiProcessingCounts MapCounts(ProcessingCounts counts) =>
        new(
            counts.SourceRows,
            counts.ValidRows,
            counts.CategorizedRows,
            counts.ReviewRequiredRows,
            counts.InvalidRows,
            counts.ExcludedFromTotalsRows,
            counts.PotentialDuplicateRows);

    private static ApiTransactionDetail MapTransactionDetail(ExpenseTransaction transaction) =>
        new(
            transaction.SourceRow.RowNumber,
            transaction.Date?.ToString("yyyy-MM-dd"),
            transaction.Description,
            transaction.Amount,
            transaction.Status.ToString(),
            transaction.Category?.ToString(),
            transaction.ReviewReason?.ToString(),
            transaction.IncludedInProcessedTotal,
            transaction.IncludedInCategoryTotals,
            transaction.RequiresReview,
            transaction.IsPotentialDuplicate,
            transaction.Installment,
            transaction.RuleMatches.Select(match => match.RuleId).ToArray());

    private static ApiReviewItem MapReviewItem(ExpenseTransaction transaction) =>
        new(
            transaction.SourceRow.RowNumber,
            transaction.Description,
            transaction.Amount,
            transaction.ReviewReason?.ToString(),
            transaction.Category?.ToString(),
            transaction.RuleMatches.Select(match => match.RuleId).ToArray(),
            transaction.IncludedInProcessedTotal,
            transaction.IncludedInCategoryTotals,
            transaction.IsPotentialDuplicate);

    private static ApiInvalidRow MapInvalidRow(ExpenseTransaction transaction) =>
        new(
            transaction.SourceRow.RowNumber,
            new ApiRawValues(
                transaction.SourceRow.RawDate,
                transaction.SourceRow.RawCode,
                transaction.SourceRow.RawDescription,
                transaction.SourceRow.RawAmount,
                transaction.SourceRow.RawInstallment,
                transaction.SourceRow.RawSourceType,
                transaction.SourceRow.RawNotes),
            transaction.ValidationIssues.Select(issue => issue.Message).ToArray(),
            transaction.IncludedInProcessedTotal,
            transaction.IncludedInCategoryTotals);

    private static ApiExcludedRow MapExcludedRow(ExpenseTransaction transaction) =>
        new(
            transaction.SourceRow.RowNumber,
            transaction.Description,
            transaction.Amount,
            transaction.Status.ToString(),
            transaction.ReviewReason?.ToString(),
            transaction.RuleMatches.Select(match => match.RuleId).ToArray());

    private static ApiFileError MapFileError(FileValidationError error) =>
        new(
            error.Code.ToString(),
            error.Message,
            error.Details ?? []);

    private static IReadOnlyCollection<ApiFileError> MapPdfWarningsToFileErrors(
        IReadOnlyCollection<PdfExtractionWarning> warnings) =>
        warnings.Count == 0
            ? [new ApiFileError("pdf_processing_failed", "PDF input could not be processed.", [])]
            : warnings.Select(warning => new ApiFileError(
                warning.Code,
                warning.Message,
                BuildPdfWarningDetails(warning))).ToArray();

    private static IReadOnlyCollection<string> BuildPdfWarningDetails(PdfExtractionWarning warning)
    {
        var details = new List<string>();

        if (warning.SourcePage is not null)
        {
            details.Add($"sourcePage={warning.SourcePage}");
        }

        if (warning.ExtractionOrder is not null)
        {
            details.Add($"extractionOrder={warning.ExtractionOrder}");
        }

        return details;
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static bool IsSupportedStatementShapeHint(string statementShapeHint) =>
        statementShapeHint is PdfStatementShapeIds.IcbcVisaLikeV1 or PdfStatementShapeIds.IcbcMastercardLikeV1;
}
