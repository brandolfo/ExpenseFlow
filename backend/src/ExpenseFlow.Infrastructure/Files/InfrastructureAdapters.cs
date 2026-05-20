using ExpenseFlow.Application.Abstractions;
using ExpenseFlow.Infrastructure.Parsing;
using ExpenseFlow.Infrastructure.Pdf;

namespace ExpenseFlow.Infrastructure.Files;

public static class InfrastructureAdapters
{
    public static ITransactionFileParser CreateTransactionFileParser(IServiceProvider _) =>
        new CsvTransactionFileParser();

    public static IPdfStatementExtractor CreatePdfStatementExtractor(IServiceProvider _) =>
        new PdfPigPdfStatementExtractor();
}
