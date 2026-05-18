using System.Globalization;
using ExpenseFlow.Application.ExpenseReports.Reporting;
using ExpenseFlow.Application.ExpenseReports.Parsing;
using ExpenseFlow.Domain.Categorization;
using ExpenseFlow.Domain.ExpenseReports;
using ExpenseFlow.Domain.Transactions;
using ExpenseFlow.Domain.Validation;
using CategorizationMatchType = ExpenseFlow.Domain.Categorization.MatchType;

namespace ExpenseFlow.UnitTests;

public sealed class DeterministicExpenseReportGeneratorTests
{
    private static readonly DateTimeOffset FixedGeneratedAt = new(2026, 5, 18, 12, 0, 0, TimeSpan.Zero);

    private readonly DeterministicExpenseReportGenerator _generator = new();

    [Fact]
    public void GeneratesProcessedAndCategoryTotalsFromEligibleRowsOnly()
    {
        var report = Generate(
            [
                Categorized(1, "FRESHVALE MARKET DEMO", 100m, ExpenseCategory.Groceries, "R001"),
                Review(2, "MARKETBOX DEMO ORDER 8842", 50m, ReviewReason.UnknownMarketplace, "R011"),
                Excluded(3, "REFUND DEMO STORE", -10m, ReviewReason.RefundLikeNegativeAmount, "R013", sourceType: "refund"),
                Excluded(4, "TRANSFER DEMO TO WALLET", 30m, ReviewReason.TransferOrPayment, "R014", sourceType: "transfer"),
                Categorized(5, "PANTRYVALE EXPRESS DEMO", 100m, ExpenseCategory.Groceries, "R016", isPotentialDuplicate: true)
            ],
            [
                Invalid(6, "", "25.00", ReviewReason.MissingDescription, "description is required")
            ],
            expectedTotal: 250m,
            sourceRowCount: 6);

        Assert.Equal(250m, report.Totals.ProcessedTotal);
        Assert.Equal(200m, report.Totals.CategoryTotal);
        Assert.Equal(20m, report.Totals.ExcludedFromTotalsTotal);
        var groceries = Assert.Single(report.CategoryTotals);
        Assert.Equal(ExpenseCategory.Groceries, groceries.Category);
        Assert.Equal(2, groceries.TransactionCount);
        Assert.Equal(200m, groceries.Total);
    }

    [Fact]
    public void KeepsInvalidReviewExcludedAndPotentialDuplicateRowsVisible()
    {
        var report = Generate(
            [
                Categorized(1, "FRESHVALE MARKET DEMO", 100m, ExpenseCategory.Groceries, "R001"),
                Review(2, "MARKETBOX DEMO ORDER 8842", 50m, ReviewReason.UnknownMarketplace, "R011"),
                Excluded(3, "REFUND DEMO STORE", -10m, ReviewReason.RefundLikeNegativeAmount, "R013", sourceType: "refund"),
                Categorized(4, "PANTRYVALE EXPRESS DEMO", 100m, ExpenseCategory.Groceries, "R016", isPotentialDuplicate: true)
            ],
            [
                Invalid(5, "", "25.00", ReviewReason.MissingDescription, "description is required")
            ],
            expectedTotal: 250m,
            sourceRowCount: 5);

        Assert.Equal([1, 2, 3, 4, 5], report.Transactions.Select(transaction => transaction.SourceRow.RowNumber).ToArray());
        Assert.Equal([2, 3, 4], report.ReviewItems.Select(transaction => transaction.SourceRow.RowNumber).ToArray());
        Assert.Equal([5], report.InvalidRows.Select(transaction => transaction.SourceRow.RowNumber).ToArray());
        Assert.Equal([3], report.ExcludedRows.Select(transaction => transaction.SourceRow.RowNumber).ToArray());
        Assert.Contains(report.Transactions, transaction => transaction.SourceRow.RowNumber == 4 && transaction.IsPotentialDuplicate);
        Assert.True(report.AccountsForEverySourceRow());
    }

    [Fact]
    public void ExpectedTotalMatchUsesExactDecimalComparison()
    {
        var report = Generate(
            [Categorized(1, "FRESHVALE MARKET DEMO", 100m, ExpenseCategory.Groceries, "R001")],
            [],
            expectedTotal: 100m,
            sourceRowCount: 1);

        Assert.Equal(ExpectedTotalValidationStatus.Match, report.ExpectedTotalValidationStatus);
        Assert.Equal(0m, report.ValidationResult.Difference);
    }

    [Fact]
    public void ExpectedTotalMismatchIncludesAbsoluteDifference()
    {
        var report = Generate(
            [Categorized(1, "FRESHVALE MARKET DEMO", 100m, ExpenseCategory.Groceries, "R001")],
            [],
            expectedTotal: 125m,
            sourceRowCount: 1);

        Assert.Equal(ExpectedTotalValidationStatus.Mismatch, report.ExpectedTotalValidationStatus);
        Assert.Equal(25m, report.ValidationResult.Difference);
    }

    [Fact]
    public void MissingExpectedTotalDoesNotFailReportGeneration()
    {
        var report = Generate(
            [Categorized(1, "FRESHVALE MARKET DEMO", 100m, ExpenseCategory.Groceries, "R001")],
            [],
            expectedTotal: null,
            sourceRowCount: 1);

        Assert.Equal(ExpectedTotalValidationStatus.NotProvided, report.ExpectedTotalValidationStatus);
        Assert.Null(report.ValidationResult.Difference);
        Assert.Equal(100m, report.Totals.ProcessedTotal);
    }

