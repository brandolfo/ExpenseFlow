# Input/Output Contract

## Purpose
This document defines the concrete MVP input and output contract for ExpenseFlow so backend architecture and implementation do not need to guess.

This is a product contract. It defines accepted inputs, expected validation behavior, row statuses, MVP treatment rules, and the shape of the report the user should receive. It is not source code, backend architecture, database design, endpoint design, framework selection, or library selection.

## First supported input
The first supported input format is CSV.

CSV is chosen for the MVP because it is simple, common, inspectable, easy to create with synthetic demo data, and sufficient to validate the core file-to-report workflow.

Out of scope for the first implementation:
- Excel files
- PDF statements
- Bank integrations
- Automatic account sync

## Required columns
The MVP CSV must include these required columns:
- `date`
- `description`
- `amount`

Column names are expected to match these names exactly for the MVP. Support for aliases or localized column names is a future decision.

### date

| Field | Description |
| --- | --- |
| Name | `date` |
| Description | The transaction date from the source file. |
| Accepted examples | `2026-04-02`, `2026-04-30` |
| Invalid examples | empty value, `not-a-date`, `31/31/2026`, `2026-99-01` |
| Validation behavior | If missing or invalid, the row status is `invalid`. The row remains visible in the report and is not silently ignored. |

MVP date format: `YYYY-MM-DD`.

Other date formats are out of scope for the first implementation unless explicitly added later.

### description

| Field | Description |
| --- | --- |
| Name | `description` |
| Description | The raw merchant, transaction, or movement description from the source file. |
| Accepted examples | `FRESH MARKET DEMO`, `DEMO CLOUD TOOLS SUBSCRIPTION`, `TRANSFER DEMO TO WALLET` |
| Invalid examples | empty value, whitespace-only value |
| Validation behavior | If missing or blank, the row status is `invalid`. The row remains visible in the report and is not silently ignored. |

The description is used for deterministic merchant matching, category rules, review reasons, and auditability.

### amount

| Field | Description |
| --- | --- |
| Name | `amount` |
| Description | The transaction amount as a signed decimal value. Positive amounts represent ordinary debits/spending in the MVP examples. Negative amounts represent refunds, reversals, credits, or other non-standard movements that require explicit handling. |
| Accepted examples | `34500.00`, `850`, `0.00`, `-12000.00` |
| Invalid examples | empty value, `abc`, `12.34.56`, `$1200`, `1,200.00`, `1200,00` |
| Validation behavior | If missing or not a valid MVP numeric value, the row status is `invalid`. The row remains visible in the report and is not silently ignored. |

MVP amount format: signed decimal number using `.` as the decimal separator and no currency symbol.

Support for comma decimal separators, currency symbols, and thousands separators is a future decision.

## Optional columns
The MVP CSV may include these optional columns:
- `code`
- `installment`
- `source_type`
- `notes`

Optional columns must not be required for successful processing.

### code

| Field | Description |
| --- | --- |
| Name | `code` |
| Description | Optional source transaction code, authorization code, movement code, or user-provided reference. |
| Behavior when missing | Processing continues. Auditability falls back to source row position and available fields. |

### installment

| Field | Description |
| --- | --- |
| Name | `installment` |
| Description | Optional installment marker, such as `03/06`, `3 of 6`, or a source-provided installment label. |
| Behavior when missing | Processing continues. The transaction is treated as a normal row unless the description indicates an installment. |

In the MVP, installments are processed as individual rows. They are not grouped, merged, forecasted, or expanded.

### source_type

| Field | Description |
| --- | --- |
| Name | `source_type` |
| Description | Optional source-provided movement type, such as `purchase`, `refund`, `transfer`, `payment`, `fee`, or `adjustment`. |
| Behavior when missing | Processing continues. Treatment relies on description, amount sign, and deterministic rules. |

The MVP does not blindly trust `source_type`; it is a signal for review and categorization behavior.

### notes

| Field | Description |
| --- | --- |
| Name | `notes` |
| Description | Optional user or source notes that may help explain the transaction. |
| Behavior when missing | Processing continues. No review reason is created solely because notes are missing. |

Notes are not a substitute for required fields.

## Expected total
Expected total is not part of the CSV.

### How it is provided conceptually
Expected total is provided separately by the caller or user when processing starts. It represents the source total the user wants ExpenseFlow to compare against the processed total.

This document does not define whether the caller is a CLI, API, UI, test harness, or other interface.

### Behavior when present
When expected total is present:
- ExpenseFlow calculates the processed total deterministically.
- ExpenseFlow compares processed total to expected total deterministically.
- The report includes a validation result.
- A mismatch creates a validation warning.
- A mismatch must not prevent the report from showing rows, categories, invalid rows, review items, and audit details.

