# Acceptance Tests

## Purpose
These acceptance tests define expected ExpenseFlow MVP behavior before implementation begins. They are executable-style product tests: concrete enough to later guide unit tests, integration tests, and end-to-end workflow tests, but not tied to a framework, endpoint, CLI, database, or backend architecture.

The tests protect the deterministic MVP workflow: one supported CSV input is transformed into a categorized, validated, auditable expense report. They also make the release boundary explicit: no AI behavior, no real financial data in public examples, and no silent row dropping.

## Test categories
The MVP acceptance suite covers:
- Supported CSV input
- Required column validation
- Valid transaction processing
- Deterministic categorization
- Unknown merchant review
- Invalid row reporting
- Negative amount/refund handling
- Transfer/payment handling
- Installment handling
- Duplicate-looking rows
- Category conflict
- Expected total present and matching
- Expected total present and mismatching
- Expected total absent
- Report output completeness
- Auditability
- No AI in MVP
- No silent row dropping
- Synthetic data only

## Test format
Each test uses this format:
- ID: stable acceptance test identifier.
- Name: short behavior name.
- Given: input state, dataset rows, or preconditions.
- When: the conceptual processing action.
- Then: observable report behavior.
- Priority: `P0` blocks the MVP, `P1` should pass before public demo, and `P2` can be deferred only with explicit release notes.
- Automation level: expected later automation target, such as unit, integration, or end-to-end.
- Notes: grounding in the source docs or any relevant assumption.

## Acceptance tests

### AT-001
| Field | Value |
| --- | --- |
| ID | AT-001 |
| Name | Accept supported CSV input shape |
| Given | A CSV-shaped input using the columns `date`, `code`, `description`, `amount`, `installment`, `source_type`, and `notes` from the demo dataset design. |
| When | ExpenseFlow processes the file with expected total `258248.00`. |
| Then | The file is accepted as the supported MVP input format, expected total is read separately from the CSV, and processing continues to produce a report. |
| Priority | P0 |
| Automation level | Integration |
| Notes | Grounded in `docs/input-output-contract.md` and `docs/demo-dataset-design.md`. |

### AT-002
| Field | Value |
| --- | --- |
| ID | AT-002 |
| Name | Reject missing required columns |
| Given | A CSV-shaped input missing one or more required columns: `date`, `description`, or `amount`. |
| When | ExpenseFlow validates the input before row processing. |
| Then | The processing result clearly reports the missing required column issue and does not pretend a trustworthy expense report was produced. |
| Priority | P0 |
| Automation level | Integration |
| Notes | Optional columns must not be required. Required column aliases are out of scope for the MVP. |

### AT-003
| Field | Value |
| --- | --- |
| ID | AT-003 |
| Name | Allow missing optional columns |
| Given | A valid CSV-shaped input containing only `date`, `description`, and `amount`. |
| When | ExpenseFlow processes the file. |
| Then | Processing continues without requiring `code`, `installment`, `source_type`, or `notes`; auditability falls back to source row position and available fields. |
| Priority | P0 |
| Automation level | Integration |
| Notes | Confirms optional columns remain optional in the MVP. |

### AT-004
| Field | Value |
| --- | --- |
| ID | AT-004 |
| Name | Process valid categorized grocery transaction |
| Given | Demo row 1: `2026-04-01`, `FRESHVALE MARKET DEMO`, amount `34500.00`. |
| When | ExpenseFlow applies deterministic categorization rules. |
| Then | The row status is `categorized`, category is `Groceries`, it is included in processed total and category totals, and rule `R001` is recorded. |
| Priority | P0 |
| Automation level | Unit |
| Notes | Covers valid transaction processing and known merchant matching. |

### AT-005
| Field | Value |
| --- | --- |
| ID | AT-005 |
| Name | Categorize all happy path rows deterministically |
| Given | Happy path dataset rows 1 through 10 and 16 through 18. |
| When | ExpenseFlow processes the dataset with expected total `179349.00`. |
| Then | Row count is `13`, valid count is `13`, review count is `0`, invalid count is `0`, excluded-from-totals count is `0`, processed total is `179349.00`, and validation result is `match`. |
| Priority | P0 |
| Automation level | End-to-end |
| Notes | Proves the simplest successful MVP workflow without edge cases. |

