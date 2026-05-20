using ExpenseFlow.Application.ExpenseReports.Pdf;

namespace ExpenseFlow.Application.Abstractions;

public interface IPdfStatementRowNormalizer
{
    PdfStatementExtractionResult Normalize(PdfStatementExtractionResult extractionResult);
}
