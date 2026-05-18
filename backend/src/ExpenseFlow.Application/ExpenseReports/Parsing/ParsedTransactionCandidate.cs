using ExpenseFlow.Domain.Transactions;

namespace ExpenseFlow.Application.ExpenseReports.Parsing;

public sealed record ParsedTransactionCandidate(
    TransactionSourceRow SourceRow,
    DateOnly Date,
    string Description,
    decimal Amount);