### AT-006
| Field | Value |
| --- | --- |
| ID | AT-006 |
| Name | Preserve trusted category summary for happy path |
| Given | Happy path dataset rows 1 through 10 and 16 through 18. |
| When | ExpenseFlow produces the category summary. |
| Then | Category totals match the demo dataset design: Groceries `44300.00`, Restaurants and cafes `4200.00`, Transport `850.00`, Housing and utilities `18700.00`, Subscriptions and software `12999.00`, Health and pharmacy `7600.00`, Education `9200.00`, Entertainment `6500.00`, Travel `42500.00`, Fees and taxes `2500.00`, and Shopping `30000.00`. |
| Priority | P0 |
| Automation level | Integration |
| Notes | Category totals must be calculated deterministically by implementation later. |

### AT-007
| Field | Value |
| --- | --- |
| ID | AT-007 |
| Name | Unknown marketplace requires review |
| Given | Demo row 11: `MARKETBOX DEMO ORDER 8842`, amount `42999.00`. |
| When | ExpenseFlow cannot find a safe deterministic category rule. |
| Then | The row is marked `review_required`, has no trusted final category, is included in processed total, is excluded from trusted category totals, and review reason `unknown_marketplace` or equivalent no-safe-match reason is visible. |
| Priority | P0 |
| Automation level | Unit |
| Notes | Unknown merchants must not be guessed. |

### AT-008
| Field | Value |
| --- | --- |
| ID | AT-008 |
| Name | Ambiguous payment service requires review |
| Given | Demo row 12: `PAYBRIDGE SERVICE DEMO 4821`, amount `18500.00`, source type `purchase`. |
| When | ExpenseFlow evaluates the row. |
| Then | The row is marked `review_required`, included in processed total, excluded from trusted category totals, and review reason `ambiguous_payment_service` or equivalent ambiguity reason is visible. |
| Priority | P0 |
| Automation level | Unit |
| Notes | Payment-like names can hide many real purchase types and must remain reviewable. |

### AT-009
| Field | Value |
| --- | --- |
| ID | AT-009 |
| Name | Missing description creates invalid row |
| Given | Demo row 20 has an empty `description` and amount `7000.00`. |
| When | ExpenseFlow validates required row fields. |
| Then | The row is marked `invalid`, visible in the invalid rows section, excluded from processed total, excluded from category totals, and audit details include a missing description error. |
| Priority | P0 |
| Automation level | Unit |
| Notes | Invalid rows must not be dropped. |

### AT-010
| Field | Value |
| --- | --- |
| ID | AT-010 |
| Name | Invalid date creates invalid row |
| Given | Demo row 21 has date `2026-99-01`. |
| When | ExpenseFlow validates the MVP date format and date validity. |
| Then | The row is marked `invalid`, visible in the invalid rows section, excluded from processed total, excluded from category totals, and audit details include an invalid date error. |
| Priority | P0 |
| Automation level | Unit |
| Notes | MVP date format is `YYYY-MM-DD`; impossible dates are invalid. |

### AT-011
| Field | Value |
| --- | --- |
| ID | AT-011 |
| Name | Invalid amount creates invalid row |
| Given | Demo row 22 has amount `abc`. |
| When | ExpenseFlow validates the MVP amount format. |
| Then | The row is marked `invalid`, visible in the invalid rows section, excluded from processed total, excluded from category totals, and audit details include an invalid amount error. |
| Priority | P0 |
| Automation level | Unit |
| Notes | MVP amount format is a signed decimal number using `.` and no currency symbol. |

### AT-012
| Field | Value |
| --- | --- |
| ID | AT-012 |
| Name | Refund-like negative amount is visible and excluded |
| Given | Demo row 14: `REFUND DEMO STORE`, amount `-12000.00`, source type `refund`. |
| When | ExpenseFlow applies MVP refund handling. |
| Then | The row is visible as review/exclusion behavior, not converted into ordinary spending, excluded from processed total, excluded from category totals, and audit details include `refund_like_negative_amount` or equivalent. |
| Priority | P0 |
| Automation level | Unit |
| Notes | Future refund total policy remains open, so the MVP excludes refund-like rows from totals. |

