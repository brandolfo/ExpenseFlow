# Synthetic PDF Fixture Policy

This folder is reserved for public synthetic PDF fixture assets for the PDF statement ingestion phase.

## Current Status

The PDF ingestion phase is implemented for the two accepted synthetic text-selectable statement variants. The PDFs are synthetic fixture assets only; no OCR, LLM integration, private-data workflow, persistence, arbitrary PDF support, trusted statement-total extraction, exchange-rate conversion, or production-hardening behavior is implemented here.

QuestPDF is used only by the dedicated generator tool under `backend/tools/ExpenseFlow.SyntheticPdfGenerator/`. It is not a production extraction dependency.

The fixtures support:

- deterministic PdfPig raw text extraction tests
- PDF row normalization tests against expected normalized rows
- internal PDF processing service tests
- API integration tests for `POST /api/expense-reports/process-pdf`
- no-silent-row-loss, non-ARS visibility, and dependency-boundary release-gate checks

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

The CSV files are the source of truth for expected extraction and normalization results. The generated PDFs visually contain those rows so extraction and normalization tests can compare extracted output against the expected normalized-row CSVs.

The CSV files and PDFs describe extraction/normalization expectations only. Final categories, processed totals, expected-total validation, and report output are produced by the existing deterministic processing pipeline and guarded by service/API tests.

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

These fixture assets support tests for statement-shape detection, active-section markers, row extraction order, page traceability, header/footer exclusion, sign handling, malformed candidate visibility, non-ARS visibility, API processing, and no silent row loss.
