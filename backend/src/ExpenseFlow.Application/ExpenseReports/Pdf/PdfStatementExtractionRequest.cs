namespace ExpenseFlow.Application.ExpenseReports.Pdf;

public sealed record PdfStatementExtractionRequest(
    string SourceName,
    byte[] PdfContent,
    string? StatementShapeHint = null);
