namespace ExpenseFlow.Application.ExpenseReports.Parsing;

public sealed record TransactionFileParseResult
{
    private TransactionFileParseResult(
        string sourceName,
        int sourceRowCount,
        IReadOnlyCollection<ParsedTransactionCandidate> validRows,
        IReadOnlyCollection<InvalidTransactionRow> invalidRows,
        IReadOnlyCollection<FileValidationError> fileErrors)
    {
        SourceName = sourceName;
        SourceRowCount = sourceRowCount;
        ValidRows = validRows;
        InvalidRows = invalidRows;
        FileErrors = fileErrors;
    }

    public string SourceName { get; }

    public int SourceRowCount { get; }

    public IReadOnlyCollection<ParsedTransactionCandidate> ValidRows { get; }

    public IReadOnlyCollection<InvalidTransactionRow> InvalidRows { get; }

    public IReadOnlyCollection<FileValidationError> FileErrors { get; }

    public bool IsSuccess => FileErrors.Count == 0;

    public static TransactionFileParseResult Success(
        string sourceName,
        int sourceRowCount,
        IReadOnlyCollection<ParsedTransactionCandidate> validRows,
        IReadOnlyCollection<InvalidTransactionRow> invalidRows) =>
        new(sourceName, sourceRowCount, validRows, invalidRows, Array.Empty<FileValidationError>());

    public static TransactionFileParseResult Failure(
        string sourceName,
        IReadOnlyCollection<FileValidationError> fileErrors) =>
        new(sourceName, sourceRowCount: 0, Array.Empty<ParsedTransactionCandidate>(), Array.Empty<InvalidTransactionRow>(), fileErrors);
}
