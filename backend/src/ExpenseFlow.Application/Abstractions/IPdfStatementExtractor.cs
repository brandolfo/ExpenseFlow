using ExpenseFlow.Application.ExpenseReports.Pdf;

namespace ExpenseFlow.Application.Abstractions;

public interface IPdfStatementExtractor
{
    Task<PdfStatementExtractionResult> ExtractAsync(
        PdfStatementExtractionRequest request,
        CancellationToken cancellationToken = default);
}
