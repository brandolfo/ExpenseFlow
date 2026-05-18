# Demo Dataset Design

## Purpose
This document designs the synthetic dataset that will prove the ExpenseFlow MVP behavior.

Public demo data must be synthetic and realistic enough to test backend behavior without exposing real financial information. The dataset should resemble the messy patterns found in exported expense files, but it must not include real personal financial data, real full card numbers, addresses, tax IDs, account numbers, credentials, or personal identifiers.

This is a product dataset design document. It does not create CSV files, source code, backend architecture, database design, endpoint design, framework choices, or library choices.

## Dataset goals
The dataset must demonstrate:
- Valid categorized transactions
- Unknown merchants requiring review
- Invalid rows
- Negative amount/refund behavior
- Duplicate-looking rows
- Installment-like descriptions
- Transfer/payment-like rows
- Total validation success or mismatch
- Category summary
- Auditability

The dataset should also make it easy to verify that:
- No source row is silently ignored.
- Totals are calculated deterministically.
- Review items are visible.
- Invalid rows are visible and excluded from totals.
- Refund-like and transfer-like rows are visible and excluded from totals in the MVP.
- Duplicate-looking rows are surfaced for review and not removed automatically.

## Synthetic CSV shape
The synthetic CSV uses the MVP columns defined in `docs/input-output-contract.md`.

Required columns:
- `date`
- `description`
- `amount`

Optional columns included in the demo design:
- `code`
- `installment`
- `source_type`
- `notes`

The conceptual CSV column order for demo files should be:

```text
date,code,description,amount,installment,source_type,notes
```

Expected total is not part of the CSV. It is provided separately by the caller/user when processing starts.

## Demo transactions
All merchant names and codes below are synthetic.

| row_number | date | code | description | amount | installment | source_type | expected_status | expected_category | included_in_processed_total | review_reason | rule_expected_to_match |
| ---: | --- | --- | --- | ---: | --- | --- | --- | --- | --- | --- | --- |
| 1 | 2026-04-01 | DMO-0001 | FRESHVALE MARKET DEMO | 34500.00 |  | purchase | categorized | Groceries | true |  | R001 |
| 2 | 2026-04-02 | DMO-0002 | LOOPBEAN CAFE DEMO | 4200.00 |  | purchase | categorized | Restaurants and cafes | true |  | R002 |
| 3 | 2026-04-03 | DMO-0003 | CITYLINE TRANSIT DEMO | 850.00 |  | purchase | categorized | Transport | true |  | R003 |
| 4 | 2026-04-04 | DMO-0004 | HOMEGRID UTILITY DEMO | 18700.00 |  | purchase | categorized | Housing and utilities | true |  | R004 |
| 5 | 2026-04-05 | DMO-0005 | CLOUDNEST TOOLS SUBSCRIPTION DEMO | 12999.00 |  | purchase | categorized | Subscriptions and software | true |  | R005 |
| 6 | 2026-04-06 | DMO-0006 | WELLSPRING PHARMACY DEMO | 7600.00 |  | purchase | categorized | Health and pharmacy | true |  | R006 |
| 7 | 2026-04-07 | DMO-0007 | BOOKHARBOR ACADEMY DEMO | 9200.00 |  | purchase | categorized | Education | true |  | R007 |
| 8 | 2026-04-08 | DMO-0008 | STARLIGHT CINEMA DEMO | 6500.00 |  | purchase | categorized | Entertainment | true |  | R008 |
| 9 | 2026-04-09 | DMO-0009 | TRAVELNOVA HOTEL DEMO | 42500.00 |  | purchase | categorized | Travel | true |  | R009 |
| 10 | 2026-04-10 | DMO-0010 | CIVICFEE CARD SERVICE DEMO | 2500.00 |  | fee | categorized | Fees and taxes | true |  | R010 |
| 11 | 2026-04-11 | DMO-0011 | MARKETBOX DEMO ORDER 8842 | 42999.00 |  | purchase | review_required |  | true | unknown_marketplace | R011 |
| 12 | 2026-04-12 | DMO-0012 | PAYBRIDGE SERVICE DEMO 4821 | 18500.00 |  | purchase | review_required |  | true | ambiguous_payment_service | R011 |
| 13 | 2026-04-13 | DMO-0013 | WELLSPRING CAFE PHARMACY DEMO | 7600.00 |  | purchase | review_required |  | true | category_conflict | R006, R012 |
| 14 | 2026-04-14 | DMO-0014 | REFUND DEMO STORE | -12000.00 |  | refund | excluded_from_totals |  | false | refund_like_negative_amount | R013 |
| 15 | 2026-04-15 | DMO-0015 | TRANSFER DEMO TO WALLET | 30000.00 |  | transfer | excluded_from_totals |  | false | transfer_or_payment | R014 |
| 16 | 2026-04-16 | DMO-0016 | PIXELGROVE ELECTRONICS DEMO | 15000.00 | 03/06 | purchase | categorized | Shopping | true |  | R015 |
| 17 | 2026-04-17 | DMO-0017 | PIXELGROVE ELECTRONICS DEMO | 15000.00 | 04/06 | purchase | categorized | Shopping | true |  | R015 |
| 18 | 2026-04-18 | DMO-0018 | PANTRYVALE EXPRESS DEMO | 9800.00 |  | purchase | categorized | Groceries | true |  | R016 |
| 19 | 2026-04-18 | DMO-0019 | PANTRYVALE EXPRESS DEMO | 9800.00 |  | purchase | potential_duplicate | Groceries | true | potential_duplicate | R016, R017 |
| 20 | 2026-04-19 | DMO-0020 |  | 7000.00 |  | purchase | invalid |  | false | missing_description | R018 |
| 21 | 2026-99-01 | DMO-0021 | RIDEHILL TAXI DEMO | 2300.00 |  | purchase | invalid |  | false | invalid_date | R018 |
| 22 | 2026-04-21 | DMO-0022 | RIDEHILL TAXI DEMO | abc |  | purchase | invalid |  | false | invalid_amount | R018 |

