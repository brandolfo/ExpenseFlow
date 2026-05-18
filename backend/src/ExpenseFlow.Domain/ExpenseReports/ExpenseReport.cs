using ExpenseFlow.Domain.Transactions;
using ExpenseFlow.Domain.Validation;

namespace ExpenseFlow.Domain.ExpenseReports;

public sealed record ExpenseReport
{
    public ExpenseReport(
        string sourceName,
        IReadOnlyCollection<ExpenseTransaction> transactions,
        ProcessingAuditSummary auditSummary,
        ExpectedTotalValidationStatus expectedTotalValidationStatus)
    {
        if (string.IsNullOrWhiteSpace(sourceName))
        {
            throw new ArgumentException("Source name is required.", nameof(sourceName));
        }

        SourceName = sourceName;
        Transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
        AuditSummary = auditSummary ?? throw new ArgumentNullException(nameof(auditSummary));
        ExpectedTotalValidationStatus = expectedTotalValidationStatus;
    }

    public string SourceName { get; }

    public IReadOnlyCollection<ExpenseTransaction> Transactions { get; }

    public ProcessingAuditSummary AuditSummary { get; }

    public ExpectedTotalValidationStatus ExpectedTotalValidationStatus { get; }

    public bool AccountsForEverySourceRow()
    {
        if (AuditSummary.Counts.SourceRows < 0)
        {
            return false;
        }

        var expectedRows = Enumerable.Range(1, AuditSummary.Counts.SourceRows);
        var visibleRows = Transactions.Select(transaction => transaction.SourceRow.RowNumber).Distinct();

        return expectedRows.SequenceEqual(visibleRows.Order());
    }

    public ValidationStatus CompletenessStatus =>
        AccountsForEverySourceRow() ? ValidationStatus.Valid : ValidationStatus.Invalid;
}
