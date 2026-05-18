using ExpenseFlow.Application.ExpenseReports.Parsing;
using ExpenseFlow.Domain.Transactions;
using ExpenseFlow.Infrastructure.Parsing;

namespace ExpenseFlow.IntegrationTests;

public sealed class CsvTransactionFileParserTests
{
    private readonly CsvTransactionFileParser _parser = new();

    [Fact]
    public async Task DemoMainParsesWithAllSourceRowsAccountedFor()
    {
        var result = await ParseFixtureAsync("demo-main.csv");

        Assert.True(result.IsSuccess);
        Assert.Equal(22, result.SourceRowCount);
        Assert.Equal(19, result.ValidRows.Count);
        Assert.Equal(3, result.InvalidRows.Count);
        Assert.Equal([20, 21, 22], result.InvalidRows.Select(row => row.SourceRow.RowNumber).ToArray());
    }

    [Fact]
    public async Task DemoHappyPathParsesAllRowsAsValid()
    {
        var result = await ParseFixtureAsync("demo-happy-path.csv");

        Assert.True(result.IsSuccess);
        Assert.Equal(13, result.SourceRowCount);
        Assert.Equal(13, result.ValidRows.Count);
        Assert.Empty(result.InvalidRows);
    }

    [Fact]
    public async Task DemoInvalidRowsReturnsDocumentedInvalidCases()
    {
        var result = await ParseFixtureAsync("demo-invalid-rows.csv");

        Assert.True(result.IsSuccess);
        Assert.Equal(4, result.SourceRowCount);
        Assert.Single(result.ValidRows);
        Assert.Equal(3, result.InvalidRows.Count);
        Assert.Contains(result.InvalidRows, row => HasReason(row, ReviewReason.MissingDescription));
        Assert.Contains(result.InvalidRows, row => HasReason(row, ReviewReason.InvalidDate));
        Assert.Contains(result.InvalidRows, row => HasReason(row, ReviewReason.InvalidAmount));
    }

    [Fact]
    public async Task DemoTotalMismatchParsesSameRowsAsDemoMain()
    {
        var main = await ParseFixtureAsync("demo-main.csv");
        var mismatch = await ParseFixtureAsync("demo-total-mismatch.csv");

        Assert.True(main.IsSuccess);
        Assert.True(mismatch.IsSuccess);
        Assert.Equal(main.SourceRowCount, mismatch.SourceRowCount);
        Assert.Equal(
            main.ValidRows.Select(row => row.SourceRow),
            mismatch.ValidRows.Select(row => row.SourceRow));
        Assert.Equal(
            main.InvalidRows.Select(row => row.SourceRow),
            mismatch.InvalidRows.Select(row => row.SourceRow));
    }

    [Fact]
    public async Task MissingRequiredColumnProducesFileLevelError()
    {
        const string csv = """
            date,description,code
            2026-04-01,FRESHVALE MARKET DEMO,DMO-0001
            """;

        var result = await _parser.ParseAsync(csv, "missing-required.csv");

        Assert.False(result.IsSuccess);
        Assert.Empty(result.ValidRows);
        Assert.Empty(result.InvalidRows);
        var error = Assert.Single(result.FileErrors);
        Assert.Equal(FileValidationErrorCode.MissingRequiredColumns, error.Code);
        Assert.Contains("amount", error.Details ?? []);
    }

    [Fact]
    public async Task MissingOptionalColumnsAreAllowed()
    {
        const string csv = """
            date,description,amount
            2026-04-01,FRESHVALE MARKET DEMO,34500.00
            """;

        var result = await _parser.ParseAsync(csv, "required-only.csv");

        Assert.True(result.IsSuccess);
        var row = Assert.Single(result.ValidRows);
        Assert.Equal(1, row.SourceRow.RowNumber);
        Assert.Null(row.SourceRow.RawCode);
        Assert.Null(row.SourceRow.RawInstallment);
        Assert.Null(row.SourceRow.RawSourceType);
        Assert.Null(row.SourceRow.RawNotes);
    }

