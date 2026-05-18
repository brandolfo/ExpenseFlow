using ExpenseFlow.Api.Contracts;
using ExpenseFlow.Application.Abstractions;
using ExpenseFlow.Application.ExpenseReports.Parsing;
using ExpenseFlow.Application.ExpenseReports.Processing;
using ExpenseFlow.Domain.ExpenseReports;
using ExpenseFlow.Domain.Transactions;

namespace ExpenseFlow.Api.Endpoints;

public static class ExpenseReportEndpoints
{
    public static IEndpointRouteBuilder MapExpenseReportEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/expense-reports/process", ProcessAsync);

        return app;
    }

    private static async Task<IResult> ProcessAsync(
        ProcessExpenseReportRequest? request,
        IExpenseReportProcessingService processingService,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var requestErrors = ValidateRequest(request);

        if (requestErrors.Count > 0)
        {
            return Results.BadRequest(new ProcessExpenseReportErrorResponse(
                "Expense report request is invalid.",
                requestErrors));
        }

        try
        {
            var result = await processingService.ProcessAsync(
                new ExpenseReportProcessingRequest(
                    request!.SourceName!.Trim(),
                    request.CsvText!,
                    request.ExpectedTotal),
                cancellationToken);

            if (!result.IsSuccess)
            {
                return Results.BadRequest(new ProcessExpenseReportErrorResponse(
                    "CSV input could not be processed.",
                    result.FileErrors.Select(MapFileError).ToArray()));
            }

            return Results.Ok(MapResponse(result.Report!));
        }
        catch (Exception exception)
        {
            var logger = loggerFactory.CreateLogger("ExpenseReportProcessing");
            logger.LogError(exception, "Unexpected error while processing expense report.");

            return Results.Problem(
                title: "Expense report processing failed.",
                detail: "An unexpected error occurred while processing the request.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static IReadOnlyCollection<ApiFileError> ValidateRequest(ProcessExpenseReportRequest? request)
    {
        if (request is null)
        {
            return
            [
                new ApiFileError(
                    "invalid_request",
                    "Request body is required.",
                    [])
            ];
        }

        var errors = new List<ApiFileError>();

        if (string.IsNullOrWhiteSpace(request.SourceName))
        {
            errors.Add(new ApiFileError(
                "missing_source_name",
                "sourceName is required.",
                []));
        }

        if (string.IsNullOrWhiteSpace(request.CsvText))
        {
            errors.Add(new ApiFileError(
                FileValidationErrorCode.EmptyInput.ToString(),
                "csvText is required.",
                []));
        }

        return errors;
    }

    private static ProcessExpenseReportResponse MapResponse(ExpenseReport report) =>
        new(
            new ApiReportMetadata(
                report.Metadata.Product,
                report.Metadata.ReportType,
                report.Metadata.InputFormat,
                report.Metadata.SourceName,
                report.Metadata.GeneratedAtUtc,
                report.Metadata.UsesRealFinancialData),
            MapCounts(report.AuditSummary.Counts),
            new ApiTotals(
                report.Totals.ExpectedTotal,
                report.Totals.ProcessedTotal,
                report.Totals.CategoryTotal,
                report.Totals.ExcludedFromTotalsTotal),
            new ApiTotalValidation(
                report.ValidationResult.Status.ToString(),
                report.ValidationResult.ExpectedTotal,
                report.ValidationResult.ProcessedTotal,
                report.ValidationResult.Difference),
            report.CategoryTotals.Select(total => new ApiCategorySummary(
                total.Category.ToString(),
                total.TransactionCount,
                total.Total)).ToArray(),
            report.Transactions.Select(MapTransactionDetail).ToArray(),
            report.ReviewItems.Select(MapReviewItem).ToArray(),
            report.InvalidRows.Select(MapInvalidRow).ToArray(),
            report.ExcludedRows.Select(MapExcludedRow).ToArray(),
            new ApiAuditSummary(
                MapCounts(report.AuditSummary.Counts),
                report.AuditSummary.CompletenessStatus.ToString(),
                report.AuditSummary.AppliedDeterministicRuleCount,
                report.AuditSummary.ExpectedTotalValidationStatus.ToString(),
                report.AuditSummary.AiUsed,
                report.AuditSummary.Messages,
                report.AuditEntries.Select(entry => new ApiAuditEntry(
                    entry.SourceRowNumber,
                    entry.EventType.ToString(),
                    entry.Message,
                    entry.RuleId)).ToArray()));

    private static ApiProcessingCounts MapCounts(ProcessingCounts counts) =>
        new(
            counts.SourceRows,
            counts.ValidRows,
            counts.CategorizedRows,
            counts.ReviewRequiredRows,
            counts.InvalidRows,
            counts.ExcludedFromTotalsRows,
            counts.PotentialDuplicateRows);

    private static ApiTransactionDetail MapTransactionDetail(ExpenseTransaction transaction) =>
        new(
            transaction.SourceRow.RowNumber,
            transaction.Date?.ToString("yyyy-MM-dd"),
            transaction.Description,
            transaction.Amount,
            transaction.Status.ToString(),
            transaction.Category?.ToString(),
            transaction.ReviewReason?.ToString(),
            transaction.IncludedInProcessedTotal,
            transaction.IncludedInCategoryTotals,
            transaction.RequiresReview,
            transaction.IsPotentialDuplicate,
            transaction.Installment,
            transaction.RuleMatches.Select(match => match.RuleId).ToArray());

    private static ApiReviewItem MapReviewItem(ExpenseTransaction transaction) =>
        new(
            transaction.SourceRow.RowNumber,
            transaction.Description,
            transaction.Amount,
            transaction.ReviewReason?.ToString(),
            transaction.Category?.ToString(),
            transaction.RuleMatches.Select(match => match.RuleId).ToArray(),
            transaction.IncludedInProcessedTotal,
            transaction.IncludedInCategoryTotals,
            transaction.IsPotentialDuplicate);

    private static ApiInvalidRow MapInvalidRow(ExpenseTransaction transaction) =>
        new(
            transaction.SourceRow.RowNumber,
            new ApiRawValues(
                transaction.SourceRow.RawDate,
                transaction.SourceRow.RawCode,
                transaction.SourceRow.RawDescription,
                transaction.SourceRow.RawAmount,
                transaction.SourceRow.RawInstallment,
                transaction.SourceRow.RawSourceType,
                transaction.SourceRow.RawNotes),
            transaction.ValidationIssues.Select(issue => issue.Message).ToArray(),
            transaction.IncludedInProcessedTotal,
            transaction.IncludedInCategoryTotals);

    private static ApiExcludedRow MapExcludedRow(ExpenseTransaction transaction) =>
        new(
            transaction.SourceRow.RowNumber,
            transaction.Description,
            transaction.Amount,
            transaction.Status.ToString(),
            transaction.ReviewReason?.ToString(),
            transaction.RuleMatches.Select(match => match.RuleId).ToArray());

    private static ApiFileError MapFileError(FileValidationError error) =>
        new(
            error.Code.ToString(),
            error.Message,
            error.Details ?? []);
}
