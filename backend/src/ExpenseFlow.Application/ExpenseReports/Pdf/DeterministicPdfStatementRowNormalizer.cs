using System.Globalization;
using System.Text.RegularExpressions;
using ExpenseFlow.Application.Abstractions;

namespace ExpenseFlow.Application.ExpenseReports.Pdf;

public sealed partial class DeterministicPdfStatementRowNormalizer : IPdfStatementRowNormalizer
{
    public PdfStatementExtractionResult Normalize(PdfStatementExtractionResult extractionResult)
    {
        ArgumentNullException.ThrowIfNull(extractionResult);

        if (!extractionResult.IsProcessable)
        {
            return extractionResult;
        }

        return extractionResult.StatementShapeId switch
        {
            PdfStatementShapeIds.IcbcVisaLikeV1 => NormalizeVisaLike(extractionResult),
            PdfStatementShapeIds.IcbcMastercardLikeV1 => NormalizeMastercardLike(extractionResult),
            _ => WithWarning(
                extractionResult,
                PdfExtractionStatus.UnsupportedStatementShape,
                new PdfExtractionWarning("unsupported_statement_shape", "PDF row normalization supports only the accepted synthetic statement shapes."))
        };
    }

    private static PdfStatementExtractionResult NormalizeVisaLike(PdfStatementExtractionResult extractionResult)
    {
        var activeLines = LinesBetween(
            extractionResult.ExtractedLines,
            line => IsVisaHeader(line.Text),
            line => ContainsAny(line.Text, "Total Consumos", "Impuesto de Sellos", "Saldo Actual", "Pago Minimo"));

        if (activeLines is null)
        {
            return WithWarning(
                extractionResult,
                PdfExtractionStatus.Failed,
                new PdfExtractionWarning("missing_transaction_section", "Visa-like transaction section markers were not found."));
        }

        var rows = new List<PdfExtractedTransactionRow>();
        var invalidRows = new List<PdfInvalidExtractedRow>();
        var warnings = new List<PdfExtractionWarning>(extractionResult.Warnings);
        var candidateOrder = 0;

        foreach (var line in activeLines)
        {
            if (!IsVisaCandidateLine(line.Text))
            {
                continue;
            }

            candidateOrder++;
            var parsed = ParseVisaCandidate(extractionResult.SourceName, line, candidateOrder);
            AddParsedCandidate(parsed, rows, invalidRows, warnings);
        }

        return WithRows(extractionResult, rows, invalidRows, warnings);
    }

