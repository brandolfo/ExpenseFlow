namespace ExpenseFlow.Application.ExpenseReports.Pdf;

public sealed record PdfExtractedFieldValues(
    string? Date,
    string? Code,
    string? Description,
    string? Amount,
    string? Installment,
    string? SourceType,
    string? Notes,
    string? CurrencyCode = null);
