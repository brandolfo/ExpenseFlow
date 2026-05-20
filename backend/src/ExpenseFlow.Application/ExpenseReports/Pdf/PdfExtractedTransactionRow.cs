using System.Globalization;
using ExpenseFlow.Application.ExpenseReports.Parsing;
using ExpenseFlow.Domain.Transactions;

namespace ExpenseFlow.Application.ExpenseReports.Pdf;

public sealed record PdfExtractedTransactionRow(
    int SourcePage,
    int ExtractionOrder,
    string? EvidenceSnippet,
    PdfExtractedFieldValues RawFields,
    DateOnly Date,
    string Description,
    decimal Amount,
    string? CurrencyCode = null,
    string? Code = null,
    string? Installment = null,
    string? SourceType = null,
    string? Notes = null,
    IReadOnlyCollection<PdfForeignCurrencyEvidence>? ForeignCurrencyEvidence = null)
{
    public IReadOnlyCollection<PdfForeignCurrencyEvidence> ForeignCurrencyEvidence { get; } =
        ForeignCurrencyEvidence ?? Array.Empty<PdfForeignCurrencyEvidence>();

    public ParsedTransactionCandidate ToParsedTransactionCandidate() =>
        new(
            new TransactionSourceRow(
                ExtractionOrder,
                RawFields.Date ?? Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                RawFields.Code ?? Code,
                RawFields.Description ?? Description,
                RawFields.Amount ?? Amount.ToString("0.00", CultureInfo.InvariantCulture),
                RawFields.Installment ?? Installment,
                RawFields.SourceType ?? SourceType,
                RawFields.Notes ?? Notes),
            Date,
            Description,
            Amount);
}
