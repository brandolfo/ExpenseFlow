using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

QuestPDF.Settings.License = LicenseType.Community;

var backendRoot = FindBackendRoot();
var pdfFixtureDirectory = Path.Combine(backendRoot, "testdata", "pdf");

GenerateVisaLikeFixture(pdfFixtureDirectory);
GenerateMastercardLikeFixture(pdfFixtureDirectory);

Console.WriteLine("Generated synthetic PDF fixtures:");
Console.WriteLine(Path.Combine(pdfFixtureDirectory, "icbc-visa-like-v1.pdf"));
Console.WriteLine(Path.Combine(pdfFixtureDirectory, "icbc-mastercard-like-v1.pdf"));

static void GenerateVisaLikeFixture(string pdfFixtureDirectory)
{
    var rows = FixtureRow.Load(Path.Combine(pdfFixtureDirectory, "icbc-visa-like-v1.expected-normalized-rows.csv"));
    var outputPath = Path.Combine(pdfFixtureDirectory, "icbc-visa-like-v1.pdf");

    Document.Create(container =>
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(28);
            page.DefaultTextStyle(text => text.FontSize(9).FontFamily("Arial"));

            page.Header().Element(header => ComposeStatementHeader(
                header,
                "Synthetic ICBC-Like Visa Statement",
                "icbc-visa-like-v1",
                "Persona Demo Visa",
                "VISA DEMO 0000",
                "SYNTHETIC ACCOUNT 0000",
                "SYN-VISA-2035-01",
                "2035-01-01 to 2035-01-31",
                "31.01.35"));

            page.Content().PaddingTop(12).Column(column =>
            {
                column.Spacing(8);
                column.Item().Text("CONSUMOS DEL PERIODO").SemiBold().FontSize(11);
                column.Item().Element(content => ComposeVisaTable(content, rows));
                column.Item().PaddingTop(8).Column(summary =>
                {
                    summary.Spacing(3);
                    summary.Item().Text("Total Consumos SINTETICO").SemiBold();
                    summary.Item().Text("Impuesto de Sellos SINTETICO").SemiBold();
                    summary.Item().Text("Saldo Actual SINTETICO").SemiBold();
                    summary.Item().Text("Pago Minimo SINTETICO").SemiBold();
                    summary.Item().Text("Texto informativo sintetico posterior al resumen. No es una transaccion.");
                });
            });

            page.Footer().AlignRight().Text("Synthetic fixture page 1 of 1");
        });
    }).GeneratePdf(outputPath);
}

static void GenerateMastercardLikeFixture(string pdfFixtureDirectory)
{
    var rows = FixtureRow.Load(Path.Combine(pdfFixtureDirectory, "icbc-mastercard-like-v1.expected-normalized-rows.csv"));
    var outputPath = Path.Combine(pdfFixtureDirectory, "icbc-mastercard-like-v1.pdf");

    Document.Create(container =>
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(28);
            page.DefaultTextStyle(text => text.FontSize(9).FontFamily("Arial"));

            page.Header().Element(header => ComposeStatementHeader(
                header,
                "Synthetic ICBC-Like Mastercard Statement",
                "icbc-mastercard-like-v1",
                "Persona Demo Mastercard",
                "MASTERCARD DEMO 1111",
                "SYNTHETIC ACCOUNT 1111",
                "SYN-MC-2035-02",
                "2035-02-01 to 2035-02-28",
                "28-FEB-35"));

            page.Content().PaddingTop(12).Column(column =>
            {
                column.Spacing(8);
                column.Item().Text("RESUMEN CONSOLIDADO").SemiBold().FontSize(11);
                column.Item().Text("Linea de resumen sintetico previa al detalle. No debe extraerse como transaccion.");
                column.Item().Text("Importe consolidado sintetico: 99999.99 ARS");
                column.Item().PaddingTop(8).Text("DETALLE DEL MES").SemiBold().FontSize(11);
                column.Item().Text("Compras del Mes").SemiBold();
                column.Item().Element(content => ComposeMastercardTable(content, rows.Where(row => row.SourcePage == 1 && row.SourceType == "purchase")));
                column.Item().Text("Debitos Automaticos").SemiBold();
                column.Item().Element(content => ComposeMastercardTable(content, rows.Where(row => row.SourcePage == 1 && row.SourceType == "automatic_debit")));
            });

            page.Footer().AlignRight().Text("Synthetic fixture page 1 of 2");
        });

        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(28);
            page.DefaultTextStyle(text => text.FontSize(9).FontFamily("Arial"));

            page.Header().Column(column =>
            {
                column.Item().Text("Synthetic ICBC-Like Mastercard Statement").Bold().FontSize(14);
                column.Item().Text("Variant id: icbc-mastercard-like-v1");
                column.Item().Text("DETALLE DEL MES - CONTINUACION");
            });

            page.Content().PaddingTop(12).Column(column =>
            {
                column.Spacing(8);
                column.Item().Text("Cuotas del Mes").SemiBold();
                column.Item().Element(content => ComposeMastercardTable(content, rows.Where(row => row.SourcePage == 2 && row.SourceType == "installment")));
                column.Item().Text("Compras del Mes").SemiBold();
                column.Item().Element(content => ComposeMastercardTable(content, rows.Where(row => row.SourcePage == 2 && row.SourceType != "installment")));
                column.Item().PaddingTop(8).Text("TOTAL TITULAR SINTETICO").SemiBold();
                column.Item().Text("Texto legal e informativo sintetico posterior al total. No es una transaccion.");
            });

            page.Footer().AlignRight().Text("Synthetic fixture page 2 of 2");
        });
    }).GeneratePdf(outputPath);
}