### AT-013
| Field | Value |
| --- | --- |
| ID | AT-013 |
| Name | Transfer row is visible and excluded |
| Given | Demo row 15: `TRANSFER DEMO TO WALLET`, amount `30000.00`, source type `transfer`. |
| When | ExpenseFlow applies MVP transfer/payment handling. |
| Then | The row is visible for review, excluded from processed total, excluded from category totals, and audit details include `transfer_or_payment` or equivalent. |
| Priority | P0 |
| Automation level | Unit |
| Notes | Transfers and payments must not be silently treated as ordinary spending. |

### AT-014
| Field | Value |
| --- | --- |
| ID | AT-014 |
| Name | Installments are processed as individual rows |
| Given | Demo rows 16 and 17 are `PIXELGROVE ELECTRONICS DEMO` with installment markers `03/06` and `04/06`, each amount `15000.00`. |
| When | ExpenseFlow processes installment-like rows. |
| Then | Each row is processed independently, both are categorized as `Shopping` by rule `R015`, both are included in processed and category totals, and the installment markers are preserved in transaction details or audit details. |
| Priority | P0 |
| Automation level | Integration |
| Notes | The MVP does not group, merge, expand, or forecast installments. |

### AT-015
| Field | Value |
| --- | --- |
| ID | AT-015 |
| Name | Duplicate-looking row is flagged, not removed |
| Given | Demo rows 18 and 19 have the same date, amount, and normalized description. |
| When | ExpenseFlow applies duplicate-looking row detection. |
| Then | Row 19 is flagged as `potential_duplicate`, remains visible, requires review, is not removed automatically, and remains included in processed total unless another exclusion rule applies. |
| Priority | P0 |
| Automation level | Integration |
| Notes | The demo dataset includes row 19 in the trusted Groceries total under the current dataset design. |

### AT-016
| Field | Value |
| --- | --- |
| ID | AT-016 |
| Name | Category conflict requires review |
| Given | Demo row 13: `WELLSPRING CAFE PHARMACY DEMO`, amount `7600.00`, matching both `PHARMACY` and `CAFE` signals. |
| When | ExpenseFlow evaluates deterministic categorization rules. |
| Then | The row is marked `review_required`, included in processed total, excluded from trusted category totals, and audit details record the conflicting rules `R006` and `R012` or equivalent conflict evidence. |
| Priority | P0 |
| Automation level | Unit |
| Notes | Conflicts must not be resolved by guessing. |

### AT-017
| Field | Value |
| --- | --- |
| ID | AT-017 |
| Name | Main dataset expected total matches |
| Given | Main mixed-behavior dataset rows 1 through 22 and expected total `258248.00`. |
| When | ExpenseFlow calculates the processed total and validates it against the expected total. |
| Then | Processed total is `258248.00`, expected total validation status is `match`, and the report remains complete with review items and invalid rows visible. |
| Priority | P0 |
| Automation level | End-to-end |
| Notes | Included rows are 1 through 13 and 16 through 19. Rows 14, 15, 20, 21, and 22 are excluded from processed total. |

### AT-018
| Field | Value |
| --- | --- |
| ID | AT-018 |
| Name | Main dataset category totals match expected summary |
| Given | Main mixed-behavior dataset rows 1 through 22. |
| When | ExpenseFlow produces the trusted category summary. |
| Then | Category total sum is `189149.00`; review rows 11, 12, and 13 are included in processed total but do not inflate trusted category totals; excluded and invalid rows do not affect category totals. |
| Priority | P0 |
| Automation level | Integration |
| Notes | Uses the category totals listed in `docs/demo-dataset-design.md`. |

### AT-019
| Field | Value |
| --- | --- |
| ID | AT-019 |
| Name | Expected total mismatch is reported without hiding detail |
| Given | Main mixed-behavior dataset rows 1 through 22 and expected total `260000.00`. |
| When | ExpenseFlow compares expected total to processed total. |
| Then | Processed total remains `258248.00`, validation result is `mismatch`, difference to investigate is `1752.00`, and report details, category summary, review items, invalid rows, and audit summary remain visible. |
| Priority | P0 |
| Automation level | End-to-end |
| Notes | AI must not reconcile or explain the mismatch as financial truth. |

