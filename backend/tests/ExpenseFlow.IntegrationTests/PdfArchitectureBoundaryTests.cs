using System.Net;
using System.Net.Http.Json;
using ExpenseFlow.Application.Abstractions;
using ExpenseFlow.Infrastructure.Files;
using ExpenseFlow.Infrastructure.Pdf;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ExpenseFlow.IntegrationTests;

public sealed class PdfArchitectureBoundaryTests
{
    [Fact]
    public async Task ApiExposesPdfProcessingEndpointForPdf6()
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

        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public void InfrastructureContainsPdfPigExtractorForPdf3()
    {
        var infrastructureTypes = typeof(InfrastructureAssemblyMarker)
            .Assembly
            .GetTypes()
            .Where(type => type.Namespace?.StartsWith("ExpenseFlow.Infrastructure", StringComparison.Ordinal) == true)
            .ToArray();

        Assert.Contains(typeof(PdfPigPdfStatementExtractor), infrastructureTypes);
        Assert.True(typeof(IPdfStatementExtractor).IsAssignableFrom(typeof(PdfPigPdfStatementExtractor)));
    }

    [Fact]
    public void ProductionProjectsDoNotReferencePdfLibrariesOrAiProviders()
    {
        var backendDirectory = GetBackendDirectory();
        var projectFiles = Directory.EnumerateFiles(Path.Combine(backendDirectory, "src"), "*.csproj", SearchOption.AllDirectories);
        var forbiddenTerms = new[] { "QuestPDF", "OpenAI", "OpenAi", "Tesseract", "DocumentIntelligence" };

        foreach (var projectFile in projectFiles)
        {
            var text = File.ReadAllText(projectFile);

            foreach (var term in forbiddenTerms)
            {
                Assert.DoesNotContain(term, text, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    [Fact]
    public void PdfPigReferenceIsIsolatedToInfrastructureProject()
    {
        var backendDirectory = GetBackendDirectory();
        var sourceProjectFiles = Directory.EnumerateFiles(Path.Combine(backendDirectory, "src"), "*.csproj", SearchOption.AllDirectories);

        var pdfPigProjects = sourceProjectFiles
            .Where(projectFile => File.ReadAllText(projectFile).Contains("PdfPig", StringComparison.OrdinalIgnoreCase))
            .Select(projectFile => Path.GetRelativePath(backendDirectory, projectFile).Replace('\\', '/'))
            .ToArray();

        Assert.Equal(["src/ExpenseFlow.Infrastructure/ExpenseFlow.Infrastructure.csproj"], pdfPigProjects);
    }

    [Fact]
    public void ApplicationDomainAndApiProjectsDoNotReferencePdfPig()
    {
        var backendDirectory = GetBackendDirectory();
        var forbiddenProjects = new[]
        {
            "src/ExpenseFlow.Application/ExpenseFlow.Application.csproj",
            "src/ExpenseFlow.Domain/ExpenseFlow.Domain.csproj",
            "src/ExpenseFlow.Api/ExpenseFlow.Api.csproj"
        };

        foreach (var projectFile in forbiddenProjects)
        {
            var text = File.ReadAllText(Path.Combine(backendDirectory, projectFile));

            Assert.DoesNotContain("PdfPig", text, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ApiSourceDoesNotReferencePdfPigTypesDirectly()
    {
        var apiDirectory = Path.Combine(GetBackendDirectory(), "src", "ExpenseFlow.Api");
        var apiSourceFiles = Directory.EnumerateFiles(apiDirectory, "*.cs", SearchOption.AllDirectories);

        foreach (var sourceFile in apiSourceFiles)
        {
            var text = File.ReadAllText(sourceFile);

            Assert.DoesNotContain("PdfPig", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("UglyToad", text, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void SyntheticPdfGeneratorIsOnlyProjectAllowedToReferenceQuestPdf()
    {
        var backendDirectory = GetBackendDirectory();
        var projectFiles = Directory.EnumerateFiles(backendDirectory, "*.csproj", SearchOption.AllDirectories);

        var questPdfProjects = projectFiles
            .Where(projectFile => File.ReadAllText(projectFile).Contains("QuestPDF", StringComparison.OrdinalIgnoreCase))
            .Select(projectFile => Path.GetRelativePath(backendDirectory, projectFile).Replace('\\', '/'))
            .ToArray();

        Assert.Equal(["tools/ExpenseFlow.SyntheticPdfGenerator/ExpenseFlow.SyntheticPdfGenerator.csproj"], questPdfProjects);
    }

    [Fact]
    public void SyntheticPdfFixtureFilesAndExpectedRowsExistUnderPdfTestdata()
    {
        var pdfFixtureDirectory = Path.Combine(GetBackendDirectory(), "testdata", "pdf");
        var requiredFiles = new[]
        {
            "icbc-visa-like-v1.pdf",
            "icbc-mastercard-like-v1.pdf",
            "icbc-visa-like-v1.expected-normalized-rows.csv",
            "icbc-mastercard-like-v1.expected-normalized-rows.csv"
        };

        foreach (var requiredFile in requiredFiles)
        {
            var path = Path.Combine(pdfFixtureDirectory, requiredFile);

            Assert.True(File.Exists(path), $"Missing PDF fixture file: {path}");
            Assert.StartsWith(pdfFixtureDirectory, Path.GetFullPath(path), StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void PdfTestdataContainsOnlyApprovedSyntheticPublicAssets()
    {
        var pdfFixtureDirectory = Path.Combine(GetBackendDirectory(), "testdata", "pdf");
        var approvedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "README.md",
            "icbc-visa-like-v1.fixture-spec.md",
            "icbc-visa-like-v1.expected-normalized-rows.csv",
            "icbc-visa-like-v1.pdf",
            "icbc-mastercard-like-v1.fixture-spec.md",
            "icbc-mastercard-like-v1.expected-normalized-rows.csv",
            "icbc-mastercard-like-v1.pdf"
        };

        var actualFiles = Directory.EnumerateFiles(pdfFixtureDirectory, "*", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(pdfFixtureDirectory, path).Replace('\\', '/'))
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        Assert.Equal(approvedFiles.Order(StringComparer.OrdinalIgnoreCase), actualFiles);
        Assert.DoesNotContain(actualFiles, path => path.Contains("private", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(actualFiles, path => path.Contains("real", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(actualFiles, path => path.EndsWith(".json", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(actualFiles, path => path.EndsWith(".txt", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(actualFiles, path => path.EndsWith(".png", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(actualFiles, path => path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(actualFiles, path => path.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase));
    }

    private static string GetBackendDirectory() =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
}
