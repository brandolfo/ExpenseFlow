using ExpenseFlow.Application.ExpenseReports.Categorization;
using ExpenseFlow.Application.ExpenseReports.Pdf;
using ExpenseFlow.Application.ExpenseReports.PdfProcessing;
using ExpenseFlow.Application.ExpenseReports.Reporting;
using ExpenseFlow.Domain.Categorization;
using ExpenseFlow.Domain.ExpenseReports;
using ExpenseFlow.Domain.Transactions;
using ExpenseFlow.Infrastructure.Pdf;

namespace ExpenseFlow.IntegrationTests;

public sealed class PdfExpenseReportProcessingServiceTests
{
    [Theory]
    [InlineData(
        PdfStatementShapeIds.IcbcVisaLikeV1,
        "icbc-visa-like-v1.synthetic.pdf",
        65521.95,
        9,
        7,
        2,
        8,
        1,
        1)]
    [InlineData(
        PdfStatementShapeIds.IcbcMastercardLikeV1,
        "icbc-mastercard-like-v1.synthetic.pdf",
        55812.00,
        10,
        8,
        2,
        9,
        1,
        1)]
    public async Task SyntheticPdfFixturesProcessThroughInternalDeterministicReportService(
        string statementShape,
        string sourceName,
        decimal expectedTotal,
        int expectedSourceRows,
        int expectedValidRows,
        int expectedInvalidRows,
        int expectedNormalizedRows,
        int expectedInvalidExtractedRows,
        int expectedUnprocessableRows)
    {
        var result = await ProcessFixtureAsync(statementShape, sourceName, expectedTotal);
        var report = AssertSuccessfulReport(result);

        Assert.Equal(sourceName, report.Metadata.SourceName);
        Assert.Equal(PdfExtractionStatus.Partial, result.Metadata.ExtractionStatus);
        Assert.Equal(statementShape, result.Metadata.StatementShapeId);
        Assert.Equal(expectedNormalizedRows, result.Metadata.NormalizedRowCount);
        Assert.Equal(expectedInvalidExtractedRows, result.Metadata.InvalidExtractedRowCount);
        Assert.Equal(expectedUnprocessableRows, result.Metadata.UnprocessableNormalizedRowCount);
        Assert.False(result.Metadata.AiUsed);

        Assert.Equal(expectedSourceRows, report.AuditSummary.Counts.SourceRows);
        Assert.Equal(expectedValidRows, report.AuditSummary.Counts.ValidRows);
        Assert.Equal(expectedInvalidRows, report.AuditSummary.Counts.InvalidRows);
        Assert.Equal(expectedTotal, report.Totals.ProcessedTotal);
        Assert.Equal(ExpectedTotalValidationStatus.Match, report.ValidationResult.Status);
        Assert.False(report.AuditSummary.AiUsed);
        Assert.Equal(Enumerable.Range(1, expectedSourceRows), report.Transactions.Select(row => row.SourceRow.RowNumber));
    }

    [Fact]
    public async Task PdfExpectedTotalMismatchIsReportedWithoutHidingRows()
    {
        var result = await ProcessFixtureAsync(
            PdfStatementShapeIds.IcbcVisaLikeV1,
            "icbc-visa-like-v1.synthetic.pdf",
            expectedTotal: 65000.00m);
        var report = AssertSuccessfulReport(result);

        Assert.Equal(ExpectedTotalValidationStatus.Mismatch, report.ValidationResult.Status);
        Assert.Equal(521.95m, report.ValidationResult.Difference);
        Assert.Equal(9, report.Transactions.Count);
        Assert.NotEmpty(report.InvalidRows);
        Assert.NotEmpty(report.ReviewItems);
    }

    [Fact]
    public async Task PdfMissingExpectedTotalStillProducesReport()
    {
        var result = await ProcessFixtureAsync(
            PdfStatementShapeIds.IcbcMastercardLikeV1,
            "icbc-mastercard-like-v1.synthetic.pdf",
            expectedTotal: null);
        var report = AssertSuccessfulReport(result);

        Assert.Equal(ExpectedTotalValidationStatus.NotProvided, report.ValidationResult.Status);
        Assert.Null(report.Totals.ExpectedTotal);
        Assert.Equal(55812.00m, report.Totals.ProcessedTotal);
    }

