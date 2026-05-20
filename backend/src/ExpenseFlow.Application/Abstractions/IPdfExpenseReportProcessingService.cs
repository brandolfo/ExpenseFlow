using ExpenseFlow.Application.ExpenseReports.PdfProcessing;

namespace ExpenseFlow.Application.Abstractions;

public interface IPdfExpenseReportProcessingService
{
    Task<PdfExpenseReportProcessingResult> ProcessAsync(
        PdfExpenseReportProcessingRequest request,
        CancellationToken cancellationToken = default);
}