    [Fact]
    public async Task EmptyCsvInputProducesFileLevelError()
    {
        var result = await _parser.ParseAsync("   ", "empty.csv");

        Assert.False(result.IsSuccess);
        var error = Assert.Single(result.FileErrors);
        Assert.Equal(FileValidationErrorCode.EmptyInput, error.Code);
    }

    [Fact]
    public async Task InvalidDateIsRepresentedAsInvalidRow()
    {
        var result = await _parser.ParseAsync(
            """
            date,description,amount
            2026-99-01,RIDEHILL TAXI DEMO,2300.00
            """,
            "invalid-date.csv");

        var invalidRow = Assert.Single(result.InvalidRows);
        Assert.Equal(1, invalidRow.SourceRow.RowNumber);
        Assert.True(HasReason(invalidRow, ReviewReason.InvalidDate));
    }

    [Fact]
    public async Task InvalidAmountIsRepresentedAsInvalidRow()
    {
        var result = await _parser.ParseAsync(
            """
            date,description,amount
            2026-04-21,RIDEHILL TAXI DEMO,abc
            """,
            "invalid-amount.csv");

        var invalidRow = Assert.Single(result.InvalidRows);
        Assert.Equal("abc", invalidRow.SourceRow.RawAmount);
        Assert.True(HasReason(invalidRow, ReviewReason.InvalidAmount));
    }

    [Fact]
    public async Task EmptyDescriptionIsRepresentedAsInvalidRow()
    {
        var result = await _parser.ParseAsync(
            """
            date,description,amount
            2026-04-19,,7000.00
            """,
            "empty-description.csv");

        var invalidRow = Assert.Single(result.InvalidRows);
        Assert.Equal(string.Empty, invalidRow.SourceRow.RawDescription);
        Assert.True(HasReason(invalidRow, ReviewReason.MissingDescription));
    }

    [Fact]
    public async Task RawRowNumberAndRawValuesArePreserved()
    {
        var result = await ParseFixtureAsync("demo-main.csv");

        var installmentRow = Assert.Single(result.ValidRows, row => row.SourceRow.RowNumber == 16);
        Assert.Equal("2026-04-16", installmentRow.SourceRow.RawDate);
        Assert.Equal("DMO-0016", installmentRow.SourceRow.RawCode);
        Assert.Equal("PIXELGROVE ELECTRONICS DEMO", installmentRow.SourceRow.RawDescription);
        Assert.Equal("15000.00", installmentRow.SourceRow.RawAmount);
        Assert.Equal("03/06", installmentRow.SourceRow.RawInstallment);
        Assert.Equal("purchase", installmentRow.SourceRow.RawSourceType);
        Assert.Equal("Synthetic installment row for R015", installmentRow.SourceRow.RawNotes);

        var invalidAmountRow = Assert.Single(result.InvalidRows, row => row.SourceRow.RowNumber == 22);
        Assert.Equal("abc", invalidAmountRow.SourceRow.RawAmount);
    }

    [Fact]
    public async Task ParsingDoesNotPerformCategorizationOrTotalCalculation()
    {
        var result = await ParseFixtureAsync("demo-main.csv");

        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(
            typeof(ParsedTransactionCandidate).GetProperties(),
            property => property.Name.Contains("Category", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(
            typeof(TransactionFileParseResult).GetProperties(),
            property => property.Name.Contains("ProcessedTotal", StringComparison.OrdinalIgnoreCase) ||
                property.Name.Contains("CategoryTotal", StringComparison.OrdinalIgnoreCase) ||
                property.Name.Contains("ExpectedTotal", StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasReason(InvalidTransactionRow row, ReviewReason reason) =>
        row.ValidationIssues.Any(issue => issue.Reason == reason);

    private Task<TransactionFileParseResult> ParseFixtureAsync(string fileName) =>
        _parser.ParseAsync(File.ReadAllText(GetFixturePath(fileName)), fileName);

    private static string GetFixturePath(string fileName) =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "testdata", fileName));
}
