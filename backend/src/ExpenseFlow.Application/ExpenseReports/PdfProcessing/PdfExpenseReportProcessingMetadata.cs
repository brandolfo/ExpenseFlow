using ExpenseFlow.Application.ExpenseReports.Pdf;

namespace ExpenseFlow.Application.ExpenseReports.PdfProcessing;

public sealed record PdfExpenseReportProcessingMetadata(
    string SourceName,
    string StatementShapeId,
    PdfExtractionStatus ExtractionStatus,
    int NormalizedRowCount,
    int InvalidExtractedRowCount,
    int UnprocessableNormalizedRowCount,
    int SourceRowCount,
    bool AiUsed);
