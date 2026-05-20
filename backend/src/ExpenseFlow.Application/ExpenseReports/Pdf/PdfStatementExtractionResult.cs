namespace ExpenseFlow.Application.ExpenseReports.Pdf;

public sealed record PdfStatementExtractionResult(
    string SourceName,
    string StatementShapeId,
    PdfExtractionStatus Status,
    IReadOnlyCollection<PdfExtractedTransactionRow> ExtractedRows,
    IReadOnlyCollection<PdfInvalidExtractedRow> InvalidRows,
    IReadOnlyCollection<PdfExtractionWarning> Warnings,
    IReadOnlyCollection<PdfExtractedStatementTotal> ExtractedStatementTotals,
    IReadOnlyCollection<PdfExtractedTextLine> ExtractedLines)
{
    public bool IsProcessable => Status is PdfExtractionStatus.Succeeded or PdfExtractionStatus.Partial;

    public static PdfStatementExtractionResult Unsupported(
        string sourceName,
        string statementShapeId,
        PdfExtractionStatus status,
        IReadOnlyCollection<PdfExtractionWarning> warnings)
    {
        if (status is PdfExtractionStatus.Succeeded or PdfExtractionStatus.Partial)
        {
            throw new ArgumentException("Unsupported extraction results must use an unsupported or failed status.", nameof(status));
        }

        return new(
            sourceName,
            statementShapeId,
            status,
            Array.Empty<PdfExtractedTransactionRow>(),
            Array.Empty<PdfInvalidExtractedRow>(),
            warnings,
            Array.Empty<PdfExtractedStatementTotal>(),
            Array.Empty<PdfExtractedTextLine>());
    }
}
