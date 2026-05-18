using ExpenseFlow.Domain.Categorization;
using ExpenseFlow.Domain.ExpenseReports;
using ExpenseFlow.Domain.Transactions;
using ExpenseFlow.Domain.Validation;

namespace ExpenseFlow.UnitTests;

public sealed class DomainModelTests
{
    [Fact]
    public void SourceRowPreservesOriginalRowNumberAndRawValues()
    {
        var row = DemoRow(rowNumber: 16, installment: "03/06");

        Assert.Equal(16, row.RowNumber);
        Assert.Equal("DMO-0016", row.RawCode);
        Assert.Equal("PIXELGROVE ELECTRONICS DEMO", row.RawDescription);
        Assert.Equal("15000.00", row.RawAmount);
        Assert.Equal("03/06", row.RawInstallment);
    }

    [Fact]
    public void InvalidReviewAndExcludedStatusesCanBeRepresentedSeparately()
    {
        var invalid = new ExpenseTransaction(
            DemoRow(rowNumber: 20, description: "", amount: "7000.00"),
            RowStatus.Invalid,
            category: null,
            ReviewReason.MissingDescription,
            includedInProcessedTotal: false,
            includedInCategoryTotals: false,
            validationIssues: [new ValidationIssue(ReviewReason.MissingDescription, "description is required")]);

        var review = new ExpenseTransaction(
            DemoRow(rowNumber: 11, description: "MARKETBOX DEMO ORDER 8842", amount: "42999.00"),
            RowStatus.ReviewRequired,
            category: null,
            ReviewReason.UnknownMarketplace,
            includedInProcessedTotal: true,
            includedInCategoryTotals: false);

        var excluded = new ExpenseTransaction(
            DemoRow(rowNumber: 14, description: "REFUND DEMO STORE", amount: "-12000.00", sourceType: "refund"),
            RowStatus.ExcludedFromTotals,
            category: null,
            ReviewReason.RefundLikeNegativeAmount,
            includedInProcessedTotal: false,
            includedInCategoryTotals: false);

        Assert.True(invalid.RequiresReview);
        Assert.True(review.RequiresReview);
        Assert.True(excluded.RequiresReview);
        Assert.NotEqual(invalid.Status, excluded.Status);
    }

    [Fact]
    public void CategoryTaxonomyIsLimitedToDocumentedCategories()
    {
        var categories = Enum.GetValues<ExpenseCategory>();

        Assert.Equal(14, categories.Length);
        Assert.Contains(ExpenseCategory.Groceries, categories);
        Assert.Contains(ExpenseCategory.RestaurantsAndCafes, categories);
        Assert.Contains(ExpenseCategory.Transport, categories);
        Assert.Contains(ExpenseCategory.HousingAndUtilities, categories);
        Assert.Contains(ExpenseCategory.SubscriptionsAndSoftware, categories);
        Assert.Contains(ExpenseCategory.HealthAndPharmacy, categories);
        Assert.Contains(ExpenseCategory.Shopping, categories);
        Assert.Contains(ExpenseCategory.Entertainment, categories);
        Assert.Contains(ExpenseCategory.Education, categories);
        Assert.Contains(ExpenseCategory.Travel, categories);
        Assert.Contains(ExpenseCategory.FeesAndTaxes, categories);
        Assert.Contains(ExpenseCategory.IncomeRefundsAndAdjustments, categories);
        Assert.Contains(ExpenseCategory.TransfersAndPayments, categories);
        Assert.Contains(ExpenseCategory.UncategorizedReview, categories);
    }

    [Fact]
    public void DuplicateLookingRowsCanBeMarkedWithoutRemovingThem()
    {
        var row18 = CategorizedTransaction(DemoRow(rowNumber: 18), ExpenseCategory.Groceries);
        var row19 = CategorizedTransaction(DemoRow(rowNumber: 19)).MarkPotentialDuplicate();
        var report = DemoReport([row18, row19], sourceRows: 2);

        Assert.True(row19.IsPotentialDuplicate);
        Assert.True(row19.IncludedInProcessedTotal);
        Assert.True(row19.IncludedInCategoryTotals);
        Assert.Equal(2, report.Transactions.Count);
    }

