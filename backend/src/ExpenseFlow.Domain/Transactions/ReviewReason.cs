namespace ExpenseFlow.Domain.Transactions;

public enum ReviewReason
{
    NoMatchingRule,
    UnknownMarketplace,
    AmbiguousPaymentService,
    CategoryConflict,
    RefundLikeNegativeAmount,
    TransferOrPayment,
    PotentialDuplicate,
    MissingDescription,
    InvalidDate,
    InvalidAmount
}
