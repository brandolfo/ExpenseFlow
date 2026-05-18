using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using ExpenseFlow.Application.Abstractions;
using ExpenseFlow.Application.ExpenseReports.Parsing;
using ExpenseFlow.Domain.Transactions;
using ExpenseFlow.Domain.Validation;

namespace ExpenseFlow.Infrastructure.Parsing;

public sealed class CsvTransactionFileParser : ITransactionFileParser
{
    private static readonly string[] RequiredColumns = ["date", "description", "amount"];

    public Task<TransactionFileParseResult> ParseAsync(
        string csvText,
        string sourceName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(csvText))
        {
            return Task.FromResult(TransactionFileParseResult.Failure(
                sourceName,
                [new FileValidationError(FileValidationErrorCode.EmptyInput, "CSV input is empty.")]));
        }

        if (HasUnclosedQuotedField(csvText))
        {
            return Task.FromResult(TransactionFileParseResult.Failure(
                sourceName,
                [new FileValidationError(FileValidationErrorCode.MalformedCsv, "CSV input could not be parsed safely.")]));
        }

        try
        {
            return Task.FromResult(Parse(csvText, sourceName, cancellationToken));
        }
        catch (Exception exception) when (exception is CsvHelperException or TypeConverterException or IOException)
        {
            return Task.FromResult(TransactionFileParseResult.Failure(
                sourceName,
                [new FileValidationError(FileValidationErrorCode.MalformedCsv, "CSV input could not be parsed safely.")]));
        }
    }

    private static TransactionFileParseResult Parse(
        string csvText,
        string sourceName,
        CancellationToken cancellationToken)
    {
        using var reader = new StringReader(csvText);
        using var csv = new CsvReader(reader, CreateConfiguration());

        if (!csv.Read())
        {
            return TransactionFileParseResult.Failure(
                sourceName,
                [new FileValidationError(FileValidationErrorCode.EmptyInput, "CSV input is empty.")]);
        }

        csv.ReadHeader();
        var headers = csv.HeaderRecord?.Select(NormalizeHeader).ToArray() ?? [];
        var missingColumns = RequiredColumns
            .Where(required => !headers.Contains(required, StringComparer.Ordinal))
            .ToArray();

        if (missingColumns.Length > 0)
        {
            return TransactionFileParseResult.Failure(
                sourceName,
                [
                    new FileValidationError(
                        FileValidationErrorCode.MissingRequiredColumns,
                        "CSV input is missing required columns.",
                        missingColumns)
                ]);
        }

        var validRows = new List<ParsedTransactionCandidate>();
        var invalidRows = new List<InvalidTransactionRow>();
        var sourceRowNumber = 0;

        while (csv.Read())
        {
            cancellationToken.ThrowIfCancellationRequested();
            sourceRowNumber++;

            var sourceRow = new TransactionSourceRow(
                sourceRowNumber,
                GetField(csv, "date"),
                GetField(csv, "code"),
                GetField(csv, "description"),
                GetField(csv, "amount"),
                GetField(csv, "installment"),
                GetField(csv, "source_type"),
                GetField(csv, "notes"));

            var issues = Validate(sourceRow);

            if (issues.Count > 0)
            {
                invalidRows.Add(new InvalidTransactionRow(sourceRow, issues));
                continue;
            }

            validRows.Add(new ParsedTransactionCandidate(
                sourceRow,
                DateOnly.ParseExact(sourceRow.RawDate!, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                sourceRow.RawDescription!,
                decimal.Parse(sourceRow.RawAmount!, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture)));
        }

        return TransactionFileParseResult.Success(sourceName, sourceRowNumber, validRows, invalidRows);
    }

    private static CsvConfiguration CreateConfiguration() =>
        new(CultureInfo.InvariantCulture)
        {
            BadDataFound = args => throw new BadDataException(args.Field, args.RawRecord, args.Context),
            DetectColumnCountChanges = false,
            HeaderValidated = null,
            LineBreakInQuotedFieldIsBadData = true,
            MissingFieldFound = null,
            TrimOptions = TrimOptions.None
        };

    private static string NormalizeHeader(string header) =>
        header.TrimStart('\uFEFF');

    private static bool HasUnclosedQuotedField(string csvText)
    {
        var inQuotedField = false;

        for (var index = 0; index < csvText.Length; index++)
        {
            if (csvText[index] != '"')
            {
                continue;
            }

            if (inQuotedField && index + 1 < csvText.Length && csvText[index + 1] == '"')
            {
                index++;
                continue;
            }

            inQuotedField = !inQuotedField;
        }

        return inQuotedField;
    }

    private static string? GetField(CsvReader csv, string columnName) =>
        csv.TryGetField(columnName, out string? value) ? value : null;

    private static IReadOnlyCollection<ValidationIssue> Validate(TransactionSourceRow sourceRow)
    {
        var issues = new List<ValidationIssue>();

        if (!DateOnly.TryParseExact(
            sourceRow.RawDate,
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out _))
        {
            issues.Add(new ValidationIssue(ReviewReason.InvalidDate, "date must use YYYY-MM-DD format and be a valid date"));
        }

        if (string.IsNullOrWhiteSpace(sourceRow.RawDescription))
        {
            issues.Add(new ValidationIssue(ReviewReason.MissingDescription, "description is required"));
        }

        if (!decimal.TryParse(
            sourceRow.RawAmount,
            NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture,
            out _))
        {
            issues.Add(new ValidationIssue(ReviewReason.InvalidAmount, "amount must be a signed decimal number using . as the decimal separator"));
        }

        return issues;
    }
}
