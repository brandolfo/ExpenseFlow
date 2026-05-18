using ExpenseFlow.Application.ExpenseReports.Categorization;
using ExpenseFlow.Domain.Categorization;
using ExpenseFlow.Domain.Transactions;
using ExpenseFlow.Infrastructure.Parsing;

namespace ExpenseFlow.IntegrationTests;

public sealed class DeterministicCategorizationFixtureTests
{
    private readonly CsvTransactionFileParser _parser = new();
    private readonly SeedDeterministicCategorizationRuleEngine _engine = new();

    [Fact]
    public async Task DemoMainCategorizesValidRowsWithExpectedReviewExclusionAndDuplicateOutcomes()
    {
        var parseResult = await ParseFixtureAsync("demo-main.csv");
        var transactions = _engine.Categorize(parseResult.ValidRows);

        Assert.True(parseResult.IsSuccess);
        Assert.Equal(19, transactions.Count);
        Assert.Equal(14, transactions.Count(transaction => transaction.Status == RowStatus.Categorized));
        Assert.Equal(6, transactions.Count(transaction => transaction.RequiresReview));
        Assert.Equal(2, transactions.Count(transaction => transaction.Status == RowStatus.ExcludedFromTotals));
        Assert.Single(transactions, transaction => transaction.IsPotentialDuplicate);
        Assert.Equal(3, parseResult.InvalidRows.Count);

        AssertRow(transactions, 1, RowStatus.Categorized, ExpenseCategory.Groceries, reviewReason: null, includedInProcessedTotal: true, includedInCategoryTotals: true, ruleIds: ["R001"]);
        AssertRow(transactions, 11, RowStatus.ReviewRequired, category: null, ReviewReason.UnknownMarketplace, includedInProcessedTotal: true, includedInCategoryTotals: false, ruleIds: ["R011"]);
        AssertRow(transactions, 12, RowStatus.ReviewRequired, category: null, ReviewReason.AmbiguousPaymentService, includedInProcessedTotal: true, includedInCategoryTotals: false, ruleIds: ["R011"]);
        AssertRow(transactions, 13, RowStatus.ReviewRequired, category: null, ReviewReason.CategoryConflict, includedInProcessedTotal: true, includedInCategoryTotals: false, ruleIds: ["R006", "R012"]);
        AssertRow(transactions, 14, RowStatus.ExcludedFromTotals, category: null, ReviewReason.RefundLikeNegativeAmount, includedInProcessedTotal: false, includedInCategoryTotals: false, ruleIds: ["R013"]);
        AssertRow(transactions, 15, RowStatus.ExcludedFromTotals, category: null, ReviewReason.TransferOrPayment, includedInProcessedTotal: false, includedInCategoryTotals: false, ruleIds: ["R014"]);
        AssertRow(transactions, 19, RowStatus.Categorized, ExpenseCategory.Groceries, ReviewReason.PotentialDuplicate, includedInProcessedTotal: true, includedInCategoryTotals: true, isPotentialDuplicate: true, ruleIds: ["R016", "R017"]);
    }

    [Fact]
    public async Task DemoHappyPathCategorizesWithoutReviewInvalidOrExcludedRows()
    {
        var parseResult = await ParseFixtureAsync("demo-happy-path.csv");
        var transactions = _engine.Categorize(parseResult.ValidRows);

        Assert.True(parseResult.IsSuccess);
        Assert.Empty(parseResult.InvalidRows);
        Assert.Equal(13, transactions.Count);
        Assert.All(transactions, transaction => Assert.Equal(RowStatus.Categorized, transaction.Status));
        Assert.DoesNotContain(transactions, transaction => transaction.RequiresReview);
        Assert.DoesNotContain(transactions, transaction => transaction.Status == RowStatus.ExcludedFromTotals);
        Assert.DoesNotContain(transactions, transaction => transaction.IsPotentialDuplicate);
    }

    [Fact]
    public async Task DemoInvalidRowsOnlyCategorizesValidParsedRows()
    {
        var parseResult = await ParseFixtureAsync("demo-invalid-rows.csv");
        var transactions = _engine.Categorize(parseResult.ValidRows);

        Assert.True(parseResult.IsSuccess);
        Assert.Equal(3, parseResult.InvalidRows.Count);
        var transaction = Assert.Single(transactions);
        Assert.Equal(1, transaction.SourceRow.RowNumber);
        Assert.Equal(RowStatus.Categorized, transaction.Status);
        Assert.Equal(ExpenseCategory.Groceries, transaction.Category);
        Assert.DoesNotContain(transactions, transaction => transaction.SourceRow.RowNumber is 20 or 21 or 22);
    }

    [Fact]
    public async Task DemoTotalMismatchCategorizesSameRowsAsDemoMain()
    {
        var main = _engine.Categorize((await ParseFixtureAsync("demo-main.csv")).ValidRows);
        var mismatch = _engine.Categorize((await ParseFixtureAsync("demo-total-mismatch.csv")).ValidRows);

        Assert.Equal(
            main.Select(OutcomeSnapshot.Create).ToArray(),
            mismatch.Select(OutcomeSnapshot.Create).ToArray());
    }

    private static void AssertRow(
        IReadOnlyCollection<ExpenseTransaction> transactions,
        int rowNumber,
        RowStatus status,
        ExpenseCategory? category,
        ReviewReason? reviewReason,
        bool includedInProcessedTotal,
        bool includedInCategoryTotals,
        string[] ruleIds,
        bool isPotentialDuplicate = false)
    {
        var transaction = Assert.Single(transactions, transaction => transaction.SourceRow.RowNumber == rowNumber);

        Assert.Equal(status, transaction.Status);
        Assert.Equal(category, transaction.Category);
        Assert.Equal(reviewReason, transaction.ReviewReason);
        Assert.Equal(includedInProcessedTotal, transaction.IncludedInProcessedTotal);
        Assert.Equal(includedInCategoryTotals, transaction.IncludedInCategoryTotals);
        Assert.Equal(isPotentialDuplicate, transaction.IsPotentialDuplicate);
        Assert.Equal(ruleIds.Order().ToArray(), transaction.RuleMatches.Select(match => match.RuleId).Order().ToArray());
    }

    private async Task<ExpenseFlow.Application.ExpenseReports.Parsing.TransactionFileParseResult> ParseFixtureAsync(string fileName) =>
        await _parser.ParseAsync(await File.ReadAllTextAsync(GetFixturePath(fileName)), fileName);

    private static string GetFixturePath(string fileName) =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "testdata", fileName));

    private sealed record OutcomeSnapshot(
        int SourceRow,
        RowStatus Status,
        ExpenseCategory? Category,
        ReviewReason? ReviewReason,
        bool IncludedInProcessedTotal,
        bool IncludedInCategoryTotals,
        bool IsPotentialDuplicate,
        string RuleIds)
    {
        public static OutcomeSnapshot Create(ExpenseTransaction transaction) =>
            new(
                transaction.SourceRow.RowNumber,
                transaction.Status,
                transaction.Category,
                transaction.ReviewReason,
                transaction.IncludedInProcessedTotal,
                transaction.IncludedInCategoryTotals,
                transaction.IsPotentialDuplicate,
                string.Join(",", transaction.RuleMatches.Select(match => match.RuleId).Order()));
    }
}
