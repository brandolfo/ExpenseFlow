namespace ExpenseFlow.Domain.ExpenseReports;

public sealed record ProcessingCounts(
    int SourceRows,
    int ValidRows,
    int CategorizedRows,
    int ReviewRequiredRows,
    int InvalidRows,
    int ExcludedFromTotalsRows,
    int PotentialDuplicateRows);
