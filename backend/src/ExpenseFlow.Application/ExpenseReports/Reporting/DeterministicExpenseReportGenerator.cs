using ExpenseFlow.Application.Abstractions;
using ExpenseFlow.Application.ExpenseReports.Parsing;
using ExpenseFlow.Domain.Categorization;
using ExpenseFlow.Domain.ExpenseReports;
using ExpenseFlow.Domain.Transactions;
using ExpenseFlow.Domain.Validation;

namespace ExpenseFlow.Application.ExpenseReports.Reporting;

public sealed class DeterministicExpenseReportGenerator : IExpenseReportGenerator
{
    public ExpenseReport Generate(ExpenseReportGenerationInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input.SourceRowCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(input), "Source row count cannot be negative.");
        }

        var invalidTransactions = input.InvalidRows
            .Select(CreateInvalidTransaction)
            .ToArray();

        var allTransactions = input.TransactionOutcomes
            .Concat(invalidTransactions)
            .OrderBy(transaction => transaction.SourceRow.RowNumber)
            .ToArray();

        var processedTotal = SumAmounts(allTransactions.Where(transaction => transaction.IncludedInProcessedTotal));
        var categoryTotals = BuildCategoryTotals(allTransactions);
        var categoryTotal = categoryTotals.Sum(total => total.Total);
        var excludedTotal = SumAmounts(allTransactions.Where(transaction => transaction.Status == RowStatus.ExcludedFromTotals));
        var validation = ValidateExpectedTotal(input.ExpectedTotal, processedTotal, allTransactions.Any(transaction => transaction.IncludedInProcessedTotal));
        var reviewItems = allTransactions
            .Where(transaction => transaction.Status != RowStatus.Invalid && transaction.RequiresReview)
            .ToArray();
        var excludedRows = allTransactions
            .Where(transaction => transaction.Status == RowStatus.ExcludedFromTotals)
            .ToArray();

        var counts = new ProcessingCounts(
            SourceRows: input.SourceRowCount,
            ValidRows: input.TransactionOutcomes.Count,
            CategorizedRows: allTransactions.Count(transaction => transaction.Status == RowStatus.Categorized),
            ReviewRequiredRows: reviewItems.Length,
            InvalidRows: invalidTransactions.Length,
            ExcludedFromTotalsRows: excludedRows.Length,
            PotentialDuplicateRows: allTransactions.Count(transaction => transaction.IsPotentialDuplicate));

        var completenessStatus = AccountsForEverySourceRow(input.SourceRowCount, allTransactions)
            ? ValidationStatus.Valid
            : ValidationStatus.Invalid;
        var auditEntries = BuildAuditEntries(allTransactions);
        var auditSummary = new ProcessingAuditSummary(
            counts,
            completenessStatus,
            auditEntries,
            BuildAuditMessages(completenessStatus, validation, processedTotal, categoryTotal),
            appliedDeterministicRuleCount: allTransactions.Sum(transaction => transaction.RuleMatches.Count),
            expectedTotalValidationStatus: validation.Status,
            aiUsed: false);

        return new ExpenseReport(
            new ReportMetadata(input.SourceName, input.GeneratedAtUtc ?? DateTimeOffset.UtcNow),
            new ExpenseReportTotals(input.ExpectedTotal, processedTotal, categoryTotal, excludedTotal),
            validation,
            categoryTotals,
            allTransactions,
            reviewItems,
            invalidTransactions,
            excludedRows,
            auditSummary,
            auditEntries);
    }

    private static ExpenseTransaction CreateInvalidTransaction(InvalidTransactionRow invalidRow)
    {
        var reviewReason = invalidRow.ValidationIssues.FirstOrDefault()?.Reason ?? ReviewReason.NoMatchingRule;

        return new ExpenseTransaction(
            invalidRow.SourceRow,
            date: null,
            description: null,
            amount: null,
            RowStatus.Invalid,
            category: null,
            reviewReason,
            includedInProcessedTotal: false,
            includedInCategoryTotals: false,
            isPotentialDuplicate: false,
            invalidRow.SourceRow.RawInstallment,
            invalidRow.ValidationIssues);
    }

    private static IReadOnlyCollection<CategoryTotal> BuildCategoryTotals(IReadOnlyCollection<ExpenseTransaction> transactions) =>
        transactions
            .Where(transaction => transaction.IncludedInCategoryTotals && transaction.Category is not null)
            .GroupBy(transaction => transaction.Category!.Value)
            .OrderBy(group => group.Key)
            .Select(group => new CategoryTotal(
                group.Key,
                group.Count(),
                SumAmounts(group)))
            .ToArray();

    private static ExpectedTotalValidationResult ValidateExpectedTotal(
        decimal? expectedTotal,
        decimal processedTotal,
        bool hasProcessableRows)
    {
        if (expectedTotal is null)
        {
            return new ExpectedTotalValidationResult(
                ExpectedTotalValidationStatus.NotProvided,
                expectedTotal,
                processedTotal,
                Difference: null);
        }

        if (!hasProcessableRows)
        {
            return new ExpectedTotalValidationResult(
                ExpectedTotalValidationStatus.NotApplicable,
                expectedTotal,
                processedTotal,
                Difference: null);
        }

        if (expectedTotal.Value == processedTotal)
        {
            return new ExpectedTotalValidationResult(
                ExpectedTotalValidationStatus.Match,
                expectedTotal,
                processedTotal,
                Difference: 0m);
        }

        return new ExpectedTotalValidationResult(
            ExpectedTotalValidationStatus.Mismatch,
            expectedTotal,
            processedTotal,
            Math.Abs(expectedTotal.Value - processedTotal));
    }

    private static IReadOnlyCollection<AuditEntry> BuildAuditEntries(IReadOnlyCollection<ExpenseTransaction> transactions)
    {
        var entries = new List<AuditEntry>();

        foreach (var transaction in transactions.OrderBy(transaction => transaction.SourceRow.RowNumber))
        {
            entries.AddRange(transaction.RuleMatches.Select(match => new AuditEntry(
                transaction.SourceRow.RowNumber,
                AuditEventType.RuleApplied,
                $"Applied deterministic rule {match.RuleId}.",
                match.RuleId)));

            if (transaction.Status == RowStatus.Invalid)
            {
                entries.AddRange(transaction.ValidationIssues.Select(issue => new AuditEntry(
                    transaction.SourceRow.RowNumber,
                    AuditEventType.RowInvalid,
                    issue.Message)));
            }

            if (transaction.Status != RowStatus.Invalid && transaction.RequiresReview && transaction.ReviewReason is not null)
            {
                entries.Add(new AuditEntry(
                    transaction.SourceRow.RowNumber,
                    AuditEventType.ReviewRequired,
                    $"Review required: {transaction.ReviewReason}."));
            }

            if (transaction.Status == RowStatus.ExcludedFromTotals)
            {
                entries.Add(new AuditEntry(
                    transaction.SourceRow.RowNumber,
                    AuditEventType.RowExcludedFromTotals,
                    "Row is visible but excluded from totals."));
            }

            if (transaction.IsPotentialDuplicate)
            {
                entries.Add(new AuditEntry(
                    transaction.SourceRow.RowNumber,
                    AuditEventType.PotentialDuplicate,
                    "Row is flagged as a potential duplicate and was not removed."));
            }
        }

        return entries;
    }

    private static IReadOnlyCollection<string> BuildAuditMessages(
        ValidationStatus completenessStatus,
        ExpectedTotalValidationResult validation,
        decimal processedTotal,
        decimal categoryTotal) =>
        [
            completenessStatus == ValidationStatus.Valid
                ? "All source rows were accounted for."
                : "Source row accounting did not match the parsed source row count.",
            $"Processed total: {processedTotal:0.00}.",
            $"Trusted category total: {categoryTotal:0.00}.",
            $"Expected total validation status: {validation.Status}.",
            "No AI was used."
        ];

    private static decimal SumAmounts(IEnumerable<ExpenseTransaction> transactions) =>
        transactions.Sum(transaction => transaction.Amount ?? 0m);

    private static bool AccountsForEverySourceRow(int sourceRowCount, IReadOnlyCollection<ExpenseTransaction> transactions)
    {
        var expectedRows = Enumerable.Range(1, sourceRowCount);
        var visibleRows = transactions.Select(transaction => transaction.SourceRow.RowNumber).Distinct().Order();

        return expectedRows.SequenceEqual(visibleRows);
    }
}