static void ComposeStatementHeader(
    IContainer container,
    string title,
    string variantId,
    string holder,
    string cardAlias,
    string accountLabel,
    string statementNumber,
    string period,
    string closingDate)
{
    container.Column(column =>
    {
        column.Spacing(3);
        column.Item().Text(title).Bold().FontSize(14);
        column.Item().Text($"Variant id: {variantId}");
        column.Item().Text($"Holder: {holder}");
        column.Item().Text($"Card alias: {cardAlias}");
        column.Item().Text($"Account: {accountLabel}");
        column.Item().Text($"Statement number: {statementNumber}");
        column.Item().Text($"Period: {period}");
        column.Item().Text($"Closing date: {closingDate}");
        column.Item().Text("Synthetic public fixture. Contains no real financial or personal data.").Italic();
    });
}

static void ComposeVisaTable(IContainer container, IReadOnlyCollection<FixtureRow> rows)
{
    container.Table(table =>
    {
        table.ColumnsDefinition(columns =>
        {
            columns.ConstantColumn(55);
            columns.ConstantColumn(78);
            columns.RelativeColumn();
            columns.ConstantColumn(70);
            columns.ConstantColumn(70);
        });

        AddHeader(table, ["FECHA", "COMPROBANTE", "DETALLE DE TRANSACCION", "PESOS", "DOLARES"]);

        foreach (var row in rows.OrderBy(row => row.ExtractionOrder))
        {
            var date = FormatVisaDate(row.Date);
            var pesos = row.Currency == "ARS" ? FormatVisaAmount(row.Amount) : string.Empty;
            var dolares = row.Currency == "USD" ? FormatPositiveAmount(row.Amount) : string.Empty;

            AddCell(table, date);
            AddCell(table, row.Code);
            AddCell(table, row.Description);
            AddCell(table, pesos);
            AddCell(table, dolares);
        }
    });
}

static void ComposeMastercardTable(IContainer container, IEnumerable<FixtureRow> sourceRows)
{
    var rows = sourceRows.OrderBy(row => row.ExtractionOrder).ToArray();

    container.Table(table =>
    {
        table.ColumnsDefinition(columns =>
        {
            columns.ConstantColumn(60);
            columns.RelativeColumn();
            columns.ConstantColumn(74);
            columns.ConstantColumn(70);
            columns.ConstantColumn(70);
        });

        AddHeader(table, ["FECHA", "DETALLE", "NRO CUPON", "PESOS", "DOLARES"]);

        foreach (var row in rows)
        {
            var date = FormatMastercardDate(row.Date);
            var pesos = row.Currency == "ARS" ? row.Amount : string.Empty;
            var dolares = row.Currency == "USD" ? FormatPositiveAmount(row.Amount) : string.Empty;

            AddCell(table, date);
            AddCell(table, row.Description);
            AddCell(table, row.Code);
            AddCell(table, pesos);
            AddCell(table, dolares);
        }
    });
}

