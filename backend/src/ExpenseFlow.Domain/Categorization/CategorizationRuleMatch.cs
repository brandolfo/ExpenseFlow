namespace ExpenseFlow.Domain.Categorization;

public sealed record CategorizationRuleMatch
{
    public CategorizationRuleMatch(
        string ruleId,
        MatchType matchType,
        ExpenseCategory? resultingCategory,
        int priority)
    {
        if (string.IsNullOrWhiteSpace(ruleId))
        {
            throw new ArgumentException("Rule id is required.", nameof(ruleId));
        }

        RuleId = ruleId;
        MatchType = matchType;
        ResultingCategory = resultingCategory;
        Priority = priority;
    }

    public string RuleId { get; }

    public MatchType MatchType { get; }

    public ExpenseCategory? ResultingCategory { get; }

    public int Priority { get; }
}
