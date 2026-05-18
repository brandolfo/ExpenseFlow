# Domain Model

## Purpose
This document defines the business domain of ExpenseFlow for the MVP and near-future product direction. It is not an implementation model, database model, API design, or architecture document.

ExpenseFlow's domain is the transformation of messy transaction exports into structured, categorized, validated, and auditable expense reports.

## 1. Core domain concepts

### Transaction file
A user-provided export containing transaction rows. For public examples, the file must use synthetic data only. The MVP assumes a structured export, not a PDF.

### Source row
One row from the original transaction file. Every source row must be accounted for in the processing result. A row may become a valid transaction, an invalid row, or a review item, but it must not disappear silently.

### Transaction
A financial movement represented by a source row after it has enough valid information to be reasoned about. A transaction can represent a purchase, fee, refund, reversal, adjustment, payment, or other financial movement.

### Merchant
The business, platform, service, or counterparty inferred from the transaction description or source data. Merchant names may be noisy, abbreviated, inconsistent, or missing.

### Category
A user-facing grouping used for expense analysis. Categories should be broad enough to be useful and stable, not so detailed that early categorization becomes fragile.

### Categorization rule
A repeatable rule that assigns a category or review state based on known information such as merchant text, description pattern, amount sign, source label, or transaction marker.

### Categorization result
The outcome of trying to categorize a transaction. A result can be categorized, review required, invalid, or uncategorized depending on the available information and rule confidence.

### Review item
A transaction or source row that requires human inspection before it should be treated as final.

### Manual correction
A human change to a category, merchant interpretation, review status, or transaction treatment. Manual correction is part of the domain because it improves trust and can later inform new deterministic rules.

### Expense report
The structured output of a processing run. It summarizes totals, categories, validation results, transaction states, and review items.

### Audit trail
The explanation of what happened during processing: source row status, matched rules, categorization result, validation results, review reasons, and manual corrections.

## 2. Transaction lifecycle
The domain lifecycle for a source row is:

1. Received: The row is present in the transaction file.
2. Parsed conceptually: The row is interpreted as candidate transaction data.
3. Validated: Required fields and values are checked.
4. Classified by state:
   - Valid transaction
   - Invalid row
   - Review required
5. Categorized when safe:
   - Known deterministic rule match
   - Manual correction
   - Future AI suggestion accepted under review rules
6. Included in deterministic calculations when eligible.
7. Reported with audit details.

### Transaction states
- Received: The row exists and has not yet been evaluated.
- Invalid: The row is missing required data or contains values that cannot be safely used.
- Review required: The row is understandable enough to inspect but not safe to finalize automatically.
- Categorized: The transaction has a category assigned by deterministic rule or accepted correction.
- Excluded from totals: The row is visible in the report but not included in totals because it is invalid or outside the documented calculation rule.

No state may allow a transaction or source row to be silently ignored.

## 3. Expense report lifecycle
The domain lifecycle for an expense report is:

1. Requested: The user provides a transaction file and optionally an expected total.
2. Processing started: The system begins accounting for every source row.
3. Rows evaluated: Each row receives a processing status.
4. Categories assigned: Deterministic rules assign known categories where possible.
5. Review items identified: Ambiguous, conflicting, suspicious, or invalid rows are surfaced.
6. Totals calculated: Processed totals and category totals are calculated deterministically.
7. Validation completed: Processed totals are compared against expected totals when available.
8. Report produced: The user receives summary totals, category totals, processing counts, validation status, and review items.
9. Corrections applied later: In a future review flow, manual corrections update report interpretation and preserve correction history.

### Expense report statuses
- Complete with no review required: All valid rows were categorized and totals were validated.
- Complete with review required: The report is generated, but some rows need human inspection.
- Complete with validation warning: The report is generated, but expected and processed totals do not match.
- Incomplete due to invalid input: The file or rows contain problems that prevent a trustworthy final report, but the invalid items are still reported.

## 4. Initial category taxonomy
The MVP should start with a small taxonomy that is useful for personal expense analysis and easy to validate.

Initial categories:
- Groceries
- Restaurants and cafes
- Transport
- Housing and utilities
- Subscriptions and software
- Health and pharmacy
- Shopping
- Entertainment
- Education
- Travel
- Fees and taxes
- Income, refunds, and adjustments
- Transfers and payments
- Uncategorized review

