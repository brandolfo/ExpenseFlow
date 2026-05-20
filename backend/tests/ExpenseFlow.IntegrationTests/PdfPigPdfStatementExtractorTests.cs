using ExpenseFlow.Application.ExpenseReports.Pdf;
using ExpenseFlow.Infrastructure.Pdf;

namespace ExpenseFlow.IntegrationTests;

public sealed class PdfPigPdfStatementExtractorTests
{
    private readonly PdfPigPdfStatementExtractor _extractor = new();

    [Fact]
    public async Task VisaLikeSyntheticPdfCanBeOpenedAndRawTextExtracted()
    {
        var result = await ExtractFixtureAsync("icbc-visa-like-v1.pdf", PdfStatementShapeIds.IcbcVisaLikeV1);
        var text = JoinLines(result);

        Assert.Equal(PdfExtractionStatus.Succeeded, result.Status);
        Assert.Equal("icbc-visa-like-v1.pdf", result.SourceName);
        Assert.Equal(PdfStatementShapeIds.IcbcVisaLikeV1, result.StatementShapeId);
        Assert.NotEmpty(result.ExtractedLines);
        Assert.Empty(result.ExtractedRows);
        Assert.Empty(result.InvalidRows);
        Assert.Contains("Synthetic ICBC-Like Visa Statement", text);
        Assert.Contains("FECHA", text);
        Assert.Contains("DETALLE DE TRANSACCION", text);
        Assert.Contains("MERCADO NORTE TEST", text);
    }

    [Fact]
    public async Task MastercardLikeSyntheticPdfCanBeOpenedAndRawTextExtracted()
    {
        var result = await ExtractFixtureAsync("icbc-mastercard-like-v1.pdf", PdfStatementShapeIds.IcbcMastercardLikeV1);
        var text = JoinLines(result);

        Assert.Equal(PdfExtractionStatus.Succeeded, result.Status);
        Assert.Equal(PdfStatementShapeIds.IcbcMastercardLikeV1, result.StatementShapeId);
        Assert.NotEmpty(result.ExtractedLines);
        Assert.Empty(result.ExtractedRows);
        Assert.Empty(result.InvalidRows);
        Assert.Contains("RESUMEN CONSOLIDADO", text);
        Assert.Contains("DETALLE DEL MES", text);
        Assert.Contains("Compras del Mes", text);
        Assert.Contains("Debitos Automaticos", text);
        Assert.Contains("Cuotas del Mes", text);
        Assert.Contains("TOTAL TITULAR", text);
    }

    [Fact]
    public async Task PageNumbersSourceNameShapeHintAndExtractionOrderArePreserved()
    {
        var result = await ExtractFixtureAsync("icbc-visa-like-v1.pdf", PdfStatementShapeIds.IcbcVisaLikeV1);
        var lines = result.ExtractedLines.ToArray();

        Assert.All(lines, line =>
        {
            Assert.Equal("icbc-visa-like-v1.pdf", line.SourceName);
            Assert.Equal(PdfStatementShapeIds.IcbcVisaLikeV1, line.StatementShapeId);
            Assert.Equal(PdfStatementShapeIds.IcbcVisaLikeV1, line.StatementShapeHint);
            Assert.Equal(1, line.SourcePage);
        });

        Assert.Equal(Enumerable.Range(1, lines.Length), lines.Select(line => line.ExtractionOrder));
        Assert.True(IndexOf(lines, "DETALLE DE TRANSACCION") < IndexOf(lines, "MERCADO NORTE TEST"));
    }

    [Fact]
    public async Task MastercardLikeMultiPageFixtureYieldsPageOneAndPageTwoEvidence()
    {
        var result = await ExtractFixtureAsync("icbc-mastercard-like-v1.pdf", PdfStatementShapeIds.IcbcMastercardLikeV1);

        Assert.Contains(result.ExtractedLines, line => line.SourcePage == 1 && line.Text.Contains("SERVICIO DIGITAL TEST", StringComparison.Ordinal));
        Assert.Contains(result.ExtractedLines, line => line.SourcePage == 2 && line.Text.Contains("ELECTRO TEST 01/06", StringComparison.Ordinal));
        Assert.Contains(result.ExtractedLines, line => line.SourcePage == 2 && line.Text.Contains("TOTAL TITULAR", StringComparison.Ordinal));
        Assert.Equal([1, 2], result.ExtractedLines.Select(line => line.SourcePage).Distinct().Order().ToArray());
    }

    [Fact]
    public async Task EmptyPdfInputReturnsStructuredFailure()
    {
        var result = await _extractor.ExtractAsync(new PdfStatementExtractionRequest(
            "empty.pdf",
            [],
            PdfStatementShapeIds.IcbcVisaLikeV1));

        Assert.Equal(PdfExtractionStatus.Failed, result.Status);
        Assert.False(result.IsProcessable);
        Assert.Empty(result.ExtractedLines);
        Assert.Contains(result.Warnings, warning => warning.Code == "empty_pdf_content");
    }

    [Fact]
    public async Task MalformedPdfInputReturnsStructuredFailureInsteadOfThrowing()
    {
        var result = await _extractor.ExtractAsync(new PdfStatementExtractionRequest(
            "malformed.pdf",
            "not a pdf"u8.ToArray(),
            PdfStatementShapeIds.IcbcVisaLikeV1));

        Assert.Equal(PdfExtractionStatus.Failed, result.Status);
        Assert.False(result.IsProcessable);
        Assert.Empty(result.ExtractedLines);
        Assert.Contains(result.Warnings, warning => warning.Code == "pdf_extraction_failed");
    }

    [Fact]
    public async Task StatementShapeCanBeDetectedWithoutHintForSupportedSyntheticFixtures()
    {
        var visa = await ExtractFixtureAsync("icbc-visa-like-v1.pdf", statementShapeHint: null);
        var mastercard = await ExtractFixtureAsync("icbc-mastercard-like-v1.pdf", statementShapeHint: null);

        Assert.Equal(PdfStatementShapeIds.IcbcVisaLikeV1, visa.StatementShapeId);
        Assert.Equal(PdfStatementShapeIds.IcbcMastercardLikeV1, mastercard.StatementShapeId);
    }

    private async Task<PdfStatementExtractionResult> ExtractFixtureAsync(string fileName, string? statementShapeHint)
    {
        var content = await File.ReadAllBytesAsync(GetPdfFixturePath(fileName));

        return await _extractor.ExtractAsync(new PdfStatementExtractionRequest(fileName, content, statementShapeHint));
    }

    private static string JoinLines(PdfStatementExtractionResult result) =>
        string.Join(Environment.NewLine, result.ExtractedLines.Select(line => line.Text));

    private static int IndexOf(IReadOnlyCollection<PdfExtractedTextLine> lines, string text) =>
        Array.FindIndex(lines.ToArray(), line => line.Text.Contains(text, StringComparison.Ordinal));

    private static string GetPdfFixturePath(string fileName) =>
        Path.Combine(GetBackendDirectory(), "testdata", "pdf", fileName);

    private static string GetBackendDirectory() =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
}
