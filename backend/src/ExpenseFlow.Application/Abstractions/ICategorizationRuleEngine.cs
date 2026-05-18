using ExpenseFlow.Application.ExpenseReports.Parsing;
using ExpenseFlow.Domain.Transactions;

namespace ExpenseFlow.Application.Abstractions;

public interface ICategorizationRuleEngine
{
    IReadOnlyCollection<ExpenseTransaction> Categorize(
        IReadOnlyCollection<ParsedTransactionCandidate> candidates);
}
