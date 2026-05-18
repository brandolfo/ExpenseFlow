namespace ExpenseFlow.Application.ExpenseReports.Processing;

public sealed record ExpenseReportProcessingRequest(
    string SourceName,
    string CsvText,
    decimal? ExpectedTotal = null);
