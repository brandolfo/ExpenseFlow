using ExpenseFlow.Domain.Transactions;
using ExpenseFlow.Domain.Validation;

namespace ExpenseFlow.Domain.ExpenseReports;

public sealed record ExpenseReport
{
    public ExpenseReport(
        ReportMetadata metadata,
        ExpenseReportTotals totals,
        ExpectedTotalValidationResult validationResult,
        IReadOnlyCollection<CategoryTotal> categoryTotals,
        IReadOnlyCollection<ExpenseTransaction> transactions,
        IReadOnlyCollection<ExpenseTransaction> reviewItems,
        IReadOnlyCollection<ExpenseTransaction> invalidRows,
        IReadOnlyCollection<ExpenseTransaction> excludedRows,
        ProcessingAuditSummary auditSummary,
        IReadOnlyCollection<AuditEntry>? auditEntries = null)
    {
        Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        Totals = totals ?? throw new ArgumentNullException(nameof(totals));
        ValidationResult = validationResult ?? throw new ArgumentNullException(nameof(validationResult));
        CategoryTotals = categoryTotals ?? throw new ArgumentNullException(nameof(categoryTotals));
        Transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
        ReviewItems = reviewItems ?? throw new ArgumentNullException(nameof(reviewItems));
        InvalidRows = invalidRows ?? throw new ArgumentNullException(nameof(invalidRows));
        ExcludedRows = excludedRows ?? throw new ArgumentNullException(nameof(excludedRows));
        AuditSummary = auditSummary ?? throw new ArgumentNullException(nameof(auditSummary));
        AuditEntries = auditEntries ?? AuditSummary.Entries;
    }

    public ExpenseReport(
        string sourceName,
        IReadOnlyCollection<ExpenseTransaction> transactions,
        ProcessingAuditSummary auditSummary,
        ExpectedTotalValidationStatus expectedTotalValidationStatus)
        : this(
            new ReportMetadata(sourceName, DateTimeOffset.UnixEpoch),
            new ExpenseReportTotals(null, 0m, 0m, 0m),
            new ExpectedTotalValidationResult(expectedTotalValidationStatus, null, 0m, null),
            Array.Empty<CategoryTotal>(),
            transactions,
            transactions.Where(transaction => transaction.Status != RowStatus.Invalid && transaction.RequiresReview).ToArray(),
            transactions.Where(transaction => transaction.Status == RowStatus.Invalid).ToArray(),
            transactions.Where(transaction => transaction.Status == RowStatus.ExcludedFromTotals).ToArray(),
            auditSummary)
    {
    }

    public ReportMetadata Metadata { get; }

    public string SourceName => Metadata.SourceName;

    public ExpenseReportTotals Totals { get; }

    public ExpectedTotalValidationResult ValidationResult { get; }

    public IReadOnlyCollection<CategoryTotal> CategoryTotals { get; }

    public IReadOnlyCollection<ExpenseTransaction> Transactions { get; }

    public IReadOnlyCollection<ExpenseTransaction> ReviewItems { get; }

    public IReadOnlyCollection<ExpenseTransaction> InvalidRows { get; }

    public IReadOnlyCollection<ExpenseTransaction> ExcludedRows { get; }

    public ProcessingAuditSummary AuditSummary { get; }

    public IReadOnlyCollection<AuditEntry> AuditEntries { get; }

    public ExpectedTotalValidationStatus ExpectedTotalValidationStatus => ValidationResult.Status;

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
