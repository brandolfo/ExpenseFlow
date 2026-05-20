using System.Globalization;
using ExpenseFlow.Application.ExpenseReports.Pdf;
using ExpenseFlow.Infrastructure.Pdf;

namespace ExpenseFlow.IntegrationTests;

public sealed class PdfStatementRowNormalizerTests
{
    private readonly PdfPigPdfStatementExtractor _extractor = new();
    private readonly DeterministicPdfStatementRowNormalizer _normalizer = new();

    [Theory]
    [InlineData("icbc-visa-like-v1")]
    [InlineData("icbc-mastercard-like-v1")]
    public async Task SyntheticPdfExtractionAndNormalizationMatchesExpectedRows(string statementShape)
    {
        var expectedRows = ExpectedNormalizedRow.Load(GetPdfFixturePath($"{statementShape}.expected-normalized-rows.csv"));
        var result = await ExtractAndNormalizeAsync(statementShape, expectedRows[0].SourceName);
        var actualRows = FlattenActualRows(result);

        Assert.Equal(PdfExtractionStatus.Partial, result.Status);
        Assert.Equal(expectedRows.Count, actualRows.Count);

        foreach (var expected in expectedRows)
        {
            var actual = Assert.Single(actualRows, row => row.ExtractionOrder == expected.ExtractionOrder);

            Assert.Equal(expected.RowNumber, actual.RowNumber);
            Assert.Equal(expected.SourceName, actual.SourceName);
            Assert.Equal(expected.StatementShape, actual.StatementShape);
            Assert.Equal(expected.SourcePage, actual.SourcePage);
            Assert.Equal(expected.Date, actual.Date);
            Assert.Equal(expected.Code, actual.Code);
            Assert.Equal(expected.Description, actual.Description);
            Assert.Equal(expected.Amount, actual.Amount);
            Assert.Equal(expected.Currency, actual.Currency);
            Assert.Equal(expected.Installment, actual.Installment);
            Assert.Equal(expected.SourceType, actual.SourceType);
            Assert.Equal(expected.Notes, actual.Notes);
            Assert.Equal(expected.ExpectedExtractionStatus, actual.ExtractionStatus);
            Assert.Equal(expected.ExpectedWarning, actual.WarningCode);
        }
    }

