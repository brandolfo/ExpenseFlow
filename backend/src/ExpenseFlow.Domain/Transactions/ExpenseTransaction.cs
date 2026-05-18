using ExpenseFlow.Domain.Categorization;
using ExpenseFlow.Domain.Validation;

namespace ExpenseFlow.Domain.Transactions;

public sealed record ExpenseTransaction
{
    public ExpenseTransaction(
        TransactionSourceRow sourceRow,
        RowStatus status,
        ExpenseCategory? category,
        ReviewReason? reviewReason,
        bool includedInProcessedTotal,
        bool includedInCategoryTotals,
        bool isPotentialDuplicate = false,
        string? installment = null,
        IReadOnlyCollection<ValidationIssue>? validationIssues = null)
        : this(
            sourceRow,
            date: null,
            description: null,
            amount: null,
            status,
            category,
            reviewReason,
            includedInProcessedTotal,
            includedInCategoryTotals,
            isPotentialDuplicate,
            installment,
            validationIssues,
            ruleMatches: null,
            conflictingRuleMatches: null)
    {
    }

    public ExpenseTransaction(
        TransactionSourceRow sourceRow,
        DateOnly? date,
        string? description,
        decimal? amount,
        RowStatus status,
        ExpenseCategory? category,
        ReviewReason? reviewReason,
        bool includedInProcessedTotal,
        bool includedInCategoryTotals,
        bool isPotentialDuplicate = false,
        string? installment = null,
        IReadOnlyCollection<ValidationIssue>? validationIssues = null,
        IReadOnlyCollection<CategorizationRuleMatch>? ruleMatches = null,
        IReadOnlyCollection<CategorizationRuleMatch>? conflictingRuleMatches = null)
    {
        SourceRow = sourceRow ?? throw new ArgumentNullException(nameof(sourceRow));
        Date = date;
        Description = description;
        Amount = amount;
        Status = status;
        Category = category;
        ReviewReason = reviewReason;
        IncludedInProcessedTotal = includedInProcessedTotal;
        IncludedInCategoryTotals = includedInCategoryTotals;
        IsPotentialDuplicate = isPotentialDuplicate;
        Installment = installment ?? sourceRow.RawInstallment;
        ValidationIssues = validationIssues ?? Array.Empty<ValidationIssue>();
        RuleMatches = ruleMatches ?? Array.Empty<CategorizationRuleMatch>();
        ConflictingRuleMatches = conflictingRuleMatches ?? Array.Empty<CategorizationRuleMatch>();

        ValidateState();
    }

    public TransactionSourceRow SourceRow { get; }

    public DateOnly? Date { get; }

    public string? Description { get; }

    public decimal? Amount { get; }

    public RowStatus Status { get; }

    public ExpenseCategory? Category { get; }

    public ReviewReason? ReviewReason { get; }

    public bool IncludedInProcessedTotal { get; }

    public bool IncludedInCategoryTotals { get; }

    public bool IsPotentialDuplicate { get; }

    public string? Installment { get; }

    public IReadOnlyCollection<ValidationIssue> ValidationIssues { get; }

    public IReadOnlyCollection<CategorizationRuleMatch> RuleMatches { get; }

    public IReadOnlyCollection<CategorizationRuleMatch> ConflictingRuleMatches { get; }

    public bool RequiresReview =>
        Status is RowStatus.ReviewRequired or RowStatus.Invalid or RowStatus.ExcludedFromTotals || IsPotentialDuplicate;

    public ExpenseTransaction MarkPotentialDuplicate() =>
        new(
            SourceRow,
            Date,
            Description,
            Amount,
            Status,
            Category,
            ReviewReason ?? Transactions.ReviewReason.PotentialDuplicate,
            IncludedInProcessedTotal,
            IncludedInCategoryTotals,
            isPotentialDuplicate: true,
            Installment,
            ValidationIssues,
            RuleMatches,
            ConflictingRuleMatches);

    private void ValidateState()
    {
        if (Status == RowStatus.Categorized && Category is null)
        {
            throw new ArgumentException("Categorized rows must have a category.");
        }

        if (IncludedInCategoryTotals && Category is null)
        {
            throw new ArgumentException("Rows included in category totals must have a category.");
        }

        if (Status is RowStatus.Invalid or RowStatus.ExcludedFromTotals &&
            (IncludedInProcessedTotal || IncludedInCategoryTotals))
        {
            throw new ArgumentException("Invalid or excluded rows cannot be included in totals.");
        }
    }
}