Notes:
- Row 19 is intentionally duplicate-looking and must not be removed automatically.
- Rows 14 and 15 are visible review/exclusion examples and are not included in processed totals for the MVP.
- Rows 20, 21, and 22 prove invalid-row visibility.
- Rows 16 and 17 prove that installments are processed as individual rows.

## Deterministic rule seed set
Higher-priority rules should be evaluated before lower-priority rules when rules conflict. Validation and exclusion rules are intentionally high priority because they protect correctness and auditability.

| rule_id | match_type | pattern | category | priority | example matching rows |
| --- | --- | --- | --- | ---: | --- |
| R001 | known_merchant | `FRESHVALE MARKET` | Groceries | 100 | 1 |
| R002 | known_merchant | `LOOPBEAN CAFE` | Restaurants and cafes | 100 | 2 |
| R003 | known_merchant | `CITYLINE TRANSIT` | Transport | 100 | 3 |
| R004 | description_keyword | `HOMEGRID` or `UTILITY` | Housing and utilities | 90 | 4 |
| R005 | description_keyword | `CLOUDNEST`, `TOOLS`, or `SUBSCRIPTION` | Subscriptions and software | 90 | 5 |
| R006 | description_keyword | `PHARMACY` | Health and pharmacy | 80 | 6, 13 |
| R007 | known_merchant | `BOOKHARBOR ACADEMY` | Education | 100 | 7 |
| R008 | known_merchant | `STARLIGHT CINEMA` | Entertainment | 100 | 8 |
| R009 | description_keyword | `TRAVELNOVA` or `HOTEL` | Travel | 90 | 9 |
| R010 | source_type_or_keyword | `source_type = fee` or `CIVICFEE` | Fees and taxes | 100 | 10 |
| R011 | review_fallback | no safe deterministic category match |  | 10 | 11, 12 |
| R012 | description_keyword | `CAFE` | Restaurants and cafes | 80 | 13 |
| R013 | refund_detector | negative amount with `refund` source type or `REFUND` description |  | 120 | 14 |
| R014 | transfer_detector | `transfer`, `payment`, or wallet movement signal |  | 120 | 15 |
| R015 | known_merchant | `PIXELGROVE ELECTRONICS` | Shopping | 100 | 16, 17 |
| R016 | known_merchant | `PANTRYVALE EXPRESS` | Groceries | 100 | 18, 19 |
| R017 | duplicate_detector | same date, same amount, and same normalized description as another row |  | 110 | 19 |
| R018 | required_field_validation | missing or invalid `date`, `description`, or `amount` |  | 200 | 20, 21, 22 |

Rule expectations:
- R018 invalidates rows before they can be categorized or totaled.
- R013 and R014 surface rows for review and exclude them from totals in the MVP.
- R017 flags potential duplicates without removing them.
- R006 and R012 intentionally conflict on row 13, producing `review_required`.
- R011 exists to make unknown merchants visible rather than guessed.

