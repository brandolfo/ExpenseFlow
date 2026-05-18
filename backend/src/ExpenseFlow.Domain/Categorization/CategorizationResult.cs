using ExpenseFlow.Domain.Transactions;

namespace ExpenseFlow.Domain.Categorization;

public sealed record CategorizationResult
{
    public CategorizationResult(
        RowStatus status,
        ExpenseCategory? category,
        ReviewReason? reviewReason,
        IReadOnlyCollection<CategorizationRuleMatch>? ruleMatches = null)
    {
        if (status == RowStatus.Categorized && category is null)
        {
            throw new ArgumentException("Categorized results must include a category.", nameof(category));
        }

        Status = status;
        Category = category;
        ReviewReason = reviewReason;
        RuleMatches = ruleMatches ?? Array.Empty<CategorizationRuleMatch>();
    }

    public RowStatus Status { get; }

    public ExpenseCategory? Category { get; }

    public ReviewReason? ReviewReason { get; }

    public IReadOnlyCollection<CategorizationRuleMatch> RuleMatches { get; }
}
