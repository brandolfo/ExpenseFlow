# Synthetic PDF Fixture Policy

This folder is reserved for future public synthetic PDF fixtures for the PDF statement ingestion phase. It may contain only this README until the PDF fixture milestone begins.

Real PDFs must never be committed here. Anonymized real PDFs are risky because visible text, hidden text, metadata, layout clues, account details, merchants, dates, and amounts can still expose private financial information, so they should not be committed.

Future public fixtures must be fully synthetic. They may imitate structural patterns from supported statement variants, but they must not use real data.

Planned public fixture variants:

- `icbc-visa-like-v1`
- `icbc-mastercard-like-v1`

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

Future fixtures should include the expected normalized rows and a clear test purpose so extraction tests can prove row accounting, traceability, header/footer handling, and no silent row loss.