### Behavior when absent
When expected total is absent:
- ExpenseFlow still calculates processed total deterministically.
- The report marks expected total validation as `not_provided`.
- The absence of expected total is not an error.
- The report should still show all processing counts, category totals, review items, invalid rows, and audit summary.

### Validation output
Expected total validation can produce:
- `match`: expected total was provided and matches processed total.
- `mismatch`: expected total was provided and does not match processed total.
- `not_provided`: expected total was not provided.
- `not_applicable`: totals cannot be meaningfully compared because the input had no processable valid rows.

## Row statuses
The MVP report should make row status visible. A row may have one primary status and additional flags such as potential duplicate or excluded from totals.

| Status | Meaning | Included in processed total? | Included in category totals? | Visible in report? | Requires review? |
| --- | --- | --- | --- | --- | --- |
| `valid` | The row has valid required fields but has not yet reached a final category outcome. | Yes, unless also excluded from totals. | No, unless categorized. | Yes | No by itself |
| `categorized` | The row is valid and has a category assigned by deterministic rules. | Yes, unless also excluded from totals. | Yes | Yes | No |
| `review_required` | The row is valid enough to inspect but not safe to finalize automatically. | Yes, unless also excluded from totals. | No trusted category total unless a review category is explicitly shown separately. | Yes | Yes |
| `invalid` | The row is missing required data or has invalid values. | No | No | Yes | Yes |
| `excluded_from_totals` | The row is visible but not included in processed or category totals because its treatment is not ordinary spending for MVP reporting. | No | No | Yes | Yes |
| `potential_duplicate` | The row resembles another row closely enough that the user should review it. | Yes, unless also excluded from totals. | Yes if categorized; otherwise no trusted category total. | Yes | Yes |

Notes:
- `potential_duplicate` is a flag-like status. It must not cause automatic removal.
- `excluded_from_totals` must be visible and explained.
- No status allows silent omission.

## MVP treatment rules

### Normal purchases
Normal purchases are rows with valid `date`, `description`, and `amount`, no exclusion signal, and a deterministic category rule match.

Behavior:
- Status: `categorized`
- Included in processed total: yes
- Included in category totals: yes
- Review required: no
- Audit summary records the matched deterministic rule.

### Unknown merchants
Unknown merchants are valid rows where no deterministic category rule safely matches.

Behavior:
- Status: `review_required`
- Included in processed total: yes, unless excluded by another treatment rule
- Included in category totals: no trusted final category total
- Review required: yes
- Audit summary records `no_matching_rule`.

### Invalid rows
Invalid rows are rows with missing or invalid required fields.

Behavior:
- Status: `invalid`
- Included in processed total: no
- Included in category totals: no
- Review required: yes
- Visible in invalid rows section
- Audit summary records the validation error.

### Negative amounts/refunds
Negative amounts are valid numeric values but require explicit interpretation.

Behavior:
- If description or `source_type` suggests refund, reversal, credit, adjustment, or similar, status is `review_required`.
- Refund-like rows remain visible in the report.
- Refund-like rows are not silently converted into ordinary spending.
- MVP default: refund-like rows are `excluded_from_totals` until the product has a documented refund total policy.
- Audit summary records the negative amount and review/exclusion reason.

Open issue: whether a future version should subtract refunds from category totals, show them separately, or support both views.

### Transfers/payments
Transfers and payments must not be silently treated as ordinary spending.

Behavior:
- If description or `source_type` suggests transfer or payment, status is `review_required`.
- MVP default: transfer/payment rows are `excluded_from_totals`.
- They remain visible in report details and review items.
- Audit summary records transfer/payment treatment reason.

### Installments
Installments are processed as individual rows in the MVP.

Behavior:
- Each installment row is processed independently.
- Installments are not grouped, merged, expanded, or forecasted.
- If the merchant/category is clear, the row may be categorized.
- If the installment marker creates ambiguity, the row is `review_required`.
- Audit summary preserves the installment marker when present.

### Duplicate-looking rows
Duplicate-looking rows must not be removed automatically.

Behavior:
- Rows that look duplicated are flagged as `potential_duplicate`.
- Potential duplicates remain visible in the report.
- Potential duplicates require review.
- Potential duplicates are included in totals unless also excluded by another treatment rule.
- Audit summary records duplicate signals, such as same date, same amount, and similar description.

### Category conflicts
Category conflicts happen when more than one deterministic rule matches different categories.

Behavior:
- Status: `review_required`
- Included in processed total: yes, unless excluded by another treatment rule
- Included in category totals: no trusted final category total
- Review required: yes
- Audit summary records conflicting rules.

### Missing expected total
Missing expected total is allowed.

Behavior:
- Report still calculates processed total.
- Expected total validation result is `not_provided`.
- The report is still useful, but cannot confirm source-total match.

### Total mismatch
Total mismatch happens when expected total is provided and does not match processed total.

