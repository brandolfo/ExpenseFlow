using System.Globalization;
using ExpenseFlow.Application.ExpenseReports.Categorization;
using ExpenseFlow.Application.ExpenseReports.Parsing;
using ExpenseFlow.Domain.Categorization;
using ExpenseFlow.Domain.Transactions;
using CategorizationMatchType = ExpenseFlow.Domain.Categorization.MatchType;

namespace ExpenseFlow.UnitTests;

public sealed class SeedDeterministicCategorizationRuleEngineTests
{
    private readonly SeedDeterministicCategorizationRuleEngine _engine = new();

    [Fact]
    public void KnownMerchantExactPatternCategorizesAndPreservesRuleInformation()
    {
        var transaction = Categorize(Row(1, "FRESHVALE MARKET", 34500m));

        Assert.Equal(RowStatus.Categorized, transaction.Status);
        Assert.Equal(ExpenseCategory.Groceries, transaction.Category);
        Assert.False(transaction.RequiresReview);
        Assert.True(transaction.IncludedInProcessedTotal);
        Assert.True(transaction.IncludedInCategoryTotals);
        Assert.Contains(
            transaction.RuleMatches,
            match => match.RuleId == "R001" &&
                match.MatchType == CategorizationMatchType.KnownMerchant &&
                match.Pattern == "FRESHVALE MARKET" &&
                match.ResultingCategory == ExpenseCategory.Groceries);
    }

    [Fact]
    public void KnownMerchantContainsPatternCategorizes()
    {
        var transaction = Categorize(Row(2, "LOOPBEAN CAFE DEMO", 4200m));

        Assert.Equal(RowStatus.Categorized, transaction.Status);
        Assert.Equal(ExpenseCategory.RestaurantsAndCafes, transaction.Category);
        Assert.Contains(transaction.RuleMatches, match => match.RuleId == "R002");
    }

    [Fact]
    public void DescriptionKeywordContainsPatternCategorizes()
    {
        var transaction = Categorize(Row(4, "HOMEGRID UTILITY DEMO", 18700m));

        Assert.Equal(RowStatus.Categorized, transaction.Status);
        Assert.Equal(ExpenseCategory.HousingAndUtilities, transaction.Category);
        Assert.Contains(transaction.RuleMatches, match => match.RuleId == "R004" && match.Pattern == "HOMEGRID or UTILITY");
    }

    [Fact]
    public void ExclusionRulesRunBeforeCategoryRules()
    {
        var transaction = Categorize(Row(14, "CLOUDNEST REFUND DEMO", -12000m, sourceType: "refund"));

        Assert.Equal(RowStatus.ExcludedFromTotals, transaction.Status);
        Assert.Null(transaction.Category);
        Assert.Equal(ReviewReason.RefundLikeNegativeAmount, transaction.ReviewReason);
        Assert.False(transaction.IncludedInProcessedTotal);
        Assert.False(transaction.IncludedInCategoryTotals);
        Assert.Equal(["R013"], transaction.RuleMatches.Select(match => match.RuleId).ToArray());
    }

    [Fact]
    public void UnknownMarketplaceRequiresReviewWithoutGuessingCategory()
    {
        var transaction = Categorize(Row(11, "MARKETBOX DEMO ORDER 8842", 42999m));

        Assert.Equal(RowStatus.ReviewRequired, transaction.Status);
        Assert.Null(transaction.Category);
        Assert.Equal(ReviewReason.UnknownMarketplace, transaction.ReviewReason);
        Assert.True(transaction.IncludedInProcessedTotal);
        Assert.False(transaction.IncludedInCategoryTotals);
        Assert.Contains(transaction.RuleMatches, match => match.RuleId == "R011");
    }

    [Fact]
    public void AmbiguousPaymentServiceRequiresReviewWithoutExclusion()
    {
        var transaction = Categorize(Row(12, "PAYBRIDGE SERVICE DEMO 4821", 18500m));

        Assert.Equal(RowStatus.ReviewRequired, transaction.Status);
        Assert.Null(transaction.Category);
        Assert.Equal(ReviewReason.AmbiguousPaymentService, transaction.ReviewReason);
        Assert.True(transaction.IncludedInProcessedTotal);
        Assert.False(transaction.IncludedInCategoryTotals);
        Assert.Contains(transaction.RuleMatches, match => match.RuleId == "R011");
    }

    [Fact]
    public void CategoryConflictRequiresReviewAndRecordsConflictingRules()
    {
        var transaction = Categorize(Row(13, "WELLSPRING CAFE PHARMACY DEMO", 7600m));

        Assert.Equal(RowStatus.ReviewRequired, transaction.Status);
        Assert.Null(transaction.Category);
        Assert.Equal(ReviewReason.CategoryConflict, transaction.ReviewReason);
        Assert.True(transaction.IncludedInProcessedTotal);
        Assert.False(transaction.IncludedInCategoryTotals);
        Assert.Equal(["R006", "R012"], transaction.ConflictingRuleMatches.Select(match => match.RuleId).Order().ToArray());
    }

