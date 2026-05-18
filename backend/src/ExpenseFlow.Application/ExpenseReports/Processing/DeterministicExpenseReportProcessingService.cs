using ExpenseFlow.Application.Abstractions;
using ExpenseFlow.Application.ExpenseReports.Reporting;

namespace ExpenseFlow.Application.ExpenseReports.Processing;

public sealed class DeterministicExpenseReportProcessingService : IExpenseReportProcessingService
{
    private readonly ITransactionFileParser _parser;
    private readonly ICategorizationRuleEngine _categorizationRuleEngine;
    private readonly IExpenseReportGenerator _reportGenerator;

    public DeterministicExpenseReportProcessingService(
        ITransactionFileParser parser,
        ICategorizationRuleEngine categorizationRuleEngine,
        IExpenseReportGenerator reportGenerator)
    {
        _parser = parser;
        _categorizationRuleEngine = categorizationRuleEngine;
        _reportGenerator = reportGenerator;
    }

    public async Task<ExpenseReportProcessingResult> ProcessAsync(
        ExpenseReportProcessingRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var parseResult = await _parser.ParseAsync(
            request.CsvText,
            request.SourceName,
            cancellationToken);

        if (!parseResult.IsSuccess)
        {
            return ExpenseReportProcessingResult.Failure(parseResult.FileErrors);
        }

        var transactions = _categorizationRuleEngine.Categorize(parseResult.ValidRows);
        var report = _reportGenerator.Generate(new ExpenseReportGenerationInput(
            parseResult.SourceName,
            parseResult.SourceRowCount,
            transactions,
            parseResult.InvalidRows,
            request.ExpectedTotal));

        return ExpenseReportProcessingResult.Success(report);
    }
}
