using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ExpenseFlow.Application.ExpenseReports.Pdf;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ExpenseFlow.IntegrationTests;

public sealed class ProcessPdfExpenseReportEndpointTests
{
    private const string Endpoint = "/api/expense-reports/process-pdf";

    [Theory]
    [InlineData(
        "icbc-visa-like-v1.pdf",
        "icbc-visa-like-v1.synthetic.pdf",
        PdfStatementShapeIds.IcbcVisaLikeV1,
        65521.95,
        9,
        7,
        2,
        1)]
    [InlineData(
        "icbc-mastercard-like-v1.pdf",
        "icbc-mastercard-like-v1.synthetic.pdf",
        PdfStatementShapeIds.IcbcMastercardLikeV1,
        55812.00,
        10,
        8,
        2,
        1)]
    public async Task SyntheticPdfFixturesReturnOkWithReportAndExtractionMetadata(
        string fixtureFileName,
        string sourceName,
        string statementShapeHint,
        decimal expectedTotal,
        int sourceRows,
        int validRows,
        int invalidRows,
        int unprocessableRows)
    {
        using var response = await PostFixtureAsync(fixtureFileName, sourceName, expectedTotal, statementShapeHint);
        using var json = await ReadJsonAsync(response);
        var root = json.RootElement;
        var report = root.GetProperty("report");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(sourceName, root.GetProperty("extractionMetadata").GetProperty("sourceName").GetString());
        Assert.Equal(statementShapeHint, root.GetProperty("extractionMetadata").GetProperty("statementShapeId").GetString());
        Assert.Equal("Partial", root.GetProperty("extractionMetadata").GetProperty("extractionStatus").GetString());
        Assert.Equal(unprocessableRows, root.GetProperty("extractionMetadata").GetProperty("unprocessableNormalizedRowCount").GetInt32());
        Assert.False(root.GetProperty("extractionMetadata").GetProperty("aiUsed").GetBoolean());
        Assert.NotEmpty(root.GetProperty("extractionWarnings").EnumerateArray());

        Assert.Equal(sourceRows, GetInt(report, "processingCounts", "sourceRows"));
        Assert.Equal(validRows, GetInt(report, "processingCounts", "validRows"));
        Assert.Equal(invalidRows, GetInt(report, "processingCounts", "invalidRows"));
        Assert.Equal(expectedTotal, GetDecimal(report, "totals", "processedTotal"));
        Assert.Equal("Match", report.GetProperty("totalValidation").GetProperty("status").GetString());
        Assert.Equal(sourceRows, report.GetProperty("transactionDetails").GetArrayLength());
        Assert.False(report.GetProperty("auditSummary").GetProperty("aiUsed").GetBoolean());
    }

    [Fact]
    public async Task ExpectedTotalMismatchReturnsOkWithVisibleReport()
    {
        using var response = await PostFixtureAsync(
            "icbc-visa-like-v1.pdf",
            "icbc-visa-like-v1.synthetic.pdf",
            expectedTotal: 65000.00m,
            PdfStatementShapeIds.IcbcVisaLikeV1);
        using var json = await ReadJsonAsync(response);
        var report = json.RootElement.GetProperty("report");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Mismatch", report.GetProperty("totalValidation").GetProperty("status").GetString());
        Assert.Equal(521.95m, report.GetProperty("totalValidation").GetProperty("difference").GetDecimal());
        Assert.Equal(9, report.GetProperty("transactionDetails").GetArrayLength());
    }

    [Fact]
    public async Task MissingExpectedTotalReturnsOkWithNotProvidedValidation()
    {
        using var response = await PostFixtureAsync(
            "icbc-mastercard-like-v1.pdf",
            "icbc-mastercard-like-v1.synthetic.pdf",
            expectedTotal: null,
            PdfStatementShapeIds.IcbcMastercardLikeV1);
        using var json = await ReadJsonAsync(response);
        var report = json.RootElement.GetProperty("report");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("NotProvided", report.GetProperty("totalValidation").GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, report.GetProperty("totals").GetProperty("expectedTotal").ValueKind);
    }

