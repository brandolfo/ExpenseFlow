using ExpenseFlow.Application.Abstractions;
using ExpenseFlow.Application.ExpenseReports.Parsing;
using ExpenseFlow.Domain.Categorization;
using ExpenseFlow.Domain.Transactions;
using CategorizationMatchType = ExpenseFlow.Domain.Categorization.MatchType;

namespace ExpenseFlow.Application.ExpenseReports.Categorization;

public sealed class SeedDeterministicCategorizationRuleEngine : ICategorizationRuleEngine
{
    private static readonly IReadOnlyCollection<SeedCategoryRule> CategoryRules =
    [
        new("R001", CategorizationMatchType.KnownMerchant, ["FRESHVALE MARKET"], ExpenseCategory.Groceries, 100),
        new("R002", CategorizationMatchType.KnownMerchant, ["LOOPBEAN CAFE"], ExpenseCategory.RestaurantsAndCafes, 100),
        new("R003", CategorizationMatchType.KnownMerchant, ["CITYLINE TRANSIT"], ExpenseCategory.Transport, 100),
        new("R004", CategorizationMatchType.DescriptionKeyword, ["HOMEGRID", "UTILITY"], ExpenseCategory.HousingAndUtilities, 90),
        new("R005", CategorizationMatchType.DescriptionKeyword, ["CLOUDNEST", "TOOLS", "SUBSCRIPTION"], ExpenseCategory.SubscriptionsAndSoftware, 90),
        new("R006", CategorizationMatchType.DescriptionKeyword, ["PHARMACY"], ExpenseCategory.HealthAndPharmacy, 80),
        new("R007", CategorizationMatchType.KnownMerchant, ["BOOKHARBOR ACADEMY"], ExpenseCategory.Education, 100),
        new("R008", CategorizationMatchType.KnownMerchant, ["STARLIGHT CINEMA"], ExpenseCategory.Entertainment, 100),
        new("R009", CategorizationMatchType.DescriptionKeyword, ["TRAVELNOVA", "HOTEL"], ExpenseCategory.Travel, 90),
        new("R010", CategorizationMatchType.SourceTypeOrKeyword, ["fee", "CIVICFEE"], ExpenseCategory.FeesAndTaxes, 100),
        new("R012", CategorizationMatchType.DescriptionKeyword, ["CAFE"], ExpenseCategory.RestaurantsAndCafes, 80),
        new("R015", CategorizationMatchType.KnownMerchant, ["PIXELGROVE ELECTRONICS"], ExpenseCategory.Shopping, 100),
        new("R016", CategorizationMatchType.KnownMerchant, ["PANTRYVALE EXPRESS"], ExpenseCategory.Groceries, 100)
    ];

    public IReadOnlyCollection<ExpenseTransaction> Categorize(
        IReadOnlyCollection<ParsedTransactionCandidate> candidates)
    {
        ArgumentNullException.ThrowIfNull(candidates);

        var duplicateKeys = candidates
            .GroupBy(candidate => DuplicateKey.Create(candidate))
            .ToDictionary(group => group.Key, group => group.Select(candidate => candidate.SourceRow.RowNumber).Order().ToArray());

        return candidates
            .OrderBy(candidate => candidate.SourceRow.RowNumber)
            .Select(candidate => Categorize(candidate, duplicateKeys))
            .ToArray();
    }

    private static ExpenseTransaction Categorize(
        ParsedTransactionCandidate candidate,
        IReadOnlyDictionary<DuplicateKey, int[]> duplicateKeys)
    {
        if (IsRefundLike(candidate))
        {
            return CreateExcludedTransaction(
                candidate,
                ReviewReason.RefundLikeNegativeAmount,
                new CategorizationRuleMatch(
                    "R013",
                    CategorizationMatchType.RefundDetector,
                    resultingCategory: null,
                    priority: 120,
                    pattern: "negative amount with refund signal"));
        }

        if (IsTransferOrPaymentLike(candidate))
        {
            return CreateExcludedTransaction(
                candidate,
                ReviewReason.TransferOrPayment,
                new CategorizationRuleMatch(
                    "R014",
                    CategorizationMatchType.TransferDetector,
                    resultingCategory: null,
                    priority: 120,
                    pattern: "transfer/payment/wallet signal"));
        }

        var categoryMatches = CategoryRules
            .Where(rule => rule.IsMatch(candidate))
            .Select(rule => rule.ToMatch())
            .ToArray();

        var duplicateMatch = TryCreateDuplicateMatch(candidate, duplicateKeys);
        var allMatches = duplicateMatch is null ? categoryMatches : [.. categoryMatches, duplicateMatch];
        var distinctCategories = categoryMatches
            .Select(match => match.ResultingCategory)
            .OfType<ExpenseCategory>()
            .Distinct()
            .ToArray();

        if (distinctCategories.Length > 1)
        {
            return CreateTransaction(
                candidate,
                RowStatus.ReviewRequired,
                category: null,
                ReviewReason.CategoryConflict,
                includedInProcessedTotal: true,
                includedInCategoryTotals: false,
                isPotentialDuplicate: duplicateMatch is not null,
                ruleMatches: allMatches,
                conflictingRuleMatches: categoryMatches);
        }

        if (distinctCategories.Length == 1)
        {
            return CreateTransaction(
                candidate,
                RowStatus.Categorized,
                distinctCategories[0],
                duplicateMatch is null ? null : ReviewReason.PotentialDuplicate,
                includedInProcessedTotal: true,
                includedInCategoryTotals: true,
                isPotentialDuplicate: duplicateMatch is not null,
                ruleMatches: allMatches);
        }

        return CreateTransaction(
            candidate,
            RowStatus.ReviewRequired,
            category: null,
            GetFallbackReviewReason(candidate),
            includedInProcessedTotal: true,
            includedInCategoryTotals: false,
            isPotentialDuplicate: duplicateMatch is not null,
            ruleMatches:
            [
                new CategorizationRuleMatch(
                    "R011",
                    CategorizationMatchType.ReviewFallback,
                    resultingCategory: null,
                    priority: 10,
                    pattern: "no safe deterministic category match"),
                .. duplicateMatch is null ? [] : new[] { duplicateMatch }
            ]);
    }

