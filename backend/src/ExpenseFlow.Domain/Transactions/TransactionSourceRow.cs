namespace ExpenseFlow.Domain.Transactions;

public sealed record TransactionSourceRow
{
    public TransactionSourceRow(
        int rowNumber,
        string? rawDate,
        string? rawCode,
        string? rawDescription,
        string? rawAmount,
        string? rawInstallment,
        string? rawSourceType,
        string? rawNotes)
    {
        if (rowNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rowNumber), "Source row numbers must be positive.");
        }

        RowNumber = rowNumber;
        RawDate = rawDate;
        RawCode = rawCode;
        RawDescription = rawDescription;
        RawAmount = rawAmount;
        RawInstallment = rawInstallment;
        RawSourceType = rawSourceType;
        RawNotes = rawNotes;
    }

    public int RowNumber { get; }

    public string? RawDate { get; }

    public string? RawCode { get; }

    public string? RawDescription { get; }

    public string? RawAmount { get; }

    public string? RawInstallment { get; }

    public string? RawSourceType { get; }

    public string? RawNotes { get; }
}