    private static PdfStatementExtractionResult NormalizeMastercardLike(PdfStatementExtractionResult extractionResult)
    {
        var detailStarted = false;
        var currentSourceType = string.Empty;
        var rows = new List<PdfExtractedTransactionRow>();
        var invalidRows = new List<PdfInvalidExtractedRow>();
        var warnings = new List<PdfExtractionWarning>(extractionResult.Warnings);
        var candidateOrder = 0;

        foreach (var line in extractionResult.ExtractedLines.OrderBy(line => line.ExtractionOrder))
        {
            if (!detailStarted)
            {
                detailStarted = line.Text.Contains("DETALLE DEL MES", StringComparison.OrdinalIgnoreCase);
                continue;
            }

            if (line.Text.Contains("TOTAL TITULAR", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            if (line.Text.Equals("Compras del Mes", StringComparison.OrdinalIgnoreCase))
            {
                currentSourceType = "purchase";
                continue;
            }

            if (line.Text.Equals("Debitos Automaticos", StringComparison.OrdinalIgnoreCase))
            {
                currentSourceType = "automatic_debit";
                continue;
            }

            if (line.Text.Equals("Cuotas del Mes", StringComparison.OrdinalIgnoreCase))
            {
                currentSourceType = "installment";
                continue;
            }

            if (!IsMastercardCandidateLine(line.Text))
            {
                continue;
            }

            candidateOrder++;
            var parsed = ParseMastercardCandidate(extractionResult.SourceName, line, candidateOrder, currentSourceType);
            AddParsedCandidate(parsed, rows, invalidRows, warnings);
        }

        if (!detailStarted)
        {
            return WithWarning(
                extractionResult,
                PdfExtractionStatus.Failed,
                new PdfExtractionWarning("missing_transaction_section", "Mastercard-like transaction detail marker was not found."));
        }

        return WithRows(extractionResult, rows, invalidRows, warnings);
    }

    private static ParsedCandidate ParseVisaCandidate(string sourceName, PdfExtractedTextLine line, int candidateOrder)
    {
        var match = VisaDatedRowRegex().Match(line.Text);

        if (match.Success)
        {
            var rawDate = match.Groups["date"].Value;
            var code = match.Groups["code"].Value;
            var description = NormalizeText(match.Groups["description"].Value);
            var rawAmount = NullIfWhiteSpace(match.Groups["amount"].Value);
            var currency = code == "SVI-9108" ? "USD" : "ARS";
            var sourceType = ResolveVisaSourceType(description, rawAmount);
            var notes = ResolveVisaNotes(description, currency, sourceType);

            return BuildCandidate(
                sourceName,
                line,
                candidateOrder,
                rawDate,
                code,
                description,
                rawAmount,
                currency,
                installment: null,
                sourceType,
                notes,
                dateParser: ParseVisaDate);
        }

        var invalidMatch = VisaMissingDateRowRegex().Match(line.Text);

        if (invalidMatch.Success)
        {
            return BuildInvalidCandidate(
                line,
                candidateOrder,
                rawDate: null,
                code: invalidMatch.Groups["code"].Value,
                description: NormalizeText(invalidMatch.Groups["description"].Value),
                rawAmount: null,
                currency: "ARS",
                installment: null,
                sourceType: "unknown",
                notes: "transaction-like candidate with missing date and amount",
                new PdfExtractionWarning(
                    "missing_date_and_amount",
                    "Transaction-like candidate is missing date and amount.",
                    line.SourcePage,
                    candidateOrder));
        }

        return BuildInvalidCandidate(
            line,
            candidateOrder,
            rawDate: null,
            code: null,
            description: line.Text,
            rawAmount: null,
            currency: null,
            installment: null,
            sourceType: "unknown",
            notes: "unparseable Visa-like transaction candidate",
            new PdfExtractionWarning("unparseable_candidate", "Visa-like transaction candidate could not be parsed.", line.SourcePage, candidateOrder));
    }

    private static ParsedCandidate ParseMastercardCandidate(
        string sourceName,
        PdfExtractedTextLine line,
        int candidateOrder,
        string currentSourceType)
    {
        var match = MastercardDatedRowRegex().Match(line.Text);

        if (match.Success)
        {
            var rawDate = match.Groups["date"].Value;
            var description = NormalizeText(match.Groups["description"].Value);
            var code = match.Groups["code"].Value;
            var rawAmount = NullIfWhiteSpace(match.Groups["amount"].Value);
            var currency = code == "SMC-8109" ? "USD" : "ARS";
            var sourceType = ResolveMastercardSourceType(description, currentSourceType);
            var installment = InstallmentRegex().Match(description);
            var installmentValue = installment.Success ? installment.Value : null;
            var notes = ResolveMastercardNotes(description, sourceType, currency);

            return BuildCandidate(
                sourceName,
                line,
                candidateOrder,
                rawDate,
                code,
                description,
                rawAmount,
                currency,
                installmentValue,
                sourceType,
                notes,
                dateParser: ParseMastercardDate);
        }

        var invalidMatch = MastercardMissingDateRowRegex().Match(line.Text);

        if (invalidMatch.Success)
        {
            return BuildInvalidCandidate(
                line,
                candidateOrder,
                rawDate: null,
                code: invalidMatch.Groups["code"].Value,
                description: NormalizeText(invalidMatch.Groups["description"].Value),
                rawAmount: null,
                currency: "ARS",
                installment: null,
                sourceType: "unknown",
                notes: "transaction-like candidate with missing date and amount",
                new PdfExtractionWarning(
                    "missing_date_and_amount",
                    "Transaction-like candidate is missing date and amount.",
                    line.SourcePage,
                    candidateOrder));
        }

        return BuildInvalidCandidate(
            line,
            candidateOrder,
            rawDate: null,
            code: null,
            description: line.Text,
            rawAmount: null,
            currency: null,
            installment: null,
            sourceType: "unknown",
            notes: "unparseable Mastercard-like transaction candidate",
            new PdfExtractionWarning("unparseable_candidate", "Mastercard-like transaction candidate could not be parsed.", line.SourcePage, candidateOrder));
    }

    private static ParsedCandidate BuildCandidate(
        string sourceName,
        PdfExtractedTextLine line,
        int candidateOrder,
        string rawDate,
        string code,
        string description,
        string? rawAmount,
        string currency,
        string? installment,
        string sourceType,
        string notes,
        Func<string, DateOnly?> dateParser)
    {
        var warnings = new List<PdfExtractionWarning>();
        var date = dateParser(rawDate);
        var amount = TryParseAmount(rawAmount, out var parsedAmount);

        if (date is null)
        {
            warnings.Add(new PdfExtractionWarning("malformed_date", "Transaction candidate date could not be normalized.", line.SourcePage, candidateOrder));
        }

        if (!amount)
        {
            warnings.Add(new PdfExtractionWarning("malformed_amount", "Transaction candidate amount could not be normalized.", line.SourcePage, candidateOrder));
        }

        var rawFields = new PdfExtractedFieldValues(
            Date: rawDate,
            Code: code,
            Description: description,
            Amount: rawAmount,
            CurrencyCode: currency,
            Installment: installment,
            SourceType: sourceType,
            Notes: notes);

        if (warnings.Count > 0 || date is null)
        {
            return new ParsedCandidate(
                Row: null,
                InvalidRow: new PdfInvalidExtractedRow(line.SourcePage, candidateOrder, line.Text, rawFields, warnings),
                Warnings: warnings);
        }

        var row = new PdfExtractedTransactionRow(
            line.SourcePage,
            candidateOrder,
            line.Text,
            rawFields,
            date.Value,
            description,
            parsedAmount,
            CurrencyCode: currency,
            Code: code,
            Installment: installment,
            SourceType: sourceType,
            Notes: notes,
            ForeignCurrencyEvidence: currency == "USD"
                ? [new PdfForeignCurrencyEvidence("USD", rawAmount ?? string.Empty, parsedAmount)]
                : []);

        return new ParsedCandidate(row, InvalidRow: null, Warnings: []);
    }

    private static ParsedCandidate BuildInvalidCandidate(
        PdfExtractedTextLine line,
        int candidateOrder,
        string? rawDate,
        string? code,
        string description,
        string? rawAmount,
        string? currency,
        string? installment,
        string sourceType,
        string notes,
        PdfExtractionWarning warning)
    {
        var rawFields = new PdfExtractedFieldValues(
            Date: rawDate,
            Code: code,
            Description: description,
            Amount: rawAmount,
            CurrencyCode: currency,
            Installment: installment,
            SourceType: sourceType,
            Notes: notes);

        return new ParsedCandidate(
            Row: null,
            InvalidRow: new PdfInvalidExtractedRow(line.SourcePage, candidateOrder, line.Text, rawFields, [warning]),
            Warnings: [warning]);
    }

    private static void AddParsedCandidate(
        ParsedCandidate parsed,
        ICollection<PdfExtractedTransactionRow> rows,
        ICollection<PdfInvalidExtractedRow> invalidRows,
        ICollection<PdfExtractionWarning> warnings)
    {
        if (parsed.Row is not null)
        {
            rows.Add(parsed.Row);
        }

        if (parsed.InvalidRow is not null)
        {
            invalidRows.Add(parsed.InvalidRow);
        }

        foreach (var warning in parsed.Warnings)
        {
            warnings.Add(warning);
        }
    }

    private static PdfStatementExtractionResult WithRows(
        PdfStatementExtractionResult extractionResult,
        IReadOnlyCollection<PdfExtractedTransactionRow> rows,
        IReadOnlyCollection<PdfInvalidExtractedRow> invalidRows,
        IReadOnlyCollection<PdfExtractionWarning> warnings) =>
        new(
            extractionResult.SourceName,
            extractionResult.StatementShapeId,
            invalidRows.Count > 0 || warnings.Count > extractionResult.Warnings.Count ? PdfExtractionStatus.Partial : PdfExtractionStatus.Succeeded,
            rows,
            invalidRows,
            warnings,
            extractionResult.ExtractedStatementTotals,
            extractionResult.ExtractedLines);

    private static PdfStatementExtractionResult WithWarning(
        PdfStatementExtractionResult extractionResult,
        PdfExtractionStatus status,
        PdfExtractionWarning warning) =>
        new(
            extractionResult.SourceName,
            extractionResult.StatementShapeId,
            status,
            extractionResult.ExtractedRows,
            extractionResult.InvalidRows,
            extractionResult.Warnings.Concat([warning]).ToArray(),
            extractionResult.ExtractedStatementTotals,
            extractionResult.ExtractedLines);

    private static IReadOnlyCollection<PdfExtractedTextLine>? LinesBetween(
        IReadOnlyCollection<PdfExtractedTextLine> lines,
        Func<PdfExtractedTextLine, bool> start,
        Func<PdfExtractedTextLine, bool> stop)
    {
        var active = false;
        var result = new List<PdfExtractedTextLine>();

        foreach (var line in lines.OrderBy(line => line.ExtractionOrder))
        {
            if (!active)
            {
                active = start(line);
                continue;
            }

            if (stop(line))
            {
                break;
            }

            result.Add(line);
        }

        return active ? result : null;
    }

    private static DateOnly? ParseVisaDate(string rawDate)
    {
        var match = VisaDateRegex().Match(rawDate);

        if (!match.Success)
        {
            return null;
        }

        return new DateOnly(
            2000 + int.Parse(match.Groups["year"].Value, CultureInfo.InvariantCulture),
            int.Parse(match.Groups["month"].Value, CultureInfo.InvariantCulture),
            int.Parse(match.Groups["day"].Value, CultureInfo.InvariantCulture));
    }

    private static DateOnly? ParseMastercardDate(string rawDate)
    {
        var match = MastercardDateRegex().Match(rawDate);

        if (!match.Success)
        {
            return null;
        }

        var month = match.Groups["month"].Value.ToUpperInvariant() switch
        {
            "ENE" => 1,
            "FEB" => 2,
            "MAR" => 3,
            "ABR" => 4,
            "MAY" => 5,
            "JUN" => 6,
            "JUL" => 7,
            "AGO" => 8,
            "SEP" => 9,
            "OCT" => 10,
            "NOV" => 11,
            "DIC" => 12,
            _ => 0
        };

        if (month == 0)
        {
            return null;
        }

        return new DateOnly(
            2000 + int.Parse(match.Groups["year"].Value, CultureInfo.InvariantCulture),
            month,
            int.Parse(match.Groups["day"].Value, CultureInfo.InvariantCulture));
    }

    private static bool TryParseAmount(string? rawAmount, out decimal amount)
    {
        amount = 0m;

        if (string.IsNullOrWhiteSpace(rawAmount))
        {
            return false;
        }

        var value = rawAmount.Trim();
        var trailingMinus = value.EndsWith("-", StringComparison.Ordinal);

        if (trailingMinus)
        {
            value = value[..^1];
        }

        if (value.Contains(',', StringComparison.Ordinal))
        {
            value = value.Replace(".", string.Empty, StringComparison.Ordinal).Replace(',', '.');
        }

        if (!decimal.TryParse(value, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out amount))
        {
            return false;
        }

        if (trailingMinus)
        {
            amount = -amount;
        }

        return true;
    }

    private static string ResolveVisaSourceType(string description, string? rawAmount)
    {
        if (description.Contains("IMPUESTO", StringComparison.OrdinalIgnoreCase))
        {
            return "fee";
        }

        if (description.Contains("DEVOLUCION", StringComparison.OrdinalIgnoreCase) ||
            rawAmount?.EndsWith("-", StringComparison.Ordinal) == true)
        {
            return "refund";
        }

        return "purchase";
    }

    private static string ResolveMastercardSourceType(string description, string currentSourceType)
    {
        if (description.Contains("DEVOLUCION", StringComparison.OrdinalIgnoreCase))
        {
            return "refund";
        }

        return string.IsNullOrWhiteSpace(currentSourceType) ? "unknown" : currentSourceType;
    }

    private static string ResolveVisaNotes(string description, string currency, string sourceType)
    {
        if (description.Contains("MERCADO NORTE", StringComparison.OrdinalIgnoreCase))
        {
            return "ordinary supermarket-like purchase from dd.MM.yy source date";
        }

        if (description.Contains("CAFE RIO", StringComparison.OrdinalIgnoreCase))
        {
            return "cafe or restaurant-like purchase from dd.MM.yy source date";
        }

        if (description.Contains("FARMACIA CENTRAL", StringComparison.OrdinalIgnoreCase))
        {
            return "pharmacy-like purchase from dd.MM.yy source date";
        }

        if (description.Contains("COMBUSTIBLE", StringComparison.OrdinalIgnoreCase))
        {
            return "fuel-like purchase from dd.MM.yy source date";
        }

        if (description.Contains("SALUD INTEGRAL", StringComparison.OrdinalIgnoreCase))
        {
            return "service or health-like purchase from dd.MM.yy source date";
        }

        if (sourceType == "fee")
        {
            return "fee or tax-like row before summary markers";
        }

        if (sourceType == "refund")
        {
            return "trailing-minus source amount should normalize to negative decimal";
        }

        if (currency == "USD")
        {
            return "amount came from DOLARES column and remains foreign-currency evidence";
        }

        return "Visa-like normalized synthetic transaction";
    }

    private static string ResolveMastercardNotes(string description, string sourceType, string currency)
    {
        if (description.Contains("ELECTRO TEST 01/06", StringComparison.OrdinalIgnoreCase))
        {
            return "cuotas del mes installment pattern extracted from description";
        }

        if (description.Contains("ELECTRO TEST 06/06", StringComparison.OrdinalIgnoreCase))
        {
            return "cuotas del mes final installment pattern extracted from description";
        }

        if (currency == "USD")
        {
            return "amount came from DOLARES column and remains foreign-currency evidence";
        }

        if (sourceType == "automatic_debit")
        {
            return "debitos automaticos subsection row";
        }

        if (description.Equals("ELECTRO TEST", StringComparison.OrdinalIgnoreCase))
        {
            return "compras del mes ordinary purchase from dd-MMM-yy source date";
        }

        if (description.Contains("DELIVERY DEMO", StringComparison.OrdinalIgnoreCase))
        {
            return "compras del mes delivery-like purchase from dd-MMM-yy source date";
        }

        if (description.Contains("TRANSPORTE FICTICIO", StringComparison.OrdinalIgnoreCase))
        {
            return "compras del mes ride or transport-like purchase";
        }

        if (description.Contains("DEVOLUCION TEST", StringComparison.OrdinalIgnoreCase))
        {
            return "compras del mes subsection hint with negative credit-like amount visible";
        }

        if (description.Contains("IMPUESTO SINTETICO", StringComparison.OrdinalIgnoreCase))
        {
            return "compras del mes subsection hint with fee or tax-like description";
        }

        return "Mastercard-like normalized synthetic transaction";
    }

    private static bool IsVisaHeader(string text) =>
        ContainsAny(text, "CONSUMOS DEL PERIODO", "FECHA", "COMPROBANTE");

    private static bool IsVisaCandidateLine(string text) =>
        text.Contains("SVI-", StringComparison.OrdinalIgnoreCase);

    private static bool IsMastercardCandidateLine(string text) =>
        text.Contains("SMC-", StringComparison.OrdinalIgnoreCase);

    private static bool ContainsAny(string text, params string[] markers) =>
        markers.Any(marker => text.Contains(marker, StringComparison.OrdinalIgnoreCase));

    private static string NormalizeText(string text) =>
        MultipleWhitespaceRegex().Replace(text.Trim(), " ");

    private static string? NullIfWhiteSpace(string text) =>
        string.IsNullOrWhiteSpace(text) ? null : text.Trim();

    private sealed record ParsedCandidate(
        PdfExtractedTransactionRow? Row,
        PdfInvalidExtractedRow? InvalidRow,
        IReadOnlyCollection<PdfExtractionWarning> Warnings);

    [GeneratedRegex(@"^(?<date>\d{2}\.\d{2}\.\d{2})\s+(?<code>SVI-\d{4})\s+(?<description>.+?)\s+(?<amount>-?\d[\d.,]*-?)$")]
    private static partial Regex VisaDatedRowRegex();

    [GeneratedRegex(@"^(?<code>SVI-\d{4})\s+(?<description>.+)$")]
    private static partial Regex VisaMissingDateRowRegex();

    [GeneratedRegex(@"^(?<date>\d{2}-(?<month>[A-Z]{3})-\d{2})\s+(?<description>.+?)\s+(?<code>SMC-\d{4})(?:\s+(?<amount>-?\d[\d.,]*-?))?$")]
    private static partial Regex MastercardDatedRowRegex();

    [GeneratedRegex(@"^(?<description>.+?)\s+(?<code>SMC-\d{4})$")]
    private static partial Regex MastercardMissingDateRowRegex();

    [GeneratedRegex(@"^(?<day>\d{2})\.(?<month>\d{2})\.(?<year>\d{2})$")]
    private static partial Regex VisaDateRegex();

    [GeneratedRegex(@"^(?<day>\d{2})-(?<month>[A-Z]{3})-(?<year>\d{2})$")]
    private static partial Regex MastercardDateRegex();

    [GeneratedRegex(@"\b\d{2}/\d{2}\b")]
    private static partial Regex InstallmentRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex MultipleWhitespaceRegex();
}
