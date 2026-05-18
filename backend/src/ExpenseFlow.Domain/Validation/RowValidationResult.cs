namespace ExpenseFlow.Domain.Validation;

public sealed record RowValidationResult
{
    public RowValidationResult(ValidationStatus status, IReadOnlyCollection<ValidationIssue>? issues = null)
    {
        Status = status;
        Issues = issues ?? Array.Empty<ValidationIssue>();

        if (Status == ValidationStatus.Valid && Issues.Count > 0)
        {
            throw new ArgumentException("Valid rows cannot include validation issues.", nameof(issues));
        }
    }

    public ValidationStatus Status { get; }

    public IReadOnlyCollection<ValidationIssue> Issues { get; }
}
