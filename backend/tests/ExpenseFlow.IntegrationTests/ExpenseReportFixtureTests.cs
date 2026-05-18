using ExpenseFlow.Application.ExpenseReports.Categorization;
using ExpenseFlow.Application.ExpenseReports.Reporting;
using ExpenseFlow.Domain.Categorization;
using ExpenseFlow.Domain.ExpenseReports;
using ExpenseFlow.Domain.Transactions;
using ExpenseFlow.Infrastructure.Parsing;

namespace ExpenseFlow.IntegrationTests;

public sealed class ExpenseReportFixtureTests
{
    private static readonly DateTimeOffset FixedGeneratedAt = new(2026, 5, 18, 12, 0, 0, TimeSpan.Zero);

    private readonly CsvTransactionFileParser _parser = new();
    private readonly SeedDeterministicCategorizationRuleEngine _categorizer = new();
    private readonly DeterministicExpenseReportGenerator _generator = new();

    [Fact]
    public async Task DemoMainGeneratesExpectedMvpReport()
    {
        var report = await GenerateReportAsync("demo-main.csv", expectedTotal: 258248m);

        Assert.Equal("demo-main.csv", report.SourceName);
        Assert.Equal(22, report.AuditSummary.Counts.SourceRows);
        Assert.Equal(19, report.AuditSummary.Counts.ValidRows);
        Assert.Equal(14, report.AuditSummary.Counts.CategorizedRows);
        Assert.Equal(6, report.AuditSummary.Counts.ReviewRequiredRows);
        Assert.Equal(3, report.AuditSummary.Counts.InvalidRows);
        Assert.Equal(2, report.AuditSummary.Counts.ExcludedFromTotalsRows);
        Assert.Equal(1, report.AuditSummary.Counts.PotentialDuplicateRows);
        Assert.Equal(258248m, report.Totals.ProcessedTotal);
        Assert.Equal(189149m, report.Totals.CategoryTotal);
        Assert.Equal(ExpectedTotalValidationStatus.Match, report.ExpectedTotalValidationStatus);
        Assert.Equal(0m, report.ValidationResult.Difference);
        Assert.Equal([11, 12, 13, 14, 15, 19], report.ReviewItems.Select(row => row.SourceRow.RowNumber).ToArray());
        Assert.Equal([20, 21, 22], report.InvalidRows.Select(row => row.SourceRow.RowNumber).ToArray());
        Assert.Equal([14, 15], report.ExcludedRows.Select(row => row.SourceRow.RowNumber).ToArray());
        Assert.True(report.AccountsForEverySourceRow());
        Assert.False(report.AuditSummary.AiUsed);
    }

    [Fact]
    public async Task DemoMainCategoryTotalsMatchExpectedSummary()
    {
        var report = await GenerateReportAsync("demo-main.csv", expectedTotal: 258248m);
        var totals = report.CategoryTotals.ToDictionary(total => total.Category, total => total.Total);

        Assert.Equal(54100m, totals[ExpenseCategory.Groceries]);
        Assert.Equal(4200m, totals[ExpenseCategory.RestaurantsAndCafes]);
        Assert.Equal(850m, totals[ExpenseCategory.Transport]);
        Assert.Equal(18700m, totals[ExpenseCategory.HousingAndUtilities]);
        Assert.Equal(12999m, totals[ExpenseCategory.SubscriptionsAndSoftware]);
        Assert.Equal(7600m, totals[ExpenseCategory.HealthAndPharmacy]);
        Assert.Equal(9200m, totals[ExpenseCategory.Education]);
        Assert.Equal(6500m, totals[ExpenseCategory.Entertainment]);
        Assert.Equal(42500m, totals[ExpenseCategory.Travel]);
        Assert.Equal(2500m, totals[ExpenseCategory.FeesAndTaxes]);
        Assert.Equal(30000m, totals[ExpenseCategory.Shopping]);
        Assert.Equal(189149m, report.CategoryTotals.Sum(total => total.Total));
    }

