namespace ExpenseFlow.Domain.ExpenseReports;

public sealed record ExpenseReportTotals(
    decimal? ExpectedTotal,
    decimal ProcessedTotal,
    decimal CategoryTotal,
    decimal ExcludedFromTotalsTotal);
