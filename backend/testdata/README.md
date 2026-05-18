# Backend Test Data

This folder contains public synthetic CSV fixtures for the deterministic ExpenseFlow MVP workflow.

All fixture data is synthetic. The files must not contain real personal financial data, card numbers, bank account numbers, tax IDs, addresses, secrets, credentials, or copied statement rows.

Expected totals are provided separately by tests or callers. They are not part of the CSV contract.

## Fixtures

| File | Purpose | Expected total to provide separately | Acceptance tests supported |
| --- | --- | ---: | --- |
| `demo-main.csv` | Main 22-row mixed-behavior dataset covering categorized rows, review rows, excluded rows, invalid rows, installments, duplicate-looking rows, and total validation. | `258248.00` | AT-001, AT-004, AT-007 through AT-018, AT-020 through AT-026 |
| `demo-happy-path.csv` | Simplest successful dataset: rows 1 through 10 and 16 through 18 from the dataset design. | `179349.00` | AT-005, AT-006, AT-014, AT-026 |
| `demo-invalid-rows.csv` | Invalid-row visibility dataset: row 1 plus rows 20 through 22 from the dataset design. | `34500.00` | AT-009, AT-010, AT-011, AT-026, AT-028 |
| `demo-total-mismatch.csv` | Same CSV rows as `demo-main.csv`, intended to be processed with mismatching expected total `260000.00`. | `260000.00` | AT-019, AT-021, AT-023, AT-026 |

## Later Test Usage

When processing logic exists, automated tests should load these files as fixture input and pass the expected total separately. Tests should verify row counts, processed totals, category totals, invalid-row visibility, review visibility, no silent row dropping, and synthetic data safety according to `docs/demo-dataset-design.md` and `docs/acceptance-tests.md`.