    [Fact]
    public async Task VisaStopMarkersAreNotNormalizedAsTransactions()
    {
        var result = await ExtractAndNormalizeAsync(
            PdfStatementShapeIds.IcbcVisaLikeV1,
            "icbc-visa-like-v1.synthetic.pdf");
        var descriptions = FlattenActualRows(result).Select(row => row.Description).ToArray();

        Assert.DoesNotContain(descriptions, description => description.Contains("Total Consumos", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(descriptions, description => description.Contains("Impuesto de Sellos", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(descriptions, description => description.Contains("Saldo Actual", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(descriptions, description => description.Contains("Pago Minimo", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task MastercardSummaryAndTotalRowsAreNotNormalizedAsTransactions()
    {
        var result = await ExtractAndNormalizeAsync(
            PdfStatementShapeIds.IcbcMastercardLikeV1,
            "icbc-mastercard-like-v1.synthetic.pdf");
        var descriptions = FlattenActualRows(result).Select(row => row.Description).ToArray();

        Assert.DoesNotContain(descriptions, description => description.Contains("RESUMEN CONSOLIDADO", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(descriptions, description => description.Contains("TOTAL TITULAR", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(descriptions, description => description.Contains("Importe consolidado", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task MastercardRowsPreservePagesOrderInstallmentsAndSpanishDates()
    {
        var result = await ExtractAndNormalizeAsync(
            PdfStatementShapeIds.IcbcMastercardLikeV1,
            "icbc-mastercard-like-v1.synthetic.pdf");

        Assert.Equal(Enumerable.Range(1, 10), FlattenActualRows(result).Select(row => row.ExtractionOrder));
        Assert.Contains(result.ExtractedRows, row => row.SourcePage == 1 && row.Date == new DateOnly(2035, 2, 3));
        Assert.Contains(result.ExtractedRows, row => row.SourcePage == 2 && row.Installment == "01/06");
        Assert.Contains(result.ExtractedRows, row => row.SourcePage == 2 && row.Installment == "06/06");
        Assert.Contains(result.InvalidRows, row => row.SourcePage == 2 && row.ExtractionOrder == 10);
    }

    [Fact]
    public async Task VisaRowsPreserveTrailingMinusAndForeignCurrencyEvidence()
    {
        var result = await ExtractAndNormalizeAsync(
            PdfStatementShapeIds.IcbcVisaLikeV1,
            "icbc-visa-like-v1.synthetic.pdf");

        var refund = Assert.Single(result.ExtractedRows, row => row.Code == "SVI-9107");
        Assert.Equal(-1200.00m, refund.Amount);
        Assert.Equal("ARS", refund.CurrencyCode);
        Assert.Equal("refund", refund.SourceType);

        var dollars = Assert.Single(result.ExtractedRows, row => row.Code == "SVI-9108");
        Assert.Equal("USD", dollars.CurrencyCode);
        Assert.Single(dollars.ForeignCurrencyEvidence);
    }

    [Fact]
    public void NormalizedPdfRowsDoNotExposeCategorizationTotalsOrReports()
    {
        var rowProperties = typeof(PdfExtractedTransactionRow).GetProperties();
        var resultProperties = typeof(PdfStatementExtractionResult).GetProperties();

        Assert.DoesNotContain(rowProperties, property => property.Name.Contains("Category", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(resultProperties, property => property.Name.Contains("ProcessedTotal", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(resultProperties, property => property.Name.Contains("CategoryTotal", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(resultProperties, property => property.Name.Contains("ExpectedTotal", StringComparison.OrdinalIgnoreCase));
    }

    private async Task<PdfStatementExtractionResult> ExtractAndNormalizeAsync(string statementShape, string sourceName)
    {
        var content = await File.ReadAllBytesAsync(GetPdfFixturePath($"{statementShape}.pdf"));
        var extraction = await _extractor.ExtractAsync(new PdfStatementExtractionRequest(sourceName, content, statementShape));

        return _normalizer.Normalize(extraction);
    }

    private static IReadOnlyCollection<ActualNormalizedRow> FlattenActualRows(PdfStatementExtractionResult result)
    {
        var validRows = result.ExtractedRows.Select(row => new ActualNormalizedRow(
            RowNumber: row.ExtractionOrder,
            result.SourceName,
            result.StatementShapeId,
            row.SourcePage,
            row.ExtractionOrder,
            row.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            row.Code ?? string.Empty,
            row.Description,
            row.Amount.ToString("0.00", CultureInfo.InvariantCulture),
            row.CurrencyCode ?? string.Empty,
            row.Installment ?? string.Empty,
            row.SourceType ?? string.Empty,
            row.Notes ?? string.Empty,
            "normalized",
            string.Empty));

        var invalidRows = result.InvalidRows.Select(row => new ActualNormalizedRow(
            RowNumber: row.ExtractionOrder,
            result.SourceName,
            result.StatementShapeId,
            row.SourcePage,
            row.ExtractionOrder,
            row.RawFields.Date ?? string.Empty,
            row.RawFields.Code ?? string.Empty,
            row.RawFields.Description ?? string.Empty,
            row.RawFields.Amount ?? string.Empty,
            row.RawFields.CurrencyCode ?? string.Empty,
            row.RawFields.Installment ?? string.Empty,
            row.RawFields.SourceType ?? string.Empty,
            row.RawFields.Notes ?? string.Empty,
            "invalid",
            row.Warnings.FirstOrDefault()?.Code ?? string.Empty));

        return validRows.Concat(invalidRows)
            .OrderBy(row => row.ExtractionOrder)
            .ToArray();
    }

    private static string GetPdfFixturePath(string fileName) =>
        Path.Combine(GetBackendDirectory(), "testdata", "pdf", fileName);

    private static string GetBackendDirectory() =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private sealed record ActualNormalizedRow(
        int RowNumber,
        string SourceName,
        string StatementShape,
        int SourcePage,
        int ExtractionOrder,
        string Date,
        string Code,
        string Description,
        string Amount,
        string Currency,
        string Installment,
        string SourceType,
        string Notes,
        string ExtractionStatus,
        string WarningCode);

    private sealed record ExpectedNormalizedRow(
        int RowNumber,
        string SourceName,
        string StatementShape,
        int SourcePage,
        int ExtractionOrder,
        string Date,
        string Code,
        string Description,
        string Amount,
        string Currency,
        string Installment,
        string SourceType,
        string Notes,
        string ExpectedExtractionStatus,
        string ExpectedWarning)
    {
        public static IReadOnlyList<ExpectedNormalizedRow> Load(string path)
        {
            var rows = new List<ExpectedNormalizedRow>();

            foreach (var line in File.ReadLines(path).Skip(1).Where(line => !string.IsNullOrWhiteSpace(line)))
            {
                var values = line.Split(',');

                rows.Add(new ExpectedNormalizedRow(
                    int.Parse(values[0], CultureInfo.InvariantCulture),
                    values[1],
                    values[2],
                    int.Parse(values[3], CultureInfo.InvariantCulture),
                    int.Parse(values[4], CultureInfo.InvariantCulture),
                    values[5],
                    values[6],
                    values[7],
                    values[8],
                    values[9],
                    values[10],
                    values[11],
                    values[12],
                    values[13],
                    values[14]));
            }

            return rows;
        }
    }
}
