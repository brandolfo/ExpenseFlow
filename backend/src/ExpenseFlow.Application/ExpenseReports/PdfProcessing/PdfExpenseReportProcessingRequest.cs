namespace ExpenseFlow.Application.ExpenseReports.PdfProcessing;

public sealed record PdfExpenseReportProcessingRequest(
    string SourceName,
    byte[] PdfContent,
    decimal? ExpectedTotal = null,
    string? StatementShapeHint = null);
