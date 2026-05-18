namespace ExpenseFlow.Domain.ExpenseReports;

public enum AuditEventType
{
    RuleApplied,
    ReviewRequired,
    RowInvalid,
    RowExcludedFromTotals,
    PotentialDuplicate,
    TotalValidation
}
