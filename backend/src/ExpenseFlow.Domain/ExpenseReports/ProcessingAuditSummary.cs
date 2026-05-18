using ExpenseFlow.Domain.Validation;

namespace ExpenseFlow.Domain.ExpenseReports;

public sealed record ProcessingAuditSummary
{
    public ProcessingAuditSummary(
        ProcessingCounts counts,
        ValidationStatus completenessStatus,
        IReadOnlyCollection<AuditEntry>? entries = null,
        IReadOnlyCollection<string>? messages = null)
    {
        Counts = counts;
        CompletenessStatus = completenessStatus;
        Entries = entries ?? Array.Empty<AuditEntry>();
        Messages = messages ?? Array.Empty<string>();
    }

    public ProcessingCounts Counts { get; }

    public ValidationStatus CompletenessStatus { get; }

    public IReadOnlyCollection<AuditEntry> Entries { get; }

    public IReadOnlyCollection<string> Messages { get; }
}