    [Fact]
    public async Task PdfRowsRemainVisibleWhenInvalidUnsupportedForCurrencyReviewOrExcluded()
    {
        var result = await ProcessFixtureAsync(
            PdfStatementShapeIds.IcbcVisaLikeV1,
            "icbc-visa-like-v1.synthetic.pdf",
            expectedTotal: 65521.95m);
        var report = AssertSuccessfulReport(result);

        Assert.Contains(report.InvalidRows, row =>
            row.SourceRow.RawCode == "SVI-9108" &&
            row.ValidationIssues.Any(issue => issue.Reason == ReviewReason.UnsupportedCurrency));
        Assert.Contains(report.InvalidRows, row =>
            row.SourceRow.RawCode == "SVI-9109" &&
            row.ValidationIssues.Any(issue => issue.Reason == ReviewReason.InvalidDate) &&
            row.ValidationIssues.Any(issue => issue.Reason == ReviewReason.InvalidAmount));
        Assert.Contains(report.ExcludedRows, row =>
            row.SourceRow.RawCode == "SVI-9107" &&
            row.ReviewReason == ReviewReason.RefundLikeNegativeAmount);
        Assert.Contains(report.ReviewItems, row =>
            row.SourceRow.RawCode == "SVI-9101" &&
            row.ReviewReason == ReviewReason.NoMatchingRule);
    }

    [Fact]
    public async Task PdfTraceabilityIsPreservedInSourceRows()
    {
        var result = await ProcessFixtureAsync(
            PdfStatementShapeIds.IcbcMastercardLikeV1,
            "icbc-mastercard-like-v1.synthetic.pdf",
            expectedTotal: 55812.00m);
        var report = AssertSuccessfulReport(result);

        Assert.All(report.Transactions, transaction =>
        {
            Assert.Contains("pdf_source=icbc-mastercard-like-v1.synthetic.pdf", transaction.SourceRow.RawNotes);
            Assert.Contains("statement_shape=icbc-mastercard-like-v1", transaction.SourceRow.RawNotes);
            Assert.Contains($"extraction_order={transaction.SourceRow.RowNumber}", transaction.SourceRow.RawNotes);
        });
        Assert.Contains(report.Transactions, transaction =>
            transaction.SourceRow.RawCode == "SMC-8105" &&
            transaction.SourceRow.RawInstallment == "01/06" &&
            transaction.SourceRow.RawNotes?.Contains("source_page=2", StringComparison.Ordinal) == true);
    }

    [Fact]
    public async Task PdfProcessingReusesExistingCategorizationAndReportGeneration()
    {
        var result = await ProcessFixtureAsync(
            PdfStatementShapeIds.IcbcVisaLikeV1,
            "icbc-visa-like-v1.synthetic.pdf",
            expectedTotal: 65521.95m);
        var report = AssertSuccessfulReport(result);

        Assert.Contains(report.Transactions, transaction =>
            transaction.SourceRow.RawCode == "SVI-9102" &&
            transaction.Category == ExpenseCategory.RestaurantsAndCafes &&
            transaction.RuleMatches.Any(match => match.RuleId == "R012"));
        Assert.Contains(report.Transactions, transaction =>
            transaction.SourceRow.RawCode == "SVI-9106" &&
            transaction.Category == ExpenseCategory.FeesAndTaxes &&
            transaction.RuleMatches.Any(match => match.RuleId == "R010"));
        Assert.Contains(report.CategoryTotals, total =>
            total.Category == ExpenseCategory.RestaurantsAndCafes &&
            total.Total == 2750.00m);
        Assert.Contains(report.CategoryTotals, total =>
            total.Category == ExpenseCategory.FeesAndTaxes &&
            total.Total == 680.40m);
    }

    [Fact]
    public async Task InternalPdfProcessingServiceDoesNotRequireApiEndpoint()
    {
        var service = CreateService();
        var content = await File.ReadAllBytesAsync(GetPdfFixturePath("icbc-visa-like-v1.pdf"));

        var result = await service.ProcessAsync(new PdfExpenseReportProcessingRequest(
            "icbc-visa-like-v1.synthetic.pdf",
            content,
            ExpectedTotal: 65521.95m,
            StatementShapeHint: PdfStatementShapeIds.IcbcVisaLikeV1));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Report);
        Assert.Equal(PdfStatementShapeIds.IcbcVisaLikeV1, result.Metadata.StatementShapeId);
    }

    private static async Task<PdfExpenseReportProcessingResult> ProcessFixtureAsync(
        string statementShape,
        string sourceName,
        decimal? expectedTotal)
    {
        var content = await File.ReadAllBytesAsync(GetPdfFixturePath($"{statementShape}.pdf"));

        return await CreateService().ProcessAsync(new PdfExpenseReportProcessingRequest(
            sourceName,
            content,
            expectedTotal,
            statementShape));
    }

    private static DeterministicPdfExpenseReportProcessingService CreateService() =>
        new(
            new PdfPigPdfStatementExtractor(),
            new DeterministicPdfStatementRowNormalizer(),
            new SeedDeterministicCategorizationRuleEngine(),
            new DeterministicExpenseReportGenerator());

    private static ExpenseReport AssertSuccessfulReport(PdfExpenseReportProcessingResult result)
    {
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Report);

        return result.Report;
    }

    private static string GetPdfFixturePath(string fileName) =>
        Path.Combine(GetBackendDirectory(), "testdata", "pdf", fileName);

    private static string GetBackendDirectory() =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
}
