namespace ExpenseFlow.Application.ExpenseReports.Pdf;

public sealed record PdfExtractionWarning(
    string Code,
    string Message,
    int? SourcePage = null,
    int? ExtractionOrder = null);
