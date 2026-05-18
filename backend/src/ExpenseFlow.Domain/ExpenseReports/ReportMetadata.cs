namespace ExpenseFlow.Domain.ExpenseReports;

public sealed record ReportMetadata
{
    public ReportMetadata(
        string sourceName,
        DateTimeOffset generatedAtUtc,
        string product = "ExpenseFlow",
        string reportType = "mvp_expense_report",
        string inputFormat = "csv",
        bool usesRealFinancialData = false)
    {
        if (string.IsNullOrWhiteSpace(product))
        {
            throw new ArgumentException("Product is required.", nameof(product));
        }

        if (string.IsNullOrWhiteSpace(reportType))
        {
            throw new ArgumentException("Report type is required.", nameof(reportType));
        }

        if (string.IsNullOrWhiteSpace(inputFormat))
        {
            throw new ArgumentException("Input format is required.", nameof(inputFormat));
        }

        if (string.IsNullOrWhiteSpace(sourceName))
        {
            throw new ArgumentException("Source name is required.", nameof(sourceName));
        }

        Product = product;
        ReportType = reportType;
        InputFormat = inputFormat;
        SourceName = sourceName;
        GeneratedAtUtc = generatedAtUtc;
        UsesRealFinancialData = usesRealFinancialData;
    }

    public string Product { get; }

    public string ReportType { get; }

    public string InputFormat { get; }

    public string SourceName { get; }

    public DateTimeOffset GeneratedAtUtc { get; }

    public bool UsesRealFinancialData { get; }
}