    [Fact]
    public async Task NonArsRowsRemainVisibleAsInvalidAndDoNotAffectTotals()
    {
        using var response = await PostFixtureAsync(
            "icbc-visa-like-v1.pdf",
            "icbc-visa-like-v1.synthetic.pdf",
            expectedTotal: 65521.95m,
            PdfStatementShapeIds.IcbcVisaLikeV1);
        using var json = await ReadJsonAsync(response);
        var root = json.RootElement;
        var report = root.GetProperty("report");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, root.GetProperty("extractionMetadata").GetProperty("unprocessableNormalizedRowCount").GetInt32());
        Assert.Contains(
            report.GetProperty("invalidRows").EnumerateArray(),
            item => item.GetProperty("rawValues").GetProperty("code").GetString() == "SVI-9108" &&
                item.GetProperty("errors").EnumerateArray().Any(error => error.GetString()!.Contains("currency", StringComparison.OrdinalIgnoreCase)) &&
                item.GetProperty("includedInProcessedTotal").GetBoolean() == false &&
                item.GetProperty("includedInCategoryTotals").GetBoolean() == false);
        Assert.Equal(65521.95m, GetDecimal(report, "totals", "processedTotal"));
    }

    [Fact]
    public async Task PdfEndpointDoesNotRequireAuthenticationOrExternalServices()
    {
        using var response = await PostFixtureAsync(
            "icbc-visa-like-v1.pdf",
            "icbc-visa-like-v1.synthetic.pdf",
            expectedTotal: 65521.95m,
            PdfStatementShapeIds.IcbcVisaLikeV1);
        using var json = await ReadJsonAsync(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(json.RootElement.GetProperty("report").GetProperty("auditSummary").GetProperty("aiUsed").GetBoolean());
        Assert.False(json.RootElement.GetProperty("extractionMetadata").GetProperty("aiUsed").GetBoolean());
    }

    [Fact]
    public async Task NullRequestBodyReturnsBadRequest()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync<object?>(Endpoint, null);
        using var json = await ReadJsonAsync(response);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("invalid_request", json.RootElement.GetProperty("fileErrors")[0].GetProperty("code").GetString());
    }

    [Fact]
    public async Task MissingSourceNameReturnsBadRequest()
    {
        using var response = await PostAsync(null, "JVBERi0xLjQ=", expectedTotal: null, PdfStatementShapeIds.IcbcVisaLikeV1);
        using var json = await ReadJsonAsync(response);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(json.RootElement.GetProperty("fileErrors").EnumerateArray(), error => error.GetProperty("code").GetString() == "missing_source_name");
    }

    [Fact]
    public async Task MissingPdfBase64ReturnsBadRequest()
    {
        using var response = await PostAsync("missing-pdf.pdf", null, expectedTotal: null, PdfStatementShapeIds.IcbcVisaLikeV1);
        using var json = await ReadJsonAsync(response);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(json.RootElement.GetProperty("fileErrors").EnumerateArray(), error => error.GetProperty("code").GetString() == "missing_pdf_base64");
    }

    [Fact]
    public async Task InvalidBase64ReturnsBadRequest()
    {
        using var response = await PostAsync("invalid-base64.pdf", "not base64", expectedTotal: null, PdfStatementShapeIds.IcbcVisaLikeV1);
        using var json = await ReadJsonAsync(response);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("invalid_pdf_base64", json.RootElement.GetProperty("fileErrors")[0].GetProperty("code").GetString());
    }

    [Fact]
    public async Task FileOverFiveMegabytesReturnsBadRequest()
    {
        var oversizedContent = Convert.ToBase64String(new byte[(5 * 1024 * 1024) + 1]);

        using var response = await PostAsync("too-large.pdf", oversizedContent, expectedTotal: null, PdfStatementShapeIds.IcbcVisaLikeV1);
        using var json = await ReadJsonAsync(response);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("pdf_too_large", json.RootElement.GetProperty("fileErrors")[0].GetProperty("code").GetString());
    }

    [Fact]
    public async Task UnsupportedStatementShapeHintReturnsBadRequest()
    {
        using var response = await PostFixtureAsync(
            "icbc-visa-like-v1.pdf",
            "icbc-visa-like-v1.synthetic.pdf",
            expectedTotal: null,
            statementShapeHint: "unsupported-shape");
        using var json = await ReadJsonAsync(response);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("unsupported_statement_shape_hint", json.RootElement.GetProperty("fileErrors")[0].GetProperty("code").GetString());
    }

    [Fact]
    public async Task MalformedPdfInputReturnsBadRequest()
    {
        var malformedPdf = Convert.ToBase64String("not a pdf"u8.ToArray());

        using var response = await PostAsync("malformed.pdf", malformedPdf, expectedTotal: null, PdfStatementShapeIds.IcbcVisaLikeV1);
        using var json = await ReadJsonAsync(response);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("pdf_extraction_failed", json.RootElement.GetProperty("fileErrors")[0].GetProperty("code").GetString());
    }

    private static async Task<HttpResponseMessage> PostFixtureAsync(
        string fixtureFileName,
        string? sourceName,
        decimal? expectedTotal,
        string? statementShapeHint)
    {
        var content = await File.ReadAllBytesAsync(GetPdfFixturePath(fixtureFileName));

        return await PostAsync(sourceName, Convert.ToBase64String(content), expectedTotal, statementShapeHint);
    }

    private static async Task<HttpResponseMessage> PostAsync(
        string? sourceName,
        string? pdfBase64,
        decimal? expectedTotal,
        string? statementShapeHint)
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        return await client.PostAsJsonAsync(Endpoint, new
        {
            sourceName,
            expectedTotal,
            pdfBase64,
            statementShapeHint
        });
    }

    private static async Task<JsonDocument> ReadJsonAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        return JsonDocument.Parse(content);
    }

    private static int GetInt(JsonElement root, string section, string property) =>
        root.GetProperty(section).GetProperty(property).GetInt32();

    private static decimal GetDecimal(JsonElement root, string section, string property) =>
        root.GetProperty(section).GetProperty(property).GetDecimal();

    private static string GetPdfFixturePath(string fileName) =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "testdata", "pdf", fileName));
}
