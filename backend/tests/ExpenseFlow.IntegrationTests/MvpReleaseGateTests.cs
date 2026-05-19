using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ExpenseFlow.IntegrationTests;

public sealed partial class MvpReleaseGateTests
{
    private const string Endpoint = "/api/expense-reports/process";

    [Fact]
    public async Task DemoMainEndpointAccountsForEverySourceRowWithoutDroppingRows()
    {
        using var response = await PostFixtureAsync("demo-main.csv", expectedTotal: 258248m);
        using var json = await ReadJsonAsync(response);
        var root = json.RootElement;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(22, GetInt(root, "processingCounts", "sourceRows"));
        Assert.Equal(22, root.GetProperty("transactionDetails").GetArrayLength());

        var transactionRows = SourceRows(root.GetProperty("transactionDetails"));
        Assert.Equal(Enumerable.Range(1, 22).ToArray(), transactionRows);
        Assert.Equal([11, 12, 13, 14, 15, 19], SourceRows(root.GetProperty("reviewItems")));
        Assert.Equal([20, 21, 22], SourceRows(root.GetProperty("invalidRows")));
        Assert.Equal([14, 15], SourceRows(root.GetProperty("excludedRows")));

        Assert.All(root.GetProperty("invalidRows").EnumerateArray(), row =>
        {
            Assert.False(row.GetProperty("includedInProcessedTotal").GetBoolean());
            Assert.False(row.GetProperty("includedInCategoryTotals").GetBoolean());
        });
        Assert.All(root.GetProperty("excludedRows").EnumerateArray(), row =>
            Assert.Contains(row.GetProperty("sourceRow").GetInt32(), new[] { 14, 15 }));
    }

    [Fact]
    public async Task DemoHappyPathEndpointMatchesExpectedTotalsAndCategorySummary()
    {
        using var response = await PostFixtureAsync("demo-happy-path.csv", expectedTotal: 179349m);
        using var json = await ReadJsonAsync(response);
        var root = json.RootElement;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(13, GetInt(root, "processingCounts", "sourceRows"));
        Assert.Equal(13, GetInt(root, "processingCounts", "categorizedRows"));
        Assert.Equal(0, GetInt(root, "processingCounts", "reviewRequiredRows"));
        Assert.Equal(0, GetInt(root, "processingCounts", "invalidRows"));
        Assert.Equal(179349m, GetDecimal(root, "totals", "processedTotal"));
        Assert.Equal(179349m, GetDecimal(root, "totals", "categoryTotal"));
        Assert.Equal("Match", root.GetProperty("totalValidation").GetProperty("status").GetString());

        var categoryTotals = root.GetProperty("categorySummary")
            .EnumerateArray()
            .ToDictionary(
                item => item.GetProperty("category").GetString()!,
                item => item.GetProperty("total").GetDecimal());

        Assert.Equal(44300m, categoryTotals["Groceries"]);
        Assert.Equal(4200m, categoryTotals["RestaurantsAndCafes"]);
        Assert.Equal(850m, categoryTotals["Transport"]);
        Assert.Equal(18700m, categoryTotals["HousingAndUtilities"]);
        Assert.Equal(12999m, categoryTotals["SubscriptionsAndSoftware"]);
        Assert.Equal(7600m, categoryTotals["HealthAndPharmacy"]);
        Assert.Equal(9200m, categoryTotals["Education"]);
        Assert.Equal(6500m, categoryTotals["Entertainment"]);
        Assert.Equal(42500m, categoryTotals["Travel"]);
        Assert.Equal(2500m, categoryTotals["FeesAndTaxes"]);
        Assert.Equal(30000m, categoryTotals["Shopping"]);
    }

    [Fact]
    public async Task UnsupportedSpreadsheetLikeInputReturnsFileLevelBadRequest()
    {
        const string spreadsheetLikePayload = "PK\u0003\u0004 synthetic xlsx-like content";

        using var response = await PostAsync("demo.xlsx", spreadsheetLikePayload, expectedTotal: 0m);
        using var json = await ReadJsonAsync(response);
        var root = json.RootElement;

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("CSV input could not be processed.", root.GetProperty("message").GetString());
        Assert.Contains(
            root.GetProperty("fileErrors").EnumerateArray(),
            error => error.GetProperty("code").GetString() == "MissingRequiredColumns");
    }

    [Fact]
    public async Task PublicFixturesRemainSyntheticAndFreeOfPrivateFinancialPatterns()
    {
        foreach (var path in Directory.EnumerateFiles(GetFixtureDirectory(), "demo-*.csv"))
        {
            var text = await File.ReadAllTextAsync(path);

            Assert.Contains("Synthetic", text);
            Assert.DoesNotMatch(LongDigitSequence(), text);
            Assert.DoesNotMatch(EmailLikeValue(), text);
            Assert.DoesNotContain("account", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("address", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("tax id", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("password", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("secret", text, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void MvpProjectsDoNotReferenceOutOfScopeInfrastructurePackages()
    {
        var backendDirectory = GetBackendDirectory();
        var projectFiles = Directory.EnumerateFiles(backendDirectory, "*.csproj", SearchOption.AllDirectories);
        var forbiddenTerms = new[]
        {
            "OpenAI",
            "OpenAi",
            "EntityFramework",
            "DbContext",
            "Authentication",
            "Authorization",
            "IHostedService",
            "BackgroundService",
            "Docker"
        };

        foreach (var projectFile in projectFiles)
        {
            var text = File.ReadAllText(projectFile);

            foreach (var term in forbiddenTerms)
            {
                Assert.DoesNotContain(term, text, StringComparison.OrdinalIgnoreCase);
            }
        }
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

    private static int[] SourceRows(JsonElement rows) =>
        rows.EnumerateArray()
            .Select(row => row.GetProperty("sourceRow").GetInt32())
            .Order()
            .ToArray();

    private static int GetInt(JsonElement root, string section, string property) =>
        root.GetProperty(section).GetProperty(property).GetInt32();

    private static decimal GetDecimal(JsonElement root, string section, string property) =>
        root.GetProperty(section).GetProperty(property).GetDecimal();

    private static string GetFixturePath(string fileName) =>
        Path.Combine(GetFixtureDirectory(), fileName);

    private static string GetFixtureDirectory() =>
        Path.Combine(GetBackendDirectory(), "testdata");

    private static string GetBackendDirectory() =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    [GeneratedRegex(@"\d{12,}")]
    private static partial Regex LongDigitSequence();

    [GeneratedRegex(@"[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}", RegexOptions.IgnoreCase)]
    private static partial Regex EmailLikeValue();
}
