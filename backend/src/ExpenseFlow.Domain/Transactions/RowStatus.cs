namespace ExpenseFlow.Domain.Transactions;

public enum RowStatus
{
    Valid,
    Categorized,
    ReviewRequired,
    Invalid,
    ExcludedFromTotals
}