### AT-020
| Field | Value |
| --- | --- |
| ID | AT-020 |
| Name | Expected total absent is allowed |
| Given | Main mixed-behavior dataset rows 1 through 22 and no expected total provided. |
| When | ExpenseFlow produces the report. |
| Then | Processed total is still calculated deterministically as `258248.00`, expected total validation status is `not_provided`, and absence of expected total is not treated as an input error. |
| Priority | P0 |
| Automation level | Integration |
| Notes | Missing expected total limits validation confidence but does not block reporting. |

### AT-021
| Field | Value |
| --- | --- |
| ID | AT-021 |
| Name | Report includes all required sections |
| Given | Main mixed-behavior dataset rows 1 through 22. |
| When | ExpenseFlow produces the MVP report. |
| Then | The report includes report metadata, processing counts, totals, validation result, category summary, transaction details, review items, invalid rows, and audit summary. |
| Priority | P0 |
| Automation level | Integration |
| Notes | Report output shape comes from `docs/input-output-contract.md`. |

### AT-022
| Field | Value |
| --- | --- |
| ID | AT-022 |
| Name | Processing counts account for every source row |
| Given | Main mixed-behavior dataset rows 1 through 22. |
| When | ExpenseFlow calculates processing counts. |
| Then | Source row count is `22`, valid count is `19`, invalid count is `3`, review count is `6`, potential duplicate count is `1`, excluded-from-totals count is `2`, and no row count discrepancy is hidden. |
| Priority | P0 |
| Automation level | Integration |
| Notes | Review rows are 11, 12, 13, 14, 15, and 19. Invalid rows are 20, 21, and 22. |

### AT-023
| Field | Value |
| --- | --- |
| ID | AT-023 |
| Name | Audit trail explains deterministic outcomes |
| Given | Main mixed-behavior dataset rows 1 through 22. |
| When | ExpenseFlow produces audit details. |
| Then | The audit trail can explain every row by source row number, matched rule or validation error, review reason when applicable, inclusion or exclusion from totals, and expected total validation status. |
| Priority | P0 |
| Automation level | Integration |
| Notes | Auditability is required for trust and portfolio value. |

### AT-024
| Field | Value |
| --- | --- |
| ID | AT-024 |
| Name | No source row is silently dropped |
| Given | Main mixed-behavior dataset rows 1 through 22. |
| When | ExpenseFlow produces transaction details, review items, and invalid rows. |
| Then | Every source row number from 1 through 22 appears in exactly one visible row outcome path, with any additional flags visible where applicable. |
| Priority | P0 |
| Automation level | End-to-end |
| Notes | This is the core completeness rule for the MVP. |

### AT-025
| Field | Value |
| --- | --- |
| ID | AT-025 |
| Name | AI is not used in the MVP workflow |
| Given | Any MVP processing run, including unknown merchants, category conflicts, refunds, transfers, and total mismatches. |
| When | ExpenseFlow processes the input and produces the report. |
| Then | No AI-generated category, AI-generated total, AI reconciliation, AI validation, or AI final decision is used; ambiguous rows remain deterministic review items. |
| Priority | P0 |
| Automation level | Integration |
| Notes | Future AI suggestions are explicitly out of the MVP workflow. |

### AT-026
| Field | Value |
| --- | --- |
| ID | AT-026 |
| Name | Public demo data remains synthetic |
| Given | Any dataset, fixture, example, or public demo artifact prepared for the MVP. |
| When | The artifact is reviewed before commit or release. |
| Then | It contains only synthetic merchant names and synthetic codes, and contains no real personal financial data, full card numbers, bank account numbers, addresses, tax IDs, credentials, or personal identifiers. |
| Priority | P0 |
| Automation level | Manual plus automated static checks where practical |
| Notes | This test protects the demo data policy and repository safety. |

### AT-027
| Field | Value |
| --- | --- |
| ID | AT-027 |
| Name | Unsupported input formats are out of MVP |
| Given | A file that is not the supported CSV input, such as Excel or PDF. |
| When | ExpenseFlow receives the file for MVP processing. |
| Then | The file is rejected or reported as unsupported without attempting partial processing, AI extraction, or architecture-specific fallback behavior. |
| Priority | P0 |
| Automation level | Integration |
| Notes | Excel, PDF, and bank integrations are out of scope for the first implementation. |

