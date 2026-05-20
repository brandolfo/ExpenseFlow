using System.Reflection;
using ExpenseFlow.Application.Abstractions;
using ExpenseFlow.Application.Common;
using ExpenseFlow.Application.ExpenseReports.Pdf;
using ExpenseFlow.Domain.Common;

namespace ExpenseFlow.UnitTests;

public sealed class PdfStatementExtractionBoundaryTests
{
    [Fact]
    public void SupportedStatementShapeIdentifiersMatchPdfPhaseContract()
    {
        Assert.Equal("icbc-visa-like-v1", PdfStatementShapeIds.IcbcVisaLikeV1);
        Assert.Equal("icbc-mastercard-like-v1", PdfStatementShapeIds.IcbcMastercardLikeV1);
        Assert.Equal("unknown", PdfStatementShapeIds.Unknown);
        Assert.Equal("unsupported", PdfStatementShapeIds.Unsupported);
    }

    [Fact]
    public void ExtractedRowsPreserveTraceabilityAndNormalizedFields()
    {
        var row = SyntheticExtractedRow();

        Assert.Equal(2, row.SourcePage);
        Assert.Equal(7, row.ExtractionOrder);
        Assert.Equal("masked row evidence", row.EvidenceSnippet);
        Assert.Equal(new DateOnly(2026, 4, 12), row.Date);
        Assert.Equal("SYNTHETIC STORE INSTALLMENT 01/06", row.Description);
        Assert.Equal(12345.67m, row.Amount);
        Assert.Equal("ABC-123", row.Code);
        Assert.Equal("01/06", row.Installment);
        Assert.Equal("installment", row.SourceType);
        Assert.Equal("Synthetic PDF row", row.Notes);

        var dollars = Assert.Single(row.ForeignCurrencyEvidence);
        Assert.Equal("USD", dollars.CurrencyCode);
        Assert.Equal("10.50", dollars.RawAmount);
        Assert.Equal(10.50m, dollars.Amount);
    }

    [Fact]
    public void ExtractedRowsCanBecomeParsedTransactionCandidatesForExistingPipeline()
    {
        var candidate = SyntheticExtractedRow().ToParsedTransactionCandidate();

        Assert.Equal(7, candidate.SourceRow.RowNumber);
        Assert.Equal(new DateOnly(2026, 4, 12), candidate.Date);
        Assert.Equal("SYNTHETIC STORE INSTALLMENT 01/06", candidate.Description);
        Assert.Equal(12345.67m, candidate.Amount);
        Assert.Equal("12.04.26", candidate.SourceRow.RawDate);
        Assert.Equal("ABC-123", candidate.SourceRow.RawCode);
        Assert.Equal("12345.67", candidate.SourceRow.RawAmount);
        Assert.Equal("01/06", candidate.SourceRow.RawInstallment);
        Assert.Equal("installment", candidate.SourceRow.RawSourceType);
    }

    [Fact]
    public void ExtractionResultCanRepresentWarningsInvalidRowsAndPartialStatus()
    {
        var warning = new PdfExtractionWarning(
            "split-description",
            "Description continued onto another extracted line.",
            SourcePage: 1,
            ExtractionOrder: 3);
        var invalidRow = new PdfInvalidExtractedRow(
            SourcePage: 1,
            ExtractionOrder: 3,
            EvidenceSnippet: "masked invalid row evidence",
            new PdfExtractedFieldValues(
                Date: "not-a-date",
                Code: null,
                Description: "SYNTHETIC UNCLEAR ROW",
                Amount: "123.00",
                Installment: null,
                SourceType: "unknown",
                Notes: "Synthetic invalid row"),
            [warning]);

        var result = new PdfStatementExtractionResult(
            "synthetic.pdf",
            PdfStatementShapeIds.IcbcVisaLikeV1,
            PdfExtractionStatus.Partial,
            [SyntheticExtractedRow()],
            [invalidRow],
            [warning],
            [],
            []);

        Assert.True(result.IsProcessable);
        Assert.Equal(PdfExtractionStatus.Partial, result.Status);
        Assert.Single(result.ExtractedRows);
        Assert.Single(result.InvalidRows);
        Assert.Single(result.Warnings);
    }

    [Theory]
    [InlineData(PdfExtractionStatus.UnsupportedStatementShape)]
    [InlineData(PdfExtractionStatus.UnsupportedScannedImageOnly)]
    [InlineData(PdfExtractionStatus.UnsupportedEncrypted)]
    [InlineData(PdfExtractionStatus.Failed)]
    public void UnsupportedAndFailureStatusesAreRepresentable(PdfExtractionStatus status)
    {
        var result = PdfStatementExtractionResult.Unsupported(
            "unsupported.pdf",
            PdfStatementShapeIds.Unsupported,
            status,
            [new PdfExtractionWarning("unsupported", "Synthetic unsupported PDF scenario.")]);

        Assert.False(result.IsProcessable);
        Assert.Empty(result.ExtractedRows);
        Assert.Empty(result.InvalidRows);
        Assert.Single(result.Warnings);
    }

