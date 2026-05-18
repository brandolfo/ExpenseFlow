using ExpenseFlow.Domain.Transactions;
using ExpenseFlow.Domain.Validation;

namespace ExpenseFlow.Application.ExpenseReports.Parsing;

public sealed record InvalidTransactionRow(
    TransactionSourceRow SourceRow,
    IReadOnlyCollection<ValidationIssue> ValidationIssues);