### Taxonomy rules
- Categories should support expense understanding, not accounting precision.
- Categories may evolve after reviewing real user workflow needs.
- "Uncategorized review" is not a final category for trusted reporting; it is a review state used to prevent guessing.
- Refunds and adjustments may need special reporting because they can distort expense totals if treated like ordinary spending.

## 5. Categorization rule types

### Known merchant rule
Assigns a category when a normalized merchant or description clearly matches a known pattern.

Example: `Fresh Market Demo` maps to `Groceries`.

### Description keyword rule
Assigns a category based on stable words or phrases in the transaction description.

Example: `SUBSCRIPTION DEMO CLOUD TOOLS` maps to `Subscriptions and software`.

### Amount sign rule
Uses whether the amount is positive or negative to help identify spending, refunds, payments, or adjustments. Amount sign must not be the only rule when the category is ambiguous.

Example: a negative amount with `REFUND` in the description may map to `Income, refunds, and adjustments` or require review depending on the documented refund rule.

### Source marker rule
Uses a source-provided label when present, such as a transaction type or movement kind, without trusting it blindly.

Example: source type `FEE` may map to `Fees and taxes` if other fields do not conflict.

### Conflict rule
Marks a transaction for review when more than one deterministic rule matches different categories.

Example: a transaction description includes both `PHARMACY` and `CAFE`, causing a category conflict.

### Review fallback rule
Marks a transaction for manual review when no deterministic rule can safely categorize it.

Example: `PAYMENT SERVICE DEMO 4821` has no known merchant pattern and no stable category signal.

### Future AI suggestion rule
May propose a category for ambiguous transactions later, but the suggestion is not final truth by default. Known merchant rules take priority over AI suggestions.

## 6. Ambiguous transaction examples
All examples are synthetic.

| Description | Amount | Why it is ambiguous | Expected handling |
| --- | ---: | --- | --- |
| `PAYMENT SERVICE DEMO 4821` | 18500.00 | Payment services can represent many merchant types. | Review required |
| `MARKETPLACE DEMO ORDER 8842` | 42999.00 | Marketplace purchases do not reveal what was bought. | Review required unless a user rule exists |
| `PHARMACY CAFE DEMO` | 7600.00 | Description matches health and restaurant signals. | Review required due to conflict |
| `TRANSFER DEMO TO WALLET` | 30000.00 | Could be a transfer, payment, or expense depending on context. | Review required |
| `REFUND DEMO STORE` | -12000.00 | Refund treatment affects category totals. | Apply documented refund rule or review |
| `INSTALLMENT 03/06 DEMO ELECTRONICS` | 15000.00 | Installment reporting may need special grouping. | Categorize if merchant is clear, otherwise review |
| `FEE DEMO CARD MAINTENANCE` | 2500.00 | Likely a fee but should remain visible. | Fees and taxes |
| `DEMO CO` | 9999.00 | Merchant is too vague. | Review required |

## 7. Validation rules
Validation must be deterministic.

### File-level validation
- The file must contain the required transaction fields for the MVP.
- Empty files are invalid.
- Files with no processable rows must produce a clear failure or invalid report state.
- Public demo files must use synthetic data only.

### Row-level validation
- Every source row must receive a status.
- Required fields must be present.
- Dates must be valid enough to support reporting.
- Amounts must be valid numeric values.
- Descriptions or merchant fields must be present enough to support auditability or review.
- Invalid rows must be included in the report as invalid items.

### Categorization validation
- Known merchant rules must produce consistent categories.
- Conflicting deterministic rules must create review items.
- Unknown merchants must not be guessed silently.
- AI suggestions, when introduced later, must not override known deterministic rules.

### Total validation
- Processed totals must be calculated deterministically.
- Category totals must be calculated deterministically.
- Expected total comparison must be deterministic.
- Total mismatch must be reported clearly.
- AI must never calculate, reconcile, or validate financial totals.

### Completeness validation
- Total source rows must equal the sum of categorized rows, review rows, invalid rows, and any other explicitly documented status.
- No row count discrepancy is acceptable without a visible validation failure.

