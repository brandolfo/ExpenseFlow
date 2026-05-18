using ExpenseFlow.Application.ExpenseReports.Processing;

namespace ExpenseFlow.Application.Abstractions;

public interface IExpenseReportProcessingService
{
    Task<ExpenseReportProcessingResult> ProcessAsync(
        ExpenseReportProcessingRequest request,
        CancellationToken cancellationToken = default);
}
