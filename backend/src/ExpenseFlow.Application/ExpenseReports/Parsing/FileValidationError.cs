namespace ExpenseFlow.Application.ExpenseReports.Parsing;

public sealed record FileValidationError(
    FileValidationErrorCode Code,
    string Message,
    IReadOnlyCollection<string>? Details = null);