    [Fact]
    public void AuditSummaryIncludesCountsRuleCountValidationStatusAndNoAiUse()
    {
        var report = Generate(
            [
                Categorized(1, "FRESHVALE MARKET DEMO", 100m, ExpenseCategory.Groceries, "R001"),
                Review(2, "WELLSPRING CAFE PHARMACY DEMO", 75m, ReviewReason.CategoryConflict, "R006", "R012"),
                Excluded(3, "TRANSFER DEMO TO WALLET", 30m, ReviewReason.TransferOrPayment, "R014", sourceType: "transfer")
            ],
            [
                Invalid(4, "", "25.00", ReviewReason.MissingDescription, "description is required")
            ],
            expectedTotal: 175m,
            sourceRowCount: 4);

        Assert.Equal(4, report.AuditSummary.Counts.SourceRows);
        Assert.Equal(3, report.AuditSummary.Counts.ValidRows);
        Assert.Equal(1, report.AuditSummary.Counts.CategorizedRows);
        Assert.Equal(2, report.AuditSummary.Counts.ReviewRequiredRows);
        Assert.Equal(1, report.AuditSummary.Counts.InvalidRows);
        Assert.Equal(1, report.AuditSummary.Counts.ExcludedFromTotalsRows);
        Assert.Equal(4, report.AuditSummary.AppliedDeterministicRuleCount);
        Assert.Equal(ExpectedTotalValidationStatus.Match, report.AuditSummary.ExpectedTotalValidationStatus);
        Assert.False(report.AuditSummary.AiUsed);
        Assert.Contains(report.AuditSummary.Messages, message => message == "No AI was used.");
    }

    private ExpenseReport Generate(
        IReadOnlyCollection<ExpenseTransaction> transactions,
        IReadOnlyCollection<InvalidTransactionRow> invalidRows,
        decimal? expectedTotal,
        int sourceRowCount) =>
        _generator.Generate(new ExpenseReportGenerationInput(
            "unit-demo.csv",
            sourceRowCount,
            transactions,
            invalidRows,
            expectedTotal,
            FixedGeneratedAt));

    private static ExpenseTransaction Categorized(
        int rowNumber,
        string description,
        decimal amount,
        ExpenseCategory category,
        string ruleId,
        bool isPotentialDuplicate = false)
    {
        var matches = isPotentialDuplicate
            ? new[] { Rule(ruleId, category), Rule("R017", null) }
            : [Rule(ruleId, category)];

        return new ExpenseTransaction(
            SourceRow(rowNumber, description, amount),
            date: new DateOnly(2026, 4, rowNumber),
            description,
            amount,
            RowStatus.Categorized,
            category,
            isPotentialDuplicate ? ReviewReason.PotentialDuplicate : null,
            includedInProcessedTotal: true,
            includedInCategoryTotals: true,
            isPotentialDuplicate,
            installment: null,
            validationIssues: null,
            matches);
    }

    private static ExpenseTransaction Review(
        int rowNumber,
        string description,
        decimal amount,
        ReviewReason reason,
        params string[] ruleIds) =>
        new(
            SourceRow(rowNumber, description, amount),
            date: new DateOnly(2026, 4, rowNumber),
            description,
            amount,
            RowStatus.ReviewRequired,
            category: null,
            reason,
            includedInProcessedTotal: true,
            includedInCategoryTotals: false,
            ruleMatches: ruleIds.Select(ruleId => Rule(ruleId, null)).ToArray());

    private static ExpenseTransaction Excluded(
        int rowNumber,
        string description,
        decimal amount,
        ReviewReason reason,
        string ruleId,
        string sourceType) =>
        new(
            SourceRow(rowNumber, description, amount, sourceType),
            date: new DateOnly(2026, 4, rowNumber),
            description,
            amount,
            RowStatus.ExcludedFromTotals,
            category: null,
            reason,
            includedInProcessedTotal: false,
            includedInCategoryTotals: false,
            ruleMatches: [Rule(ruleId, null)]);

    private static InvalidTransactionRow Invalid(
        int rowNumber,
        string description,
        string amount,
        ReviewReason reason,
        string message) =>
        new(
            new TransactionSourceRow(
                rowNumber,
                $"2026-04-{rowNumber:00}",
                $"DMO-{rowNumber:0000}",
                description,
                amount,
                rawInstallment: null,
                "purchase",
                "Synthetic invalid row"),
            [new ValidationIssue(reason, message)]);

    private static TransactionSourceRow SourceRow(
        int rowNumber,
        string description,
        decimal amount,
        string sourceType = "purchase") =>
        new(
            rowNumber,
            $"2026-04-{rowNumber:00}",
            $"DMO-{rowNumber:0000}",
            description,
            amount.ToString("0.00", CultureInfo.InvariantCulture),
            rawInstallment: null,
            sourceType,
            "Synthetic test row");

    private static CategorizationRuleMatch Rule(string ruleId, ExpenseCategory? category) =>
        new(ruleId, CategorizationMatchType.KnownMerchant, category, 100, "synthetic test rule");
}
