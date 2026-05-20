namespace ExpenseFlow.Application.ExpenseReports.Pdf;

public sealed record PdfInvalidExtractedRow(
    int SourcePage,
    int ExtractionOrder,
    string? EvidenceSnippet,
    PdfExtractedFieldValues RawFields,
    IReadOnlyCollection<PdfExtractionWarning> Warnings);
