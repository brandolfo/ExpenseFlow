namespace ExpenseFlow.Application.ExpenseReports.Pdf;

public sealed record PdfExtractedStatementTotal(
    string Label,
    string RawAmount,
    decimal? Amount,
    string? CurrencyCode,
    int? SourcePage = null,
    string? EvidenceSnippet = null);