    private static ExpenseTransaction CreateExcludedTransaction(
        ParsedTransactionCandidate candidate,
        ReviewReason reviewReason,
        CategorizationRuleMatch match) =>
        CreateTransaction(
            candidate,
            RowStatus.ExcludedFromTotals,
            category: null,
            reviewReason,
            includedInProcessedTotal: false,
            includedInCategoryTotals: false,
            isPotentialDuplicate: false,
            ruleMatches: [match]);

    private static ExpenseTransaction CreateTransaction(
        ParsedTransactionCandidate candidate,
        RowStatus status,
        ExpenseCategory? category,
        ReviewReason? reviewReason,
        bool includedInProcessedTotal,
        bool includedInCategoryTotals,
        bool isPotentialDuplicate,
        IReadOnlyCollection<CategorizationRuleMatch> ruleMatches,
        IReadOnlyCollection<CategorizationRuleMatch>? conflictingRuleMatches = null) =>
        new(
            candidate.SourceRow,
            candidate.Date,
            candidate.Description,
            candidate.Amount,
            status,
            category,
            reviewReason,
            includedInProcessedTotal,
            includedInCategoryTotals,
            isPotentialDuplicate,
            candidate.SourceRow.RawInstallment,
            validationIssues: null,
            ruleMatches,
            conflictingRuleMatches);

    private static bool IsRefundLike(ParsedTransactionCandidate candidate) =>
        candidate.Amount < 0 &&
        (Contains(candidate.SourceRow.RawSourceType, "refund") ||
            Contains(candidate.Description, "REFUND") ||
            Contains(candidate.Description, "REVERSAL") ||
            Contains(candidate.Description, "CREDIT") ||
            Contains(candidate.Description, "ADJUSTMENT"));

    private static bool IsTransferOrPaymentLike(ParsedTransactionCandidate candidate) =>
        Contains(candidate.SourceRow.RawSourceType, "transfer") ||
        Contains(candidate.SourceRow.RawSourceType, "payment") ||
        Contains(candidate.Description, "TRANSFER") ||
        Contains(candidate.Description, "WALLET");

    private static CategorizationRuleMatch? TryCreateDuplicateMatch(
        ParsedTransactionCandidate candidate,
        IReadOnlyDictionary<DuplicateKey, int[]> duplicateKeys)
    {
        var duplicateRows = duplicateKeys[DuplicateKey.Create(candidate)];

        if (duplicateRows.Length <= 1 || candidate.SourceRow.RowNumber != duplicateRows.Skip(1).First())
        {
            return null;
        }

        return new CategorizationRuleMatch(
            "R017",
            CategorizationMatchType.DuplicateDetector,
            resultingCategory: null,
            priority: 110,
            pattern: "same date, amount, and normalized description");
    }

    private static ReviewReason GetFallbackReviewReason(ParsedTransactionCandidate candidate)
    {
        if (Contains(candidate.Description, "MARKETBOX") || Contains(candidate.Description, "MARKETPLACE"))
        {
            return ReviewReason.UnknownMarketplace;
        }

        if (Contains(candidate.Description, "PAYBRIDGE") || Contains(candidate.Description, "PAYMENT SERVICE"))
        {
            return ReviewReason.AmbiguousPaymentService;
        }

        return ReviewReason.NoMatchingRule;
    }

    private static string NormalizeDescription(string description) =>
        string.Join(
            ' ',
            description
                .Trim()
                .ToUpperInvariant()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries));

    private static bool Contains(string? text, string pattern) =>
        text?.Contains(pattern, StringComparison.OrdinalIgnoreCase) == true;

    private sealed record SeedCategoryRule(
        string RuleId,
        CategorizationMatchType MatchType,
        IReadOnlyCollection<string> Patterns,
        ExpenseCategory Category,
        int Priority)
    {
        public bool IsMatch(ParsedTransactionCandidate candidate) =>
            RuleId == "R010"
                ? Patterns.Any(pattern => string.Equals(candidate.SourceRow.RawSourceType, pattern, StringComparison.OrdinalIgnoreCase)) ||
                    Patterns.Any(pattern => Contains(candidate.Description, pattern))
                : Patterns.Any(pattern => Contains(candidate.Description, pattern));

        public CategorizationRuleMatch ToMatch() =>
            new(RuleId, MatchType, Category, Priority, string.Join(" or ", Patterns));
    }

    private sealed record DuplicateKey(DateOnly Date, decimal Amount, string NormalizedDescription)
    {
        public static DuplicateKey Create(ParsedTransactionCandidate candidate) =>
            new(candidate.Date, candidate.Amount, NormalizeDescription(candidate.Description));
    }
}
