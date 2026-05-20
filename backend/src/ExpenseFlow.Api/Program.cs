using ExpenseFlow.Api.Endpoints;
using ExpenseFlow.Application.Abstractions;
using ExpenseFlow.Application.ExpenseReports.Categorization;
using ExpenseFlow.Application.ExpenseReports.Pdf;
using ExpenseFlow.Application.ExpenseReports.PdfProcessing;
using ExpenseFlow.Application.ExpenseReports.Processing;
using ExpenseFlow.Application.ExpenseReports.Reporting;
using ExpenseFlow.Infrastructure.Files;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped(InfrastructureAdapters.CreateTransactionFileParser);
builder.Services.AddScoped(InfrastructureAdapters.CreatePdfStatementExtractor);
builder.Services.AddScoped<IPdfStatementRowNormalizer, DeterministicPdfStatementRowNormalizer>();
builder.Services.AddScoped<ICategorizationRuleEngine, SeedDeterministicCategorizationRuleEngine>();
builder.Services.AddScoped<IExpenseReportGenerator, DeterministicExpenseReportGenerator>();
builder.Services.AddScoped<IExpenseReportProcessingService, DeterministicExpenseReportProcessingService>();
builder.Services.AddScoped<IPdfExpenseReportProcessingService, DeterministicPdfExpenseReportProcessingService>();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new HealthResponse("Healthy")));
app.MapExpenseReportEndpoints();

app.Run();

internal sealed record HealthResponse(string Status);

public partial class Program;