## 8. Edge cases
- Missing required fields.
- Empty transaction file.
- Invalid dates.
- Invalid amount formats.
- Zero-value transactions.
- Negative amounts.
- Refunds and reversals.
- Duplicate-looking transactions.
- Installments.
- Fees and taxes.
- Transfers or payments that are not true expenses.
- Unknown merchants.
- Noisy merchant descriptions.
- Category rule conflicts.
- Expected total missing.
- Expected total mismatch.
- Rows that are readable but not trustworthy.
- Transactions that should be excluded from totals but still shown.
- Synthetic demo data that accidentally resembles real personal data too closely.

## 9. Manual review rules
Manual review is a normal domain outcome.

Transactions require manual review when:
- No deterministic rule matches.
- Multiple deterministic rules match different categories.
- A known rule matches but required context is missing.
- The transaction appears to be a duplicate.
- The transaction appears to be a refund, reversal, transfer, payment, installment, fee, or adjustment and the current rule is not precise enough.
- A future AI suggestion has low confidence.
- A future AI suggestion conflicts with a deterministic rule.
- A future AI suggestion is invalid, incomplete, or unsupported by the transaction data.

Manual correction should:
- Record the original category or review state.
- Record the corrected category or decision.
- Record why the correction was made when possible.
- Preserve who or what made the change when the product supports that distinction.
- Be available for future rule improvement.

For the MVP, correction history may be deferred, but manual correction remains part of the domain model.

## 10. Auditability requirements
ExpenseFlow must preserve enough information to explain each report.

The audit trail should answer:
- Was every source row accounted for?
- Which rows were invalid?
- Which rows required review?
- Which deterministic rule categorized a transaction?
- Which rules conflicted?
- Which totals were calculated?
- Did processed totals match expected totals?
- Were any transactions excluded from totals?
- Were manual corrections applied?
- Did any future AI suggestion influence a review decision?

Auditability requirements:
- Keep source row identity or position visible enough for traceability.
- Record categorization method: deterministic rule, manual correction, future accepted AI suggestion, invalid, or review required.
- Record review reason for every review item.
- Record validation status and validation messages.
- Record total calculation inputs at the report level.
- Never hide invalid rows from the user.

## 11. Examples using synthetic data only

### Synthetic transaction examples
| Source row | Date | Description | Amount | Domain result | Category | Reason |
| ---: | --- | --- | ---: | --- | --- | --- |
| 1 | 2026-04-02 | `FRESH MARKET DEMO` | 34500.00 | Categorized | Groceries | Known merchant rule |
| 2 | 2026-04-03 | `METRO DEMO CARD` | 850.00 | Categorized | Transport | Known merchant rule |
| 3 | 2026-04-05 | `PAYMENT SERVICE DEMO 4821` | 18500.00 | Review required | Uncategorized review | Merchant is ambiguous |
| 4 | 2026-04-06 | `DEMO CLOUD TOOLS SUBSCRIPTION` | 12999.00 | Categorized | Subscriptions and software | Description keyword rule |
| 5 | 2026-04-08 | `REFUND DEMO STORE` | -12000.00 | Review required | Uncategorized review | Refund treatment must be confirmed |
| 6 | 2026-04-10 | `CARD MAINTENANCE FEE DEMO` | 2500.00 | Categorized | Fees and taxes | Source or description marker |
| 7 | 2026-04-11 | `` | 7000.00 | Invalid | None | Missing description |

### Synthetic report summary example
| Metric | Value |
| --- | ---: |
| Source rows | 7 |
| Categorized rows | 4 |
| Review rows | 2 |
| Invalid rows | 1 |
| Processed total | 57349.00 |
| Expected total | 57349.00 |
| Validation status | Match |

The totals in examples are illustrative documentation values and must be calculated by deterministic product logic once implementation begins.

## 12. Things the system must never do
- Silently ignore a transaction or source row.
- Use AI to calculate financial totals.
- Use AI to validate expected totals or processed totals.
- Treat an AI suggestion as final truth without confidence and review rules.
- Let AI override a known deterministic merchant rule.
- Hide invalid rows from the report.
- Hide total mismatches from the user.
- Commit real financial data to the repository.
- Use public examples containing real card numbers, bank account numbers, tax IDs, addresses, or personal identifiers.
- Pretend ambiguous transactions are certain.
- Collapse manual review into an invisible implementation detail.
- Present "uncategorized" as a trusted final category without review context.
- Make architecture, database, endpoint, framework, or library decisions from this domain document.
