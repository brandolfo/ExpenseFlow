using ExpenseFlow.Application.ExpenseReports.Parsing;
using ExpenseFlow.Domain.ExpenseReports;

namespace ExpenseFlow.Application.ExpenseReports.Processing;

public sealed record ExpenseReportProcessingResult
{
    private ExpenseReportProcessingResult(
        ExpenseReport? report,
        IReadOnlyCollection<FileValidationError> fileErrors)
    {
        Report = report;
        FileErrors = fileErrors;
    }

    public ExpenseReport? Report { get; }

    public IReadOnlyCollection<FileValidationError> FileErrors { get; }

    public bool IsSuccess => FileErrors.Count == 0 && Report is not null;

    public static ExpenseReportProcessingResult Success(ExpenseReport report) =>
        new(report, Array.Empty<FileValidationError>());

    public static ExpenseReportProcessingResult Failure(IReadOnlyCollection<FileValidationError> fileErrors) =>
        new(null, fileErrors);
}
