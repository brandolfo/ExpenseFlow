namespace ExpenseFlow.Domain.Categorization;

public enum MatchType
{
    KnownMerchant,
    DescriptionKeyword,
    AmountSign,
    SourceMarker,
    SourceTypeOrKeyword,
    Conflict,
    ReviewFallback,
    RefundDetector,
    TransferDetector,
    DuplicateDetector,
    RequiredFieldValidation
}
