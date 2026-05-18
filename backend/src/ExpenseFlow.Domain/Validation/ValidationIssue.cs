using ExpenseFlow.Domain.Transactions;

namespace ExpenseFlow.Domain.Validation;

public sealed record ValidationIssue(ReviewReason Reason, string Message);