    [Fact]
    public async Task DemoHappyPathGeneratesSuccessfulReport()
    {
        var report = await GenerateReportAsync("demo-happy-path.csv", expectedTotal: 179349m);

        Assert.Equal(13, report.AuditSummary.Counts.SourceRows);
        Assert.Equal(13, report.AuditSummary.Counts.ValidRows);
        Assert.Equal(13, report.AuditSummary.Counts.CategorizedRows);
        Assert.Equal(0, report.AuditSummary.Counts.ReviewRequiredRows);
        Assert.Equal(0, report.AuditSummary.Counts.InvalidRows);
        Assert.Equal(0, report.AuditSummary.Counts.ExcludedFromTotalsRows);
        Assert.Equal(179349m, report.Totals.ProcessedTotal);
        Assert.Equal(179349m, report.Totals.CategoryTotal);
        Assert.Equal(ExpectedTotalValidationStatus.Match, report.ExpectedTotalValidationStatus);
        Assert.Empty(report.ReviewItems);
        Assert.Empty(report.InvalidRows);
    }

    [Fact]
    public async Task DemoInvalidRowsRemainVisibleAndExcludedFromTotals()
    {
        var report = await GenerateReportAsync("demo-invalid-rows.csv", expectedTotal: 34500m);

        Assert.Equal(4, report.AuditSummary.Counts.SourceRows);
        Assert.Equal(1, report.AuditSummary.Counts.ValidRows);
        Assert.Equal(3, report.AuditSummary.Counts.InvalidRows);
        Assert.Equal(34500m, report.Totals.ProcessedTotal);
        Assert.Equal(34500m, report.Totals.CategoryTotal);
        Assert.Equal(ExpectedTotalValidationStatus.Match, report.ExpectedTotalValidationStatus);
        Assert.Equal([1, 2, 3, 4], report.Transactions.Select(transaction => transaction.SourceRow.RowNumber).ToArray());
        Assert.Equal([2, 3, 4], report.InvalidRows.Select(transaction => transaction.SourceRow.RowNumber).ToArray());
        Assert.All(report.InvalidRows, row =>
        {
            Assert.False(row.IncludedInProcessedTotal);
            Assert.False(row.IncludedInCategoryTotals);
        });
        Assert.True(report.AccountsForEverySourceRow());
    }

    [Fact]
    public async Task DemoTotalMismatchProducesExpectedMismatchBehavior()
    {
        var report = await GenerateReportAsync("demo-total-mismatch.csv", expectedTotal: 260000m);

        Assert.Equal(258248m, report.Totals.ProcessedTotal);
        Assert.Equal(ExpectedTotalValidationStatus.Mismatch, report.ExpectedTotalValidationStatus);
        Assert.Equal(1752m, report.ValidationResult.Difference);
        Assert.Equal(189149m, report.Totals.CategoryTotal);
        Assert.NotEmpty(report.ReviewItems);
        Assert.NotEmpty(report.InvalidRows);
        Assert.Contains(report.AuditSummary.Messages, message => message == "No AI was used.");
    }

    [Fact]
    public async Task MissingExpectedTotalIsAllowed()
    {
        var report = await GenerateReportAsync("demo-main.csv", expectedTotal: null);

        Assert.Equal(258248m, report.Totals.ProcessedTotal);
        Assert.Equal(ExpectedTotalValidationStatus.NotProvided, report.ExpectedTotalValidationStatus);
        Assert.Null(report.ValidationResult.Difference);
    }

    private async Task<ExpenseReport> GenerateReportAsync(string fileName, decimal? expectedTotal)
    {
        var parseResult = await _parser.ParseAsync(await File.ReadAllTextAsync(GetFixturePath(fileName)), fileName);
        Assert.True(parseResult.IsSuccess);

        var transactions = _categorizer.Categorize(parseResult.ValidRows);

        return _generator.Generate(new ExpenseReportGenerationInput(
            parseResult.SourceName,
            parseResult.SourceRowCount,
            transactions,
            parseResult.InvalidRows,
            expectedTotal,
            FixedGeneratedAt));
    }

    private static string GetFixturePath(string fileName) =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "testdata", fileName));
}
