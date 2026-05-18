namespace ExpenseFlow.Domain.ExpenseReports;

public sealed record ExpectedTotalValidationResult(
    ExpectedTotalValidationStatus Status,
    decimal? ExpectedTotal,
    decimal ProcessedTotal,
    decimal? Difference);