## Expected summary

### Expected row counts
| Metric | Expected value |
| --- | ---: |
| Expected row count | 22 |
| Expected valid count | 19 |
| Expected review count | 6 |
| Expected invalid count | 3 |
| Expected potential duplicate count | 1 |
| Expected excluded from totals count | 2 |

Expected review rows:
- 11: unknown marketplace
- 12: ambiguous payment service
- 13: category conflict
- 14: refund-like negative amount excluded from totals
- 15: transfer/payment-like row excluded from totals
- 19: potential duplicate

### Expected processed total
Expected processed total: `258248.00`

Included rows:
- 1 through 13
- 16 through 19

Excluded from processed total:
- 14: refund-like negative amount
- 15: transfer/payment-like row
- 20, 21, 22: invalid rows

### Expected category totals
Only categorized rows are included in trusted category totals. Review-required rows without a trusted category do not inflate category totals.

| Category | Rows | Expected total |
| --- | --- | ---: |
| Groceries | 1, 18, 19 | 54100.00 |
| Restaurants and cafes | 2 | 4200.00 |
| Transport | 3 | 850.00 |
| Housing and utilities | 4 | 18700.00 |
| Subscriptions and software | 5 | 12999.00 |
| Health and pharmacy | 6 | 7600.00 |
| Education | 7 | 9200.00 |
| Entertainment | 8 | 6500.00 |
| Travel | 9 | 42500.00 |
| Fees and taxes | 10 | 2500.00 |
| Shopping | 16, 17 | 30000.00 |

Expected category total sum: `189149.00`

Review rows included in processed total but not trusted category totals:
- 11: `42999.00`
- 12: `18500.00`
- 13: `7600.00`

Review-included uncategorized total: `69099.00`

### Expected validation result
For the main mixed-behavior dataset:
- Expected total provided separately: `258248.00`
- Processed total: `258248.00`
- Expected validation result: `match`

## Dataset variants

### 1. Happy path dataset
Purpose: prove the simplest successful processing path.

Rows:
- 1 through 10
- 16 through 18

Expected behavior:
- Row count: 13
- Valid count: 13
- Categorized count: 13
- Review count: 0
- Invalid count: 0
- Potential duplicate count: 0
- Excluded from totals count: 0
- Expected processed total: `179349.00`
- Expected total provided separately: `179349.00`
- Expected validation result: `match`

Expected category totals:
- Groceries: `44300.00`
- Restaurants and cafes: `4200.00`
- Transport: `850.00`
- Housing and utilities: `18700.00`
- Subscriptions and software: `12999.00`
- Health and pharmacy: `7600.00`
- Education: `9200.00`
- Entertainment: `6500.00`
- Travel: `42500.00`
- Fees and taxes: `2500.00`
- Shopping: `30000.00`

### 2. Dataset with total mismatch
Purpose: prove deterministic expected-total validation and mismatch visibility.

Rows:
- Same rows as the main mixed-behavior dataset.

Expected behavior:
- Processed total: `258248.00`
- Expected total provided separately: `260000.00`
- Expected validation result: `mismatch`
- Difference to investigate: `1752.00`
- Report still includes processing counts, category summary, review items, invalid rows, and audit summary.

### 3. Dataset with invalid rows
Purpose: prove invalid-row visibility and non-silent failure behavior.

Rows:
- 1
- 20
- 21
- 22

Expected behavior:
- Row count: 4
- Valid count: 1
- Categorized count: 1
- Review count: 3
- Invalid count: 3
- Potential duplicate count: 0
- Excluded from totals count: 0
- Expected processed total: `34500.00`
- Expected total provided separately: `34500.00`
- Expected validation result: `match`
- Invalid rows are visible and excluded from processed and category totals.

## Security and privacy checks
Before this dataset becomes actual CSV files, verify that:
- All merchant names are synthetic.
- All transaction codes are synthetic.
- No real full card numbers are present.
- No real bank account numbers are present.
- No real addresses are present.
- No tax IDs are present.
- No personal identifiers are present.
- No real financial statements are copied into examples.

## Open questions
- Should the first actual CSV file include all 22 mixed-behavior rows, or should the happy path be the first public demo file?
- Should duplicate-looking rows be included in category totals in the first implementation, or should the report show category totals with and without potential duplicates?
- Should the main demo report headline emphasize total validation, review visibility, or auditability?
