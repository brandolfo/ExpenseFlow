using ExpenseFlow.Application.ExpenseReports.Parsing;
using ExpenseFlow.Domain.Transactions;

namespace ExpenseFlow.Application.ExpenseReports.Reporting;

public sealed record ExpenseReportGenerationInput(
    string SourceName,
    int SourceRowCount,
    IReadOnlyCollection<ExpenseTransaction> TransactionOutcomes,
    IReadOnlyCollection<InvalidTransactionRow> InvalidRows,
    decimal? ExpectedTotal = null,
    DateTimeOffset? GeneratedAtUtc = null);