Behavior:
- Report is still produced.
- Validation result is `mismatch`.
- The mismatch is visible in the totals and validation sections.
- Review items and invalid rows help explain possible causes.
- AI must not reconcile or explain the mismatch as financial truth.

## Output report shape
The MVP output report should include these sections:
- Report metadata
- Processing counts
- Totals
- Validation result
- Category summary
- Transaction details
- Review items
- Invalid rows
- Audit summary

The examples below are JSON-like product examples. They are not code and do not choose an implementation format.

### Report metadata
```json
{
  "report_metadata": {
    "product": "ExpenseFlow",
    "report_type": "mvp_expense_report",
    "input_format": "csv",
    "source_name": "synthetic-april-demo.csv",
    "generated_at": "conceptual timestamp",
    "uses_real_financial_data": false
  }
}
```

### Processing counts
```json
{
  "processing_counts": {
    "source_rows": 8,
    "valid_rows": 6,
    "categorized_rows": 3,
    "review_required_rows": 3,
    "invalid_rows": 1,
    "excluded_from_totals_rows": 2,
    "potential_duplicate_rows": 1
  }
}
```

### Totals
```json
{
  "totals": {
    "expected_total": 50849.00,
    "processed_total": 50849.00,
    "category_total": 35849.00,
    "excluded_from_totals_total": -12000.00
  }
}
```

Totals must be calculated deterministically by product logic when implementation begins.

### Validation result
```json
{
  "validation_result": {
    "expected_total_status": "match",
    "messages": [
      "Expected total was provided separately.",
      "Processed total matches expected total.",
      "Two rows were excluded from totals and remain visible for review."
    ]
  }
}
```

### Category summary
```json
{
  "category_summary": [
    {
      "category": "Groceries",
      "transaction_count": 1,
      "total": 34500.00
    },
    {
      "category": "Transport",
      "transaction_count": 1,
      "total": 850.00
    },
    {
      "category": "Fees and taxes",
      "transaction_count": 1,
      "total": 499.00
    }
  ]
}
```

Rows requiring review should not inflate trusted category totals.

### Transaction details
```json
{
  "transaction_details": [
    {
      "source_row": 1,
      "date": "2026-04-02",
      "description": "FRESH MARKET DEMO",
      "amount": 34500.00,
      "status": "categorized",
      "category": "Groceries",
      "included_in_processed_total": true,
      "included_in_category_totals": true,
      "review_required": false
    },
    {
      "source_row": 2,
      "date": "2026-04-03",
      "description": "PAYMENT SERVICE DEMO 4821",
      "amount": 18500.00,
      "status": "review_required",
      "category": null,
      "included_in_processed_total": true,
      "included_in_category_totals": false,
      "review_required": true
    }
  ]
}
```

### Review items
```json
{
  "review_items": [
    {
      "source_row": 2,
      "reason": "no_matching_rule",
      "description": "PAYMENT SERVICE DEMO 4821",
      "suggested_action": "Review merchant and choose category in a later manual correction workflow."
    },
    {
      "source_row": 5,
      "reason": "refund_like_negative_amount",
      "description": "REFUND DEMO STORE",
      "suggested_action": "Confirm refund treatment before trusting category totals."
    }
  ]
}
```

Manual review visibility is in MVP. Manual correction workflow is deferred to Phase 2.

### Invalid rows
```json
{
  "invalid_rows": [
    {
      "source_row": 7,
      "raw_values": {
        "date": "2026-04-11",
        "description": "",
        "amount": "7000.00"
      },
      "errors": [
        "description is required"
      ],
      "included_in_processed_total": false
    }
  ]
}
```

Invalid rows must be visible and must not be silently ignored.

### Audit summary
```json
{
  "audit_summary": {
    "rules_applied": [
      {
        "rule": "known_merchant",
        "source_row": 1,
        "result": "Groceries"
      }
    ],
    "review_reasons": [
      {
        "source_row": 2,
        "reason": "no_matching_rule"
      }
    ],
    "validation_messages": [
      "All source rows were accounted for.",
      "Expected total validation status: match."
    ]
  }
}
```

## Out of scope
The MVP input/output contract explicitly excludes:
- Excel
- PDF
- Bank integrations
- AI categorization
- Manual correction workflow
- Frontend
- Authentication
- Persistent multi-user accounts

## Open questions
- Should future versions support comma decimal separators such as `1200,00`?
- Should future versions support localized date formats such as `DD/MM/YYYY`?
- Should refund rows subtract from category totals, appear separately, or support both views?
- Should transfer/payment rows always be excluded from totals, or should the user choose by source?
- What exact deterministic merchant rules should be included in the synthetic demo dataset?
- What is the final category taxonomy after validating synthetic examples?
- What should be the first public demo report filename and narrative?