    [Fact]
    public void ExtractedStatementTotalsAreMetadataOnly()
    {
        var result = new PdfStatementExtractionResult(
            "synthetic.pdf",
            PdfStatementShapeIds.IcbcMastercardLikeV1,
            PdfExtractionStatus.Succeeded,
            [SyntheticExtractedRow()],
            [],
            [],
            [
                new PdfExtractedStatementTotal(
                    "TOTAL TITULAR",
                    "12345.67",
                    12345.67m,
                    "ARS",
                    SourcePage: 3,
                    EvidenceSnippet: "masked total evidence")
            ],
            []);

        var total = Assert.Single(result.ExtractedStatementTotals);
        Assert.Equal("TOTAL TITULAR", total.Label);
        Assert.Equal(12345.67m, total.Amount);
        Assert.DoesNotContain(
            typeof(PdfStatementExtractionResult).GetProperties(),
            property => property.Name.Contains("ExpectedTotal", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ExtractedTextLinesPreserveRawTraceabilityWithoutPdfLibraryTypes()
    {
        var line = new PdfExtractedTextLine(
            "synthetic.pdf",
            PdfStatementShapeIds.IcbcVisaLikeV1,
            PdfStatementShapeIds.IcbcVisaLikeV1,
            SourcePage: 1,
            ExtractionOrder: 2,
            Text: "FECHA COMPROBANTE DETALLE DE TRANSACCION PESOS DOLARES");

        Assert.Equal("synthetic.pdf", line.SourceName);
        Assert.Equal(PdfStatementShapeIds.IcbcVisaLikeV1, line.StatementShapeId);
        Assert.Equal(PdfStatementShapeIds.IcbcVisaLikeV1, line.StatementShapeHint);
        Assert.Equal(1, line.SourcePage);
        Assert.Equal(2, line.ExtractionOrder);
        Assert.Contains("DETALLE DE TRANSACCION", line.Text);
    }

    [Fact]
    public void PdfExtractorBoundaryUsesApplicationModelsOnly()
    {
        var method = typeof(IPdfStatementExtractor).GetMethod(nameof(IPdfStatementExtractor.ExtractAsync));

        Assert.NotNull(method);
        Assert.Equal(typeof(PdfStatementExtractionResult), method!.ReturnType.GenericTypeArguments.Single());
        Assert.Equal(typeof(PdfStatementExtractionRequest), method.GetParameters()[0].ParameterType);
    }

    [Fact]
    public void CsvParserBoundaryRemainsSeparateFromPdfExtractorBoundary()
    {
        var csvParseMethod = typeof(ITransactionFileParser).GetMethod(nameof(ITransactionFileParser.ParseAsync));

        Assert.NotNull(csvParseMethod);
        Assert.Equal(typeof(string), csvParseMethod!.GetParameters()[0].ParameterType);
        Assert.Equal("csvText", csvParseMethod.GetParameters()[0].Name);
        Assert.NotEqual(typeof(IPdfStatementExtractor), typeof(ITransactionFileParser));
    }

    [Fact]
    public void ApplicationAndDomainDoNotReferencePdfLibrariesOrRuntimeAiAgents()
    {
        AssertAssemblyDoesNotReferenceForbiddenTerms(typeof(ApplicationAssemblyMarker).Assembly);
        AssertAssemblyDoesNotReferenceForbiddenTerms(typeof(DomainAssemblyMarker).Assembly);
        AssertNoForbiddenTypeNames(typeof(ApplicationAssemblyMarker).Assembly);
        AssertNoForbiddenTypeNames(typeof(DomainAssemblyMarker).Assembly);
    }

    private static PdfExtractedTransactionRow SyntheticExtractedRow() =>
        new(
            SourcePage: 2,
            ExtractionOrder: 7,
            EvidenceSnippet: "masked row evidence",
            new PdfExtractedFieldValues(
                Date: "12.04.26",
                Code: "ABC-123",
                Description: "SYNTHETIC STORE INSTALLMENT 01/06",
                Amount: "12345.67",
                Installment: "01/06",
                SourceType: "installment",
                Notes: "Synthetic PDF row"),
            new DateOnly(2026, 4, 12),
            "SYNTHETIC STORE INSTALLMENT 01/06",
            12345.67m,
            Code: "ABC-123",
            Installment: "01/06",
            SourceType: "installment",
            Notes: "Synthetic PDF row",
            ForeignCurrencyEvidence:
            [
                new PdfForeignCurrencyEvidence("USD", "10.50", 10.50m)
            ]);

    private static void AssertAssemblyDoesNotReferenceForbiddenTerms(Assembly assembly)
    {
        var referencedAssemblies = assembly.GetReferencedAssemblies().Select(reference => reference.Name ?? string.Empty);
        var forbiddenTerms = new[] { "PdfPig", "QuestPDF", "OpenAI", "OpenAi", "AspNetCore", "EntityFramework" };

        foreach (var forbiddenTerm in forbiddenTerms)
        {
            Assert.DoesNotContain(referencedAssemblies, reference => reference.Contains(forbiddenTerm, StringComparison.OrdinalIgnoreCase));
        }
    }

    private static void AssertNoForbiddenTypeNames(Assembly assembly)
    {
        var types = assembly.GetTypes();
        var forbiddenTerms = new[] { "PdfPig", "QuestPdf", "OpenAi", "Llm", "Agent" };

        foreach (var forbiddenTerm in forbiddenTerms)
        {
            Assert.DoesNotContain(types, type => type.Name.Contains(forbiddenTerm, StringComparison.OrdinalIgnoreCase));
        }
    }
}
