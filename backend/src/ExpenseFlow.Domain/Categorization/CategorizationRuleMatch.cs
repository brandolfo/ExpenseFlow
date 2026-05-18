namespace ExpenseFlow.Domain.Categorization;

public sealed record CategorizationRuleMatch
{
    public CategorizationRuleMatch(
        string ruleId,
        MatchType matchType,
        ExpenseCategory? resultingCategory,
        int priority,
        string? pattern = null)
    {
        if (string.IsNullOrWhiteSpace(ruleId))
        {
            throw new ArgumentException("Rule id is required.", nameof(ruleId));
        }

        RuleId = ruleId;
        MatchType = matchType;
        ResultingCategory = resultingCategory;
        Priority = priority;
        Pattern = pattern;
    }

    public string RuleId { get; }

    public MatchType MatchType { get; }

    public ExpenseCategory? ResultingCategory { get; }

    public int Priority { get; }

    public string? Pattern { get; }
}
