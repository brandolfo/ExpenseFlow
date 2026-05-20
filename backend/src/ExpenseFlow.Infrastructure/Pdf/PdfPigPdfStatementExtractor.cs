using System.Text.RegularExpressions;
using ExpenseFlow.Application.Abstractions;
using ExpenseFlow.Application.ExpenseReports.Pdf;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Exceptions;

namespace ExpenseFlow.Infrastructure.Pdf;

public sealed partial class PdfPigPdfStatementExtractor : IPdfStatementExtractor
{
    private const double LineTolerance = 3.0;

    public Task<PdfStatementExtractionResult> ExtractAsync(
        PdfStatementExtractionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.PdfContent.Length == 0)
        {
            return Task.FromResult(Failed(
                request.SourceName,
                PdfStatementShapeIds.Unknown,
                "empty_pdf_content",
                "PDF content was empty."));
        }

        try
        {
            using var document = PdfDocument.Open(request.PdfContent);
            var extractedPageLines = new List<PageLine>();

            foreach (var page in document.GetPages())
            {
                cancellationToken.ThrowIfCancellationRequested();
                extractedPageLines.AddRange(ExtractPageLines(page));
            }

            if (extractedPageLines.Count == 0)
            {
                return Task.FromResult(PdfStatementExtractionResult.Unsupported(
                    request.SourceName,
                    PdfStatementShapeIds.Unsupported,
                    PdfExtractionStatus.UnsupportedScannedImageOnly,
                    [new PdfExtractionWarning("no_text_extracted", "No selectable text was extracted from the PDF.")]));
            }

            var fullText = string.Join(Environment.NewLine, extractedPageLines.Select(line => line.Text));
            var statementShapeId = ResolveStatementShapeId(request.StatementShapeHint, fullText);
            var extractedLines = BuildExtractedLines(request.SourceName, request.StatementShapeHint, statementShapeId, extractedPageLines);

            if (statementShapeId == PdfStatementShapeIds.Unknown)
            {
                return Task.FromResult(new PdfStatementExtractionResult(
                    request.SourceName,
                    PdfStatementShapeIds.Unknown,
                    PdfExtractionStatus.UnsupportedStatementShape,
                    Array.Empty<PdfExtractedTransactionRow>(),
                    Array.Empty<PdfInvalidExtractedRow>(),
                    [new PdfExtractionWarning("unsupported_statement_shape", "The extracted text did not match a supported synthetic statement shape.")],
                    Array.Empty<PdfExtractedStatementTotal>(),
                    extractedLines));
            }

            return Task.FromResult(new PdfStatementExtractionResult(
                request.SourceName,
                statementShapeId,
                PdfExtractionStatus.Succeeded,
                Array.Empty<PdfExtractedTransactionRow>(),
                Array.Empty<PdfInvalidExtractedRow>(),
                Array.Empty<PdfExtractionWarning>(),
                Array.Empty<PdfExtractedStatementTotal>(),
                extractedLines));
        }
        catch (PdfDocumentEncryptedException)
        {
            return Task.FromResult(PdfStatementExtractionResult.Unsupported(
                request.SourceName,
                PdfStatementShapeIds.Unsupported,
                PdfExtractionStatus.UnsupportedEncrypted,
                [new PdfExtractionWarning("encrypted_pdf", "Encrypted PDF input is unsupported for this milestone.")]));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            return Task.FromResult(Failed(
                request.SourceName,
                PdfStatementShapeIds.Unsupported,
                "pdf_extraction_failed",
                $"PDF text extraction failed safely: {exception.GetType().Name}."));
        }
    }

    private static IReadOnlyCollection<PdfExtractedTextLine> BuildExtractedLines(
        string sourceName,
        string? statementShapeHint,
        string statementShapeId,
        IReadOnlyCollection<PageLine> pageLines)
    {
        var extractionOrder = 0;

        return pageLines
            .OrderBy(line => line.SourcePage)
            .ThenBy(line => line.PageOrder)
            .Select(line => new PdfExtractedTextLine(
                sourceName,
                statementShapeId,
                statementShapeHint,
                line.SourcePage,
                ++extractionOrder,
                line.Text))
            .ToArray();
    }

    private static IReadOnlyCollection<PageLine> ExtractPageLines(Page page)
    {
        var words = page
            .GetWords()
            .Where(word => !string.IsNullOrWhiteSpace(word.Text))
            .OrderByDescending(word => word.BoundingBox.Bottom)
            .ThenBy(word => word.BoundingBox.Left)
            .ToArray();
        var groups = new List<LineGroup>();

        foreach (var word in words)
        {
            var group = groups.FirstOrDefault(candidate => Math.Abs(candidate.Baseline - word.BoundingBox.Bottom) <= LineTolerance);

            if (group is null)
            {
                group = new LineGroup(word.BoundingBox.Bottom);
                groups.Add(group);
            }

            group.Words.Add(word);
        }

        return groups
            .OrderByDescending(group => group.Baseline)
            .Select((group, index) => new PageLine(
                page.Number,
                index + 1,
                NormalizeWhitespace(string.Join(" ", group.Words.OrderBy(word => word.BoundingBox.Left).Select(word => word.Text)))))
            .Where(line => !string.IsNullOrWhiteSpace(line.Text))
            .ToArray();
    }

    private static string ResolveStatementShapeId(string? statementShapeHint, string extractedText)
    {
        if (IsSupportedStatementShape(statementShapeHint))
        {
            return statementShapeHint!;
        }

        if (extractedText.Contains(PdfStatementShapeIds.IcbcVisaLikeV1, StringComparison.OrdinalIgnoreCase) ||
            extractedText.Contains("Synthetic ICBC-Like Visa Statement", StringComparison.OrdinalIgnoreCase))
        {
            return PdfStatementShapeIds.IcbcVisaLikeV1;
        }

        if (extractedText.Contains(PdfStatementShapeIds.IcbcMastercardLikeV1, StringComparison.OrdinalIgnoreCase) ||
            extractedText.Contains("Synthetic ICBC-Like Mastercard Statement", StringComparison.OrdinalIgnoreCase))
        {
            return PdfStatementShapeIds.IcbcMastercardLikeV1;
        }

        return PdfStatementShapeIds.Unknown;
    }

    private static bool IsSupportedStatementShape(string? statementShapeHint) =>
        statementShapeHint is PdfStatementShapeIds.IcbcVisaLikeV1 or PdfStatementShapeIds.IcbcMastercardLikeV1;

    private static PdfStatementExtractionResult Failed(
        string sourceName,
        string statementShapeId,
        string warningCode,
        string warningMessage) =>
        PdfStatementExtractionResult.Unsupported(
            sourceName,
            statementShapeId,
            PdfExtractionStatus.Failed,
            [new PdfExtractionWarning(warningCode, warningMessage)]);

    private static string NormalizeWhitespace(string text) =>
        MultipleWhitespaceRegex().Replace(text.Trim(), " ");

    [GeneratedRegex(@"\s+")]
    private static partial Regex MultipleWhitespaceRegex();

    private sealed record PageLine(int SourcePage, int PageOrder, string Text);

    private sealed class LineGroup(double baseline)
    {
        public double Baseline { get; } = baseline;

        public List<Word> Words { get; } = [];
    }
}