    [Fact]
    public void RefundLikeNegativeAmountIsVisibleAndExcluded()
    {
        var transaction = Categorize(Row(14, "REFUND DEMO STORE", -12000m, sourceType: "refund"));

        Assert.Equal(RowStatus.ExcludedFromTotals, transaction.Status);
        Assert.Equal(ReviewReason.RefundLikeNegativeAmount, transaction.ReviewReason);
        Assert.False(transaction.IncludedInProcessedTotal);
        Assert.False(transaction.IncludedInCategoryTotals);
        Assert.Contains(transaction.RuleMatches, match => match.RuleId == "R013" && match.MatchType == CategorizationMatchType.RefundDetector);
    }

    [Fact]
    public void TransferOrPaymentLikeRowIsVisibleAndExcluded()
    {
        var transaction = Categorize(Row(15, "TRANSFER DEMO TO WALLET", 30000m, sourceType: "transfer"));

        Assert.Equal(RowStatus.ExcludedFromTotals, transaction.Status);
        Assert.Equal(ReviewReason.TransferOrPayment, transaction.ReviewReason);
        Assert.False(transaction.IncludedInProcessedTotal);
        Assert.False(transaction.IncludedInCategoryTotals);
        Assert.Contains(transaction.RuleMatches, match => match.RuleId == "R014" && match.MatchType == CategorizationMatchType.TransferDetector);
    }

    [Fact]
    public void DuplicateLookingRowsAreFlaggedButNotRemoved()
    {
        var transactions = _engine.Categorize(
        [
            Row(18, "PANTRYVALE EXPRESS DEMO", 9800m, date: new DateOnly(2026, 4, 18)),
            Row(19, "PANTRYVALE EXPRESS DEMO", 9800m, date: new DateOnly(2026, 4, 18))
        ]).OrderBy(transaction => transaction.SourceRow.RowNumber).ToArray();

        Assert.Equal(2, transactions.Length);
        Assert.False(transactions[0].IsPotentialDuplicate);
        Assert.True(transactions[1].IsPotentialDuplicate);
        Assert.Equal(RowStatus.Categorized, transactions[1].Status);
        Assert.Equal(ExpenseCategory.Groceries, transactions[1].Category);
        Assert.Equal(ReviewReason.PotentialDuplicate, transactions[1].ReviewReason);
        Assert.True(transactions[1].RequiresReview);
        Assert.True(transactions[1].IncludedInProcessedTotal);
        Assert.True(transactions[1].IncludedInCategoryTotals);
        Assert.Contains(transactions[1].RuleMatches, match => match.RuleId == "R017");
    }

    [Fact]
    public void InstallmentsArePreservedAndNotGrouped()
    {
        var transactions = _engine.Categorize(
        [
            Row(16, "PIXELGROVE ELECTRONICS DEMO", 15000m, installment: "03/06"),
            Row(17, "PIXELGROVE ELECTRONICS DEMO", 15000m, installment: "04/06")
        ]).OrderBy(transaction => transaction.SourceRow.RowNumber).ToArray();

        Assert.Equal(2, transactions.Length);
        Assert.All(transactions, transaction => Assert.Equal(RowStatus.Categorized, transaction.Status));
        Assert.Equal("03/06", transactions[0].Installment);
        Assert.Equal("04/06", transactions[1].Installment);
        Assert.All(transactions, transaction => Assert.Contains(transaction.RuleMatches, match => match.RuleId == "R015"));
    }

    [Fact]
    public void CategorizationPreservesParsedAndRawValues()
    {
        var candidate = Row(16, "PIXELGROVE ELECTRONICS DEMO", 15000m, installment: "03/06");
        var transaction = Categorize(candidate);

        Assert.Equal(candidate.Date, transaction.Date);
        Assert.Equal(candidate.Description, transaction.Description);
        Assert.Equal(candidate.Amount, transaction.Amount);
        Assert.Equal("15000.00", transaction.SourceRow.RawAmount);
        Assert.Equal("03/06", transaction.SourceRow.RawInstallment);
    }

    [Fact]
    public void CategorizationDoesNotRequireAiOrOwnReportGeneration()
    {
        var engineMembers = typeof(SeedDeterministicCategorizationRuleEngine)
            .GetMembers()
            .Select(member => member.Name);

        Assert.DoesNotContain(engineMembers, member => member.Contains("Ai", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(engineMembers, member => member.Contains("OpenAi", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(engineMembers, member => member.Contains("Total", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(engineMembers, member => member.Contains("Report", StringComparison.OrdinalIgnoreCase));
    }

    private ExpenseTransaction Categorize(ParsedTransactionCandidate candidate) =>
        Assert.Single(_engine.Categorize([candidate]));

    private static ParsedTransactionCandidate Row(
        int rowNumber,
        string description,
        decimal amount,
        DateOnly? date = null,
        string? installment = null,
        string sourceType = "purchase") =>
        new(
            new TransactionSourceRow(
                rowNumber,
                (date ?? new DateOnly(2026, 4, rowNumber)).ToString("yyyy-MM-dd"),
                $"DMO-{rowNumber:0000}",
                description,
                amount.ToString("0.00", CultureInfo.InvariantCulture),
                installment,
                sourceType,
                "Synthetic test row"),
            date ?? new DateOnly(2026, 4, rowNumber),
            description,
            amount);
}