static void AddHeader(TableDescriptor table, string[] headers)
{
    table.Header(header =>
    {
        foreach (var text in headers)
        {
            header.Cell()
                .Background(Colors.Grey.Lighten2)
                .Border(0.5f)
                .Padding(4)
                .Text(text)
                .SemiBold();
        }
    });
}

static void AddCell(TableDescriptor table, string text)
{
    table.Cell()
        .Border(0.25f)
        .Padding(4)
        .Text(text);
}

static string FormatVisaDate(string isoDate)
{
    if (string.IsNullOrWhiteSpace(isoDate))
    {
        return string.Empty;
    }

    return DateOnly.ParseExact(isoDate, "yyyy-MM-dd", CultureInfo.InvariantCulture)
        .ToString("dd.MM.yy", CultureInfo.InvariantCulture);
}

static string FormatMastercardDate(string isoDate)
{
    if (string.IsNullOrWhiteSpace(isoDate))
    {
        return string.Empty;
    }

    var date = DateOnly.ParseExact(isoDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
    var months = new[] { "ENE", "FEB", "MAR", "ABR", "MAY", "JUN", "JUL", "AGO", "SEP", "OCT", "NOV", "DIC" };

    return $"{date.Day:00}-{months[date.Month - 1]}-{date:yy}";
}

static string FormatVisaAmount(string amount)
{
    if (string.IsNullOrWhiteSpace(amount))
    {
        return string.Empty;
    }

    var value = decimal.Parse(amount, CultureInfo.InvariantCulture);

    return value < 0
        ? $"{Math.Abs(value).ToString("0.00", CultureInfo.InvariantCulture)}-"
        : value.ToString("0.00", CultureInfo.InvariantCulture);
}

static string FormatPositiveAmount(string amount)
{
    if (string.IsNullOrWhiteSpace(amount))
    {
        return string.Empty;
    }

    return decimal.Parse(amount, CultureInfo.InvariantCulture).ToString("0.00", CultureInfo.InvariantCulture);
}

static string FindBackendRoot()
{
    var directory = new DirectoryInfo(AppContext.BaseDirectory);

    while (directory is not null)
    {
        if (File.Exists(Path.Combine(directory.FullName, "ExpenseFlow.sln")) &&
            Directory.Exists(Path.Combine(directory.FullName, "testdata", "pdf")))
        {
            return directory.FullName;
        }

        directory = directory.Parent;
    }

    throw new DirectoryNotFoundException("Could not locate backend directory containing ExpenseFlow.sln and testdata/pdf.");
}

internal sealed record FixtureRow(
    int RowNumber,
    string SourceName,
    string StatementShape,
    int SourcePage,
    int ExtractionOrder,
    string Date,
    string Code,
    string Description,
    string Amount,
    string Currency,
    string Installment,
    string SourceType,
    string Notes,
    string ExpectedExtractionStatus,
    string ExpectedWarning)
{
    public static IReadOnlyList<FixtureRow> Load(string path)
    {
        var lines = File.ReadAllLines(path);
        var rows = new List<FixtureRow>();

        foreach (var line in lines.Skip(1).Where(line => !string.IsNullOrWhiteSpace(line)))
        {
            var values = ParseCsvLine(line);

            if (values.Count != 15)
            {
                throw new InvalidDataException($"Expected 15 columns in {path}, but found {values.Count}: {line}");
            }

            rows.Add(new FixtureRow(
                int.Parse(values[0], CultureInfo.InvariantCulture),
                values[1],
                values[2],
                int.Parse(values[3], CultureInfo.InvariantCulture),
                int.Parse(values[4], CultureInfo.InvariantCulture),
                values[5],
                values[6],
                values[7],
                values[8],
                values[9],
                values[10],
                values[11],
                values[12],
                values[13],
                values[14]));
        }

        return rows;
    }

    private static IReadOnlyList<string> ParseCsvLine(string line)
    {
        var values = new List<string>();
        var current = new List<char>();
        var inQuotes = false;

        for (var index = 0; index < line.Length; index++)
        {
            var character = line[index];

            if (character == '"')
            {
                if (inQuotes && index + 1 < line.Length && line[index + 1] == '"')
                {
                    current.Add('"');
                    index++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }

                continue;
            }

            if (character == ',' && !inQuotes)
            {
                values.Add(new string(current.ToArray()));
                current.Clear();
                continue;
            }

            current.Add(character);
        }

        values.Add(new string(current.ToArray()));

        return values;
    }
}
