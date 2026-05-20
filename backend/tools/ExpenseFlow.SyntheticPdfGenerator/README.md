# ExpenseFlow Synthetic PDF Generator

This tool generates public synthetic PDF fixtures for the ExpenseFlow PDF ingestion phase.

It must never use private statement data. The generated PDFs are committed only because every name, date, amount, code, account label, statement number, and merchant-like value is synthetic.

QuestPDF is used only here for fixture generation. Production PDF extraction remains separate and is expected to use a different library later, such as the PdfPig candidate documented in `docs/pdf-ingestion-plan.md`.

## Regenerate Fixtures

Run from `backend/`:

```powershell
dotnet run --project tools/ExpenseFlow.SyntheticPdfGenerator
```

The tool reads:

- `testdata/pdf/icbc-visa-like-v1.expected-normalized-rows.csv`
- `testdata/pdf/icbc-mastercard-like-v1.expected-normalized-rows.csv`

It writes:

- `testdata/pdf/icbc-visa-like-v1.pdf`
- `testdata/pdf/icbc-mastercard-like-v1.pdf`
