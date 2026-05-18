using ExpenseFlow.Application.ExpenseReports.Parsing;

namespace ExpenseFlow.Application.Abstractions;

public interface ITransactionFileParser
{
    Task<TransactionFileParseResult> ParseAsync(
        string csvText,
        string sourceName,
        CancellationToken cancellationToken = default);
}
