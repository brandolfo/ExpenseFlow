namespace ExpenseFlow.Application.ExpenseReports.Pdf;

public sealed record PdfForeignCurrencyEvidence(
    string CurrencyCode,
    string RawAmount,
    decimal? Amount);
