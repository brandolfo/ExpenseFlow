using System.Net;
using System.Net.Http.Json;
using ExpenseFlow.Infrastructure.Files;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ExpenseFlow.IntegrationTests;

public sealed class PdfArchitectureBoundaryTests
{
    [Fact]
    public async Task ApiDoesNotExposePdfProcessingEndpointYet()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync("/api/expense-reports/process-pdf", new
        {
            sourceName = "synthetic.pdf",
            expectedTotal = 0m,
            pdfBase64 = "JVBERi0xLjQ=",
            statementShapeHint = "icbc-visa-like-v1"
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public void InfrastructureDoesNotContainRealPdfExtractorYet()
    {
        var infrastructureTypes = typeof(InfrastructureAssemblyMarker)
            .Assembly
            .GetTypes()
            .Where(type => type.Namespace?.StartsWith("ExpenseFlow.Infrastructure", StringComparison.Ordinal) == true)
            .ToArray();

        Assert.DoesNotContain(infrastructureTypes, type => type.Name.Contains("Pdf", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(infrastructureTypes, type => type.Name.Contains("StatementExtractor", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void BackendProjectsDoNotReferencePdfLibrariesOrAiProviders()
    {
        var backendDirectory = GetBackendDirectory();
        var projectFiles = Directory.EnumerateFiles(backendDirectory, "*.csproj", SearchOption.AllDirectories);
        var forbiddenTerms = new[] { "PdfPig", "QuestPDF", "OpenAI", "OpenAi", "Tesseract", "DocumentIntelligence" };

        foreach (var projectFile in projectFiles)
        {
            var text = File.ReadAllText(projectFile);

            foreach (var term in forbiddenTerms)
            {
                Assert.DoesNotContain(term, text, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    private static string GetBackendDirectory() =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
}
