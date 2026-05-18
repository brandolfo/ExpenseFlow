namespace ExpenseFlow.Application.ExpenseReports.Parsing;

public enum FileValidationErrorCode
{
    EmptyInput,
    MissingRequiredColumns,
    MalformedCsv
}
