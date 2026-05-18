using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ExpenseFlow.IntegrationTests;

public sealed class ProcessExpenseReportEndpointTests
{
    private const string Endpoint = "/api/expense-reports/process";

    [Fact]
    public async Task DemoMainReturnsOkWithExpectedReportStructure()
    {
        using var response = await PostFixtureAsync("demo-main.csv", expectedTotal: 258248m);
        using var json = await ReadJsonAsync(response);
        var root = json.RootElement;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("ExpenseFlow", root.GetProperty("reportMetadata").GetProperty("product").GetString());
        Assert.Equal("demo-main.csv", root.GetProperty("reportMetadata").GetProperty("sourceName").GetString());
        Assert.Equal(22, GetInt(root, "processingCounts", "sourceRows"));
        Assert.Equal(19, GetInt(root, "processingCounts", "validRows"));
        Assert.Equal(6, GetInt(root, "processingCounts", "reviewRequiredRows"));
        Assert.Equal(3, GetInt(root, "processingCounts", "invalidRows"));
        Assert.Equal(2, GetInt(root, "processingCounts", "excludedFromTotalsRows"));
        Assert.Equal(1, GetInt(root, "processingCounts", "potentialDuplicateRows"));
        Assert.Equal(258248m, GetDecimal(root, "totals", "processedTotal"));
        Assert.Equal(189149m, GetDecimal(root, "totals", "categoryTotal"));
        Assert.Equal("Match", root.GetProperty("totalValidation").GetProperty("status").GetString());
        Assert.Equal(22, root.GetProperty("transactionDetails").GetArrayLength());
        Assert.Equal(6, root.GetProperty("reviewItems").GetArrayLength());
        Assert.Equal(3, root.GetProperty("invalidRows").GetArrayLength());
        Assert.Equal(2, root.GetProperty("excludedRows").GetArrayLength());
        Assert.True(root.GetProperty("auditSummary").TryGetProperty("messages", out _));
        Assert.False(root.GetProperty("auditSummary").GetProperty("aiUsed").GetBoolean());
    }

    [Fact]
    public async Task DemoMainResponseIncludesCategorySummaryReviewExcludedAndAuditDetails()
    {
        using var response = await PostFixtureAsync("demo-main.csv", expectedTotal: 258248m);
        using var json = await ReadJsonAsync(response);
        var root = json.RootElement;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(
            root.GetProperty("categorySummary").EnumerateArray(),
            item => item.GetProperty("category").GetString() == "Groceries" &&
                item.GetProperty("total").GetDecimal() == 54100m);
        Assert.Contains(
            root.GetProperty("reviewItems").EnumerateArray(),
            item => item.GetProperty("sourceRow").GetInt32() == 13 &&
                item.GetProperty("reason").GetString() == "CategoryConflict");
        Assert.Contains(
            root.GetProperty("excludedRows").EnumerateArray(),
            item => item.GetProperty("sourceRow").GetInt32() == 15 &&
                item.GetProperty("reason").GetString() == "TransferOrPayment");
        Assert.Contains(
            root.GetProperty("transactionDetails").EnumerateArray(),
            item => item.GetProperty("sourceRow").GetInt32() == 16 &&
                item.GetProperty("installment").GetString() == "03/06");
        Assert.Contains(
            root.GetProperty("transactionDetails").EnumerateArray(),
            item => item.GetProperty("sourceRow").GetInt32() == 19 &&
                item.GetProperty("isPotentialDuplicate").GetBoolean());
        Assert.Contains(
            root.GetProperty("auditSummary").GetProperty("entries").EnumerateArray(),
            item => item.GetProperty("sourceRow").GetInt32() == 19 &&
                item.GetProperty("eventType").GetString() == "PotentialDuplicate");
    }

    [Fact]
    public async Task DemoTotalMismatchReturnsOkWithMismatchValidation()
    {
        using var response = await PostFixtureAsync("demo-total-mismatch.csv", expectedTotal: 260000m);
        using var json = await ReadJsonAsync(response);
        var root = json.RootElement;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(258248m, GetDecimal(root, "totals", "processedTotal"));
        Assert.Equal("Mismatch", root.GetProperty("totalValidation").GetProperty("status").GetString());
        Assert.Equal(1752m, root.GetProperty("totalValidation").GetProperty("difference").GetDecimal());
    }

