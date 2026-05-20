# Synthetic PDF Fixture Policy

This folder is reserved for public synthetic PDF fixture assets for the PDF statement ingestion phase.

## Current Status

PDF-2 uses fixture specifications and expected normalized-row CSV files only. Actual generated PDF binaries are deferred to a later fixture-generation milestone. QuestPDF is not added yet because PDF-2 can define the extraction target without introducing a generation dependency.

## Variant IDs

The first public synthetic fixture variants are:

- `icbc-visa-like-v1`
- `icbc-mastercard-like-v1`

## Files

- `icbc-visa-like-v1.fixture-spec.md`: describes the future single-page synthetic Visa-like PDF layout.
- `icbc-visa-like-v1.expected-normalized-rows.csv`: expected extraction and normalization rows for the Visa-like variant.
- `icbc-mastercard-like-v1.fixture-spec.md`: describes the future multi-page synthetic Mastercard-like PDF layout.
- `icbc-mastercard-like-v1.expected-normalized-rows.csv`: expected extraction and normalization rows for the Mastercard-like variant.

The CSV files describe extraction/normalization expectations only. They do not define final categories, processed totals, expected-total validation, or report output.

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

When actual PDF binaries are generated later, they should be small, text-selectable, fully synthetic, and aligned with these specs and expected rows. Generated PDFs remain allowed under this folder unless they match private filename patterns from `.gitignore`.

These fixture assets will support PDF-3/PDF-4 tests for statement-shape detection, active-section markers, row extraction order, page traceability, header/footer exclusion, sign handling, malformed candidate visibility, and no silent row loss.
