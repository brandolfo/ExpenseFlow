using ExpenseFlow.Domain.Validation;

namespace ExpenseFlow.Domain.ExpenseReports;

public sealed record ProcessingAuditSummary
{
    public ProcessingAuditSummary(
        ProcessingCounts counts,
        ValidationStatus completenessStatus,
        IReadOnlyCollection<AuditEntry>? entries = null,
        IReadOnlyCollection<string>? messages = null,
        int appliedDeterministicRuleCount = 0,
        ExpectedTotalValidationStatus expectedTotalValidationStatus = ExpectedTotalValidationStatus.NotProvided,
        bool aiUsed = false)
    {
        Counts = counts;
        CompletenessStatus = completenessStatus;
        Entries = entries ?? Array.Empty<AuditEntry>();
        Messages = messages ?? Array.Empty<string>();
        AppliedDeterministicRuleCount = appliedDeterministicRuleCount;
        ExpectedTotalValidationStatus = expectedTotalValidationStatus;
        AiUsed = aiUsed;
    }

    public ProcessingCounts Counts { get; }

    public ValidationStatus CompletenessStatus { get; }

    public IReadOnlyCollection<AuditEntry> Entries { get; }

    public IReadOnlyCollection<string> Messages { get; }

    public int AppliedDeterministicRuleCount { get; }

    public ExpectedTotalValidationStatus ExpectedTotalValidationStatus { get; }

    public bool AiUsed { get; }
}
