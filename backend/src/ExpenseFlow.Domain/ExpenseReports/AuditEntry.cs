namespace ExpenseFlow.Domain.ExpenseReports;

public sealed record AuditEntry
{
    public AuditEntry(int sourceRowNumber, AuditEventType eventType, string message, string? ruleId = null)
    {
        if (sourceRowNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sourceRowNumber), "Audit entries must reference a positive source row number.");
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Audit message is required.", nameof(message));
        }

        SourceRowNumber = sourceRowNumber;
        EventType = eventType;
        Message = message;
        RuleId = ruleId;
    }

    public int SourceRowNumber { get; }

    public AuditEventType EventType { get; }

    public string Message { get; }

    public string? RuleId { get; }
}