    [Fact]
    public void InstallmentInformationIsPreservedWithoutGrouping()
    {
        var row16 = DemoRow(rowNumber: 16, installment: "03/06");
        var transaction = CategorizedTransaction(row16, ExpenseCategory.Shopping);

        Assert.Equal("03/06", transaction.Installment);
        Assert.Equal(16, transaction.SourceRow.RowNumber);
    }

    [Fact]
    public void ExpenseReportCanVerifyEverySourceRowHasAVisibleOutcome()
    {
        var transactions = new[]
        {
            CategorizedTransaction(DemoRow(rowNumber: 1), ExpenseCategory.Groceries),
            new ExpenseTransaction(
                DemoRow(rowNumber: 2, description: "PAYBRIDGE SERVICE DEMO 4821", amount: "18500.00"),
                RowStatus.ReviewRequired,
                category: null,
                ReviewReason.AmbiguousPaymentService,
                includedInProcessedTotal: true,
                includedInCategoryTotals: false),
            new ExpenseTransaction(
                DemoRow(rowNumber: 3, description: "", amount: "7000.00"),
                RowStatus.Invalid,
                category: null,
                ReviewReason.MissingDescription,
                includedInProcessedTotal: false,
                includedInCategoryTotals: false)
        };

        var report = DemoReport(transactions, sourceRows: 3);

        Assert.True(report.AccountsForEverySourceRow());
        Assert.Equal(ValidationStatus.Valid, report.CompletenessStatus);
    }

    [Fact]
    public void DeterministicMvpDomainDoesNotRequireAiTypes()
    {
        var domainTypes = typeof(ExpenseTransaction)
            .Assembly
            .GetTypes()
            .Where(type => type.Namespace?.StartsWith("ExpenseFlow.Domain", StringComparison.Ordinal) == true);

        Assert.DoesNotContain(domainTypes, type => type.Name.Contains("OpenAi", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(domainTypes, type => type.Name.Contains("AiSuggestion", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(domainTypes, type => type.Name.Contains("ArtificialIntelligence", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(domainTypes, type => type.Name.Contains("Agent", StringComparison.OrdinalIgnoreCase));
    }

    private static TransactionSourceRow DemoRow(
        int rowNumber,
        string date = "2026-04-16",
        string code = "DMO-0016",
        string description = "PIXELGROVE ELECTRONICS DEMO",
        string amount = "15000.00",
        string? installment = null,
        string sourceType = "purchase",
        string? notes = "Synthetic test row") =>
        new(rowNumber, date, code, description, amount, installment, sourceType, notes);

    private static ExpenseTransaction CategorizedTransaction(
        TransactionSourceRow row,
        ExpenseCategory category = ExpenseCategory.Groceries) =>
        new(
            row,
            RowStatus.Categorized,
            category,
            reviewReason: null,
            includedInProcessedTotal: true,
            includedInCategoryTotals: true);

    private static ExpenseReport DemoReport(IReadOnlyCollection<ExpenseTransaction> transactions, int sourceRows) =>
        new(
            "demo.csv",
            transactions,
            new ProcessingAuditSummary(
                new ProcessingCounts(
                    SourceRows: sourceRows,
                    ValidRows: transactions.Count(transaction => transaction.Status != RowStatus.Invalid),
                    CategorizedRows: transactions.Count(transaction => transaction.Status == RowStatus.Categorized),
                    ReviewRequiredRows: transactions.Count(transaction => transaction.RequiresReview),
                    InvalidRows: transactions.Count(transaction => transaction.Status == RowStatus.Invalid),
                    ExcludedFromTotalsRows: transactions.Count(transaction => transaction.Status == RowStatus.ExcludedFromTotals),
                    PotentialDuplicateRows: transactions.Count(transaction => transaction.IsPotentialDuplicate)),
                ValidationStatus.Valid),
            ExpectedTotalValidationStatus.NotProvided);
}