    [Fact]
    public async Task DemoInvalidRowsReturnsOkWithInvalidRowsVisible()
    {
        using var response = await PostFixtureAsync("demo-invalid-rows.csv", expectedTotal: 34500m);
        using var json = await ReadJsonAsync(response);
        var root = json.RootElement;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(4, GetInt(root, "processingCounts", "sourceRows"));
        Assert.Equal(3, root.GetProperty("invalidRows").GetArrayLength());
        Assert.Equal(34500m, GetDecimal(root, "totals", "processedTotal"));
        Assert.Contains(
            root.GetProperty("invalidRows").EnumerateArray(),
            item => item.GetProperty("rawValues").GetProperty("amount").GetString() == "abc" &&
                item.GetProperty("includedInProcessedTotal").GetBoolean() == false);
    }

    [Fact]
    public async Task MissingRequiredHeaderReturnsBadRequestWithStructuredError()
    {
        const string csv = """
            date,description,code
            2026-04-01,FRESHVALE MARKET DEMO,DMO-0001
            """;

        using var response = await PostAsync("missing-required.csv", csv, expectedTotal: 0m);
        using var json = await ReadJsonAsync(response);
        var error = json.RootElement.GetProperty("fileErrors")[0];

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("MissingRequiredColumns", error.GetProperty("code").GetString());
        Assert.Contains("amount", error.GetProperty("details").EnumerateArray().Select(item => item.GetString()));
    }

    [Fact]
    public async Task EmptyCsvTextReturnsBadRequest()
    {
        using var response = await PostAsync("empty.csv", "   ", expectedTotal: 0m);
        using var json = await ReadJsonAsync(response);
        var error = json.RootElement.GetProperty("fileErrors")[0];

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("EmptyInput", error.GetProperty("code").GetString());
    }

    [Fact]
    public async Task MalformedCsvReturnsBadRequestWithStructuredError()
    {
        const string csv = """
            date,description,amount
            2026-04-01,"FRESHVALE MARKET DEMO,34500.00
            """;

        using var response = await PostAsync("malformed.csv", csv, expectedTotal: 0m);
        using var json = await ReadJsonAsync(response);
        var error = json.RootElement.GetProperty("fileErrors")[0];

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("MalformedCsv", error.GetProperty("code").GetString());
    }

    [Fact]
    public async Task NullRequestBodyReturnsBadRequest()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync<object?>(Endpoint, null);
        using var json = await ReadJsonAsync(response);
        var error = json.RootElement.GetProperty("fileErrors")[0];

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("invalid_request", error.GetProperty("code").GetString());
    }

    [Fact]
    public async Task MissingExpectedTotalReturnsOkWithNotProvidedValidation()
    {
        using var response = await PostFixtureAsync("demo-main.csv", expectedTotal: null);
        using var json = await ReadJsonAsync(response);
        var root = json.RootElement;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("NotProvided", root.GetProperty("totalValidation").GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("totalValidation").GetProperty("expectedTotal").ValueKind);
        Assert.Equal(258248m, GetDecimal(root, "totals", "processedTotal"));
    }

    [Fact]
    public async Task MissingOptionalColumnsAreAcceptedThroughEndpoint()
    {
        const string csv = """
            date,description,amount
            2026-04-01,FRESHVALE MARKET DEMO,34500.00
            """;

        using var response = await PostAsync("required-only.csv", csv, expectedTotal: 34500m);
        using var json = await ReadJsonAsync(response);
        var root = json.RootElement;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, GetInt(root, "processingCounts", "sourceRows"));
        Assert.Equal("Match", root.GetProperty("totalValidation").GetProperty("status").GetString());
    }

    [Fact]
    public async Task EndpointDoesNotRequireAuthenticationOrExternalServices()
    {
        using var response = await PostFixtureAsync("demo-happy-path.csv", expectedTotal: 179349m);
        using var json = await ReadJsonAsync(response);
        var root = json.RootElement;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(179349m, GetDecimal(root, "totals", "processedTotal"));
        Assert.False(root.GetProperty("auditSummary").GetProperty("aiUsed").GetBoolean());
    }

    private static async Task<HttpResponseMessage> PostFixtureAsync(string fileName, decimal? expectedTotal) =>
        await PostAsync(fileName, await File.ReadAllTextAsync(GetFixturePath(fileName)), expectedTotal);

    private static async Task<HttpResponseMessage> PostAsync(string sourceName, string csvText, decimal? expectedTotal)
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        return await client.PostAsJsonAsync(Endpoint, new
        {
            sourceName,
            expectedTotal,
            csvText
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

    private static string GetFixturePath(string fileName) =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "testdata", fileName));
}
