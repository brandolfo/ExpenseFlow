# Synthetic PDF Fixture Policy

This folder is reserved for public synthetic PDF fixture assets for the PDF statement ingestion phase.

## Current Status

PDF-2.2 adds generated, text-selectable synthetic PDF binaries for the two accepted statement variants. The PDFs are fixture assets only; no PDF endpoint, OCR, LLM integration, private-data workflow, or report pipeline integration is implemented here.

QuestPDF is used only by the dedicated generator tool under `backend/tools/ExpenseFlow.SyntheticPdfGenerator/`. It is not a production extraction dependency.

PDF-3 now uses these committed synthetic PDFs in deterministic raw text extraction tests. The tests verify that PdfPig can read the fixtures and preserve page/order/source traceability, but they do not define final PDF transaction normalization behavior.

PDF-4 now uses the expected normalized-row CSVs as fixture-driven assertions for deterministic normalization from raw extracted lines into transaction-like extraction rows.

PDF-5 now uses these synthetic fixtures in internal application service tests that feed normalized PDF rows into the existing deterministic categorization and report-generation pipeline. No public PDF endpoint exists yet.

## Variant IDs

The first public synthetic fixture variants are:

- `icbc-visa-like-v1`
- `icbc-mastercard-like-v1`

## Files

- `icbc-visa-like-v1.fixture-spec.md`: describes the future single-page synthetic Visa-like PDF layout.
- `icbc-visa-like-v1.expected-normalized-rows.csv`: expected extraction and normalization rows for the Visa-like variant.
- `icbc-visa-like-v1.pdf`: generated text-selectable synthetic Visa-like fixture.
- `icbc-mastercard-like-v1.fixture-spec.md`: describes the future multi-page synthetic Mastercard-like PDF layout.
- `icbc-mastercard-like-v1.expected-normalized-rows.csv`: expected extraction and normalization rows for the Mastercard-like variant.
- `icbc-mastercard-like-v1.pdf`: generated text-selectable synthetic Mastercard-like fixture.

The CSV files are the source of truth for expected extraction and normalization results. The generated PDFs visually contain those rows so future extraction tests can compare extracted output against the expected normalized-row CSVs.

The CSV files and PDFs describe extraction/normalization expectations only. They do not define final categories, processed totals, expected-total validation, or report output.

## Synthetic-Only Rule

Real PDFs must never be committed here. Anonymized real PDFs are risky because visible text, hidden text, metadata, layout clues, account details, merchants, dates, and amounts can still expose private financial information, so they must not be committed.

Public fixtures must be fully synthetic. They may imitate structural patterns from supported statement variants, but they must not use real data.

Fixtures must not contain:

- real names
- real account or card numbers
- real addresses
- real IDs or tax identifiers
- real emails or phone numbers
- real statement numbers
- real merchants
- real transaction dates
- real transaction amounts
- real transaction data
- hidden metadata copied from real statements

## Future PDF Generation

Regenerate the public synthetic PDFs from `backend/` with:

```powershell
dotnet run --project tools/ExpenseFlow.SyntheticPdfGenerator
```

The generator reads the expected normalized-row CSVs and writes the PDF fixture files listed above.

Generated PDFs are safe to commit only because they are fully synthetic. Real PDFs remain forbidden here, including anonymized real statements, screenshots, extracted text, and private statement metadata.

These fixture assets will support PDF-3/PDF-4 tests for statement-shape detection, active-section markers, row extraction order, page traceability, header/footer exclusion, sign handling, malformed candidate visibility, and no silent row loss.
