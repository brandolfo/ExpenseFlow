using ExpenseFlow.Domain.Categorization;

namespace ExpenseFlow.Domain.ExpenseReports;

public sealed record CategoryTotal(
    ExpenseCategory Category,
    int TransactionCount,
    decimal Total);
