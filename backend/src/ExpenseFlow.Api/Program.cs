using ExpenseFlow.Api.Endpoints;
using ExpenseFlow.Application.Abstractions;
using ExpenseFlow.Application.ExpenseReports.Categorization;
using ExpenseFlow.Application.ExpenseReports.Processing;
using ExpenseFlow.Application.ExpenseReports.Reporting;
using ExpenseFlow.Infrastructure.Parsing;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<ITransactionFileParser, CsvTransactionFileParser>();
builder.Services.AddScoped<ICategorizationRuleEngine, SeedDeterministicCategorizationRuleEngine>();
builder.Services.AddScoped<IExpenseReportGenerator, DeterministicExpenseReportGenerator>();
builder.Services.AddScoped<IExpenseReportProcessingService, DeterministicExpenseReportProcessingService>();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new HealthResponse("Healthy")));
app.MapExpenseReportEndpoints();

app.Run();

internal sealed record HealthResponse(string Status);

public partial class Program;