### AT-028
| Field | Value |
| --- | --- |
| ID | AT-028 |
| Name | Invalid-row dataset remains reportable |
| Given | Dataset variant with rows 1, 20, 21, and 22 and expected total `34500.00`. |
| When | ExpenseFlow processes the dataset. |
| Then | Row count is `4`, valid count is `1`, invalid count is `3`, processed total is `34500.00`, expected total validation result is `match`, and invalid rows remain visible and excluded from totals. |
| Priority | P0 |
| Automation level | End-to-end |
| Notes | The report may be incomplete for trust purposes, but it must still surface invalid rows clearly. |

## MVP release gate
The MVP is not complete until these minimum acceptance tests pass against the implemented deterministic workflow:
- P0 file contract tests: AT-001, AT-002, AT-003, AT-027.
- P0 row processing tests: AT-004, AT-005, AT-007, AT-008, AT-009, AT-010, AT-011, AT-012, AT-013, AT-014, AT-015, AT-016, AT-028.
- P0 totals and summary tests: AT-006, AT-017, AT-018, AT-019, AT-020, AT-022.
- P0 report and audit tests: AT-021, AT-023, AT-024.
- P0 safety and scope tests: AT-025, AT-026.

Passing the release gate means the MVP can demonstrate the deterministic file-to-report workflow with synthetic data. It does not mean future features are complete, including manual correction workflow, AI assistance, PDF parsing, Excel import, dashboards, authentication, or persistent multi-user storage.

## Traceability matrix
| Product requirement | Related acceptance test IDs | Related dataset rows when applicable |
| --- | --- | --- |
| Support the first CSV input shape. | AT-001, AT-002, AT-003, AT-027 | All rows |
| Require exact MVP columns `date`, `description`, and `amount`. | AT-002, AT-003, AT-009, AT-010, AT-011 | 20, 21, 22 |
| Account for every source row. | AT-021, AT-022, AT-023, AT-024 | 1-22 |
| Process valid transactions into deterministic totals. | AT-004, AT-005, AT-017, AT-020 | 1-13, 16-19 |
| Categorize known merchants and patterns deterministically. | AT-004, AT-005, AT-006, AT-014, AT-018 | 1-10, 16-19 |
| Mark unknown merchants for review instead of guessing. | AT-007, AT-008, AT-022, AT-023 | 11, 12 |
| Report invalid rows visibly. | AT-009, AT-010, AT-011, AT-022, AT-028 | 20, 21, 22 |
| Exclude invalid rows from processed and category totals. | AT-009, AT-010, AT-011, AT-017, AT-028 | 20, 21, 22 |
| Handle negative/refund-like rows safely. | AT-012, AT-017, AT-023 | 14 |
| Handle transfer/payment-like rows safely. | AT-013, AT-017, AT-023 | 15 |
| Preserve installment markers and process installments individually. | AT-014 | 16, 17 |
| Flag duplicate-looking rows without automatic removal. | AT-015, AT-022, AT-024 | 18, 19 |
| Surface category conflicts for review. | AT-016, AT-023 | 13 |
| Validate expected total when present and matching. | AT-005, AT-017, AT-028 | 1-22 and dataset variants |
| Validate expected total when present and mismatching. | AT-019 | 1-22 |
| Allow expected total to be absent. | AT-020 | 1-22 |
| Produce required report sections. | AT-021 | 1-22 |
| Preserve auditability. | AT-004, AT-007, AT-012, AT-013, AT-016, AT-021, AT-023, AT-024 | 1-22 |
| Exclude AI from the MVP. | AT-019, AT-025 | All ambiguous or validation cases |
| Use synthetic public data only. | AT-026 | All demo rows |

## Open questions
- Should the processing counts later distinguish primary statuses from flags more explicitly, especially for rows that are both categorized and potential duplicates?
- Should category totals eventually be shown both with and without potential duplicates?
- Should refund rows subtract from category totals, appear in a separate refund section, or support both views in a future version?
- Should transfer/payment rows always be excluded from totals, or should future workflows let the user choose treatment by source?
- What exact public demo file should be created first once actual CSV files are explicitly requested?
- Which implementation interface will run these acceptance tests first: CLI, API, test harness, or another workflow?
