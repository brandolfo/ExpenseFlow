namespace ExpenseFlow.Api.Contracts;

public sealed record ProcessExpenseReportRequest(
    string? SourceName,
    decimal? ExpectedTotal,
    string? CsvText);

public sealed record ProcessPdfExpenseReportRequest(
    string? SourceName,
    decimal? ExpectedTotal,
    string? PdfBase64,
    string? StatementShapeHint);

public sealed record ProcessExpenseReportResponse(
    ApiReportMetadata ReportMetadata,
    ApiProcessingCounts ProcessingCounts,
    ApiTotals Totals,
    ApiTotalValidation TotalValidation,
    IReadOnlyCollection<ApiCategorySummary> CategorySummary,
    IReadOnlyCollection<ApiTransactionDetail> TransactionDetails,
    IReadOnlyCollection<ApiReviewItem> ReviewItems,
    IReadOnlyCollection<ApiInvalidRow> InvalidRows,
    IReadOnlyCollection<ApiExcludedRow> ExcludedRows,
    ApiAuditSummary AuditSummary);

public sealed record ProcessPdfExpenseReportResponse(
    ApiPdfExtractionMetadata ExtractionMetadata,
    IReadOnlyCollection<ApiPdfExtractionWarning> ExtractionWarnings,
    ProcessExpenseReportResponse Report);

public sealed record ProcessExpenseReportErrorResponse(
    string Message,
    IReadOnlyCollection<ApiFileError> FileErrors);

public sealed record ApiFileError(
    string Code,
    string Message,
    IReadOnlyCollection<string> Details);

public sealed record ApiPdfExtractionMetadata(
    string SourceName,
    string StatementShapeId,
    string ExtractionStatus,
    int NormalizedRowCount,
    int InvalidExtractedRowCount,
    int UnprocessableNormalizedRowCount,
    int SourceRowCount,
    bool AiUsed);

public sealed record ApiPdfExtractionWarning(
    string Code,
    string Message,
    int? SourcePage,
    int? ExtractionOrder);

public sealed record ApiReportMetadata(
    string Product,
    string ReportType,
    string InputFormat,
    string SourceName,
    DateTimeOffset GeneratedAtUtc,
    bool UsesRealFinancialData);

public sealed record ApiProcessingCounts(
    int SourceRows,
    int ValidRows,
    int CategorizedRows,
    int ReviewRequiredRows,
    int InvalidRows,
    int ExcludedFromTotalsRows,
    int PotentialDuplicateRows);

public sealed record ApiTotals(
    decimal? ExpectedTotal,
    decimal ProcessedTotal,
    decimal CategoryTotal,
    decimal ExcludedFromTotalsTotal);

public sealed record ApiTotalValidation(
    string Status,
    decimal? ExpectedTotal,
    decimal ProcessedTotal,
    decimal? Difference);

public sealed record ApiCategorySummary(
    string Category,
    int TransactionCount,
    decimal Total);

public sealed record ApiTransactionDetail(
    int SourceRow,
    string? Date,
    string? Description,
    decimal? Amount,
    string Status,
    string? Category,
    string? ReviewReason,
    bool IncludedInProcessedTotal,
    bool IncludedInCategoryTotals,
    bool RequiresReview,
    bool IsPotentialDuplicate,
    string? Installment,
    IReadOnlyCollection<string> AppliedRules);

public sealed record ApiReviewItem(
    int SourceRow,
    string? Description,
    decimal? Amount,
    string? Reason,
    string? Category,
    IReadOnlyCollection<string> AppliedRules,
    bool IncludedInProcessedTotal,
    bool IncludedInCategoryTotals,
    bool IsPotentialDuplicate);

public sealed record ApiInvalidRow(
    int SourceRow,
    ApiRawValues RawValues,
    IReadOnlyCollection<string> Errors,
    bool IncludedInProcessedTotal,
    bool IncludedInCategoryTotals);

public sealed record ApiExcludedRow(
    int SourceRow,
    string? Description,
    decimal? Amount,
    string Status,
    string? Reason,
    IReadOnlyCollection<string> AppliedRules);

public sealed record ApiRawValues(
    string? Date,
    string? Code,
    string? Description,
    string? Amount,
    string? Installment,
    string? SourceType,
    string? Notes);

public sealed record ApiAuditSummary(
    ApiProcessingCounts Counts,
    string CompletenessStatus,
    int AppliedDeterministicRuleCount,
    string ExpectedTotalValidationStatus,
    bool AiUsed,
    IReadOnlyCollection<string> Messages,
    IReadOnlyCollection<ApiAuditEntry> Entries);

public sealed record ApiAuditEntry(
    int SourceRow,
    string EventType,
    string Message,
    string? RuleId);
