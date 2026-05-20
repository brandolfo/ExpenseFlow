namespace ExpenseFlow.Application.ExpenseReports.Pdf;

public sealed record PdfExtractedTextLine(
    string SourceName,
    string StatementShapeId,
    string? StatementShapeHint,
    int SourcePage,
    int ExtractionOrder,
    string Text);
