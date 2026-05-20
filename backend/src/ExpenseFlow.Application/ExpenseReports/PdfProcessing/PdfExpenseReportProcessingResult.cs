using ExpenseFlow.Application.ExpenseReports.Pdf;
using ExpenseFlow.Domain.ExpenseReports;

namespace ExpenseFlow.Application.ExpenseReports.PdfProcessing;

public sealed record PdfExpenseReportProcessingResult
{
    private PdfExpenseReportProcessingResult(
        ExpenseReport? report,
        PdfExpenseReportProcessingMetadata metadata,
        IReadOnlyCollection<PdfExtractionWarning> extractionWarnings)
    {
        Report = report;
        Metadata = metadata;
        ExtractionWarnings = extractionWarnings;
    }

    public ExpenseReport? Report { get; }

    public PdfExpenseReportProcessingMetadata Metadata { get; }

    public IReadOnlyCollection<PdfExtractionWarning> ExtractionWarnings { get; }

    public bool IsSuccess => Report is not null && Metadata.ExtractionStatus is PdfExtractionStatus.Succeeded or PdfExtractionStatus.Partial;

    public static PdfExpenseReportProcessingResult Success(
        ExpenseReport report,
        PdfExpenseReportProcessingMetadata metadata,
        IReadOnlyCollection<PdfExtractionWarning> extractionWarnings) =>
        new(report, metadata, extractionWarnings);

    public static PdfExpenseReportProcessingResult Failure(
        PdfExpenseReportProcessingMetadata metadata,
        IReadOnlyCollection<PdfExtractionWarning> extractionWarnings) =>
        new(null, metadata, extractionWarnings);
}
