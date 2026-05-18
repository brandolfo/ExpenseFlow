# Demo Story

## 1. Demo narrative
ExpenseFlow's first portfolio demo tells a focused story:

Santiago has a messy but structured monthly expense export. Instead of manually rebuilding a spreadsheet, he runs ExpenseFlow on a synthetic CSV and receives a structured report that accounts for every row, categorizes known spending, flags ambiguous or unsafe rows for review, validates totals, and explains what happened.

The demo is not a generic expense tracker. It is a backend processing workflow that turns transaction rows into a categorized, validated, auditable report.

The core proof is trust:
- Known merchants are categorized by deterministic rules.
- Unknown or conflicting rows are not guessed.
- Invalid rows remain visible.
- Refund-like and transfer-like rows are visible but excluded from MVP totals.
- Duplicate-looking rows are flagged instead of removed.
- Totals are calculated deterministically and compared with a separately provided expected total.
- No AI is used in the MVP workflow.
- Public data is synthetic.

## 2. Demo user
The demo user is Santiago, a backend developer who wants:
- A practical way to understand exported expense transactions.
- A portfolio-grade project that demonstrates backend depth.
- A trustworthy workflow where every transaction row is accounted for.
- Deterministic financial totals instead of AI-generated or guessed numbers.
- A concise interview story about validation, auditability, and edge-case handling.

## 3. Demo input
The first demo input is the main mixed-behavior synthetic CSV-shaped dataset from `docs/demo-dataset-design.md`.

Conceptual CSV columns:

```text
date,code,description,amount,installment,source_type,notes
```

Required MVP columns:
- `date`
- `description`
- `amount`

Optional columns used by the demo:
- `code`
- `installment`
- `source_type`
- `notes`

Expected total is provided separately from the CSV:

```text
258248.00
```

The demo uses the 22-row mixed-behavior dataset because it proves normal and edge-case behavior in one run. The data includes categorized transactions, unknown merchants, category conflict, refund-like negative amount, transfer-like row, installments, duplicate-looking rows, and invalid rows.

## 4. Demo workflow
The demo workflow is:

1. Provide the synthetic CSV-shaped transaction input.
2. Provide expected total `258248.00` separately.
3. Validate the file shape against the MVP CSV contract.
4. Validate required row fields: `date`, `description`, and `amount`.
5. Apply deterministic rules from the dataset seed set.
6. Categorize known merchants and stable description patterns.
7. Mark unknown, ambiguous, conflicting, duplicate-looking, refund-like, transfer-like, or invalid rows for visible review or invalid reporting.
8. Calculate processed total from eligible rows only.
9. Calculate trusted category totals from categorized rows only.
10. Compare processed total against expected total.
11. Produce the MVP report sections defined by `docs/input-output-contract.md`.
12. Show audit details that explain row outcomes, matched rules, review reasons, invalid-row reasons, and total validation.

This workflow is implementation-agnostic. It does not decide whether the first runnable demo is a CLI, API, local service, or test harness.

## 5. Expected demo output
The demo output is a structured MVP expense report.

Required report sections:
- Report metadata
- Processing counts
- Totals
- Validation result
- Category summary
- Transaction details
- Review items
- Invalid rows
- Audit summary

Expected main dataset summary:

| Metric | Expected value |
| --- | ---: |
| Source rows | 22 |
| Valid rows | 19 |
| Review rows | 6 |
| Invalid rows | 3 |
| Excluded from totals rows | 2 |
| Potential duplicate rows | 1 |
| Processed total | 258248.00 |
| Trusted category total sum | 189149.00 |
| Review-included uncategorized total | 69099.00 |
| Expected total | 258248.00 |
| Expected total validation | match |

Rows excluded from processed total:
- Row 14: refund-like negative amount.
- Row 15: transfer-like movement.
- Rows 20, 21, and 22: invalid rows.

Rows requiring review:
- Row 11: unknown marketplace.
- Row 12: ambiguous payment service.
- Row 13: category conflict.
- Row 14: refund-like negative amount excluded from totals.
- Row 15: transfer-like row excluded from totals.
- Row 19: potential duplicate.

Trusted category totals:

| Category | Expected total |
| --- | ---: |
| Groceries | 54100.00 |
| Restaurants and cafes | 4200.00 |
| Transport | 850.00 |
| Housing and utilities | 18700.00 |
| Subscriptions and software | 12999.00 |
| Health and pharmacy | 7600.00 |
| Education | 9200.00 |
| Entertainment | 6500.00 |
| Travel | 42500.00 |
| Fees and taxes | 2500.00 |
| Shopping | 30000.00 |

## 6. First vertical slice
The first vertical slice is:

Process the 22-row synthetic mixed-behavior CSV-shaped dataset with expected total `258248.00` and produce the MVP report showing deterministic categorization, review items, invalid rows, totals, validation result, and audit summary.

The slice should prove the smallest useful end-to-end backend behavior:
- A supported CSV-shaped input can be accepted.
- Every source row is represented in the output.
- Known rules categorize normal spending.
- Unsafe rows are surfaced instead of guessed.
- Invalid rows are reported instead of dropped.
- Processed total and category totals are deterministic.
- Expected total validation returns `match`.
- The report is auditable enough to explain each important result.

The first vertical slice does not require:
- Actual CSV files before they are explicitly requested.
- A frontend.
- User accounts.
- Persistence.
- AI.
- PDF or Excel imports.
- Manual correction workflow.
- Final backend architecture decisions.

## 7. What must be visible in the demo
The demo must visibly show:
- The source row count is `22`.
- Every row is accounted for.
- Known merchant rules were applied.
- Unknown merchant rows require review.
- Invalid rows are visible with reasons.
- Refund-like and transfer-like rows are visible and excluded from totals.
- Installment markers are preserved for installment-like rows.
- Duplicate-looking row 19 is flagged and not removed.
- Category conflict row 13 requires review.
- Processed total is `258248.00`.
- Expected total validation result is `match`.
- Trusted category totals are separated from review-required rows without trusted categories.
- Audit details explain rule matches, review reasons, invalid-row reasons, and total validation.
- The report states or implies that demo data is synthetic and does not use real financial data.
- No AI-generated categorization, total, validation, or reconciliation appears in the MVP result.

## 8. What is intentionally not included
The demo intentionally excludes:
- Real financial data.
- Actual CSV fixture files until explicitly requested.
- Application code.
- Test code.
- Backend architecture decisions.
- Database, framework, endpoint, module, or deployment choices.
- PDF parsing.
- Excel import.
- Bank integrations.
- Account syncing.
- Authentication or multi-user workflows.
- Frontend dashboards.
- Manual correction and correction history.
- AI categorization, AI summaries, AI validation, or AI total reconciliation.
- Budgeting, forecasting, alerts, or financial advice.

These omissions keep the demo centered on the MVP's backend value: deterministic file processing, validation, reporting, and auditability.

## 9. Backend skills demonstrated
The first demo story is designed to demonstrate:
- File input contract design.
- Row-level validation.
- Deterministic rule evaluation.
- Category mapping from known merchant and description patterns.
- Ambiguity handling without guessing.
- Financial total calculation without AI.
- Expected total validation.
- Edge-case treatment for refunds, transfers, installments, duplicates, category conflicts, and invalid rows.
- Report shaping with multiple sections.
- Audit trail design.
- Traceability from requirements to dataset rows and acceptance tests.
- Scope control and product-driven backend design.
- Privacy-aware synthetic demo data.

The backend skill signal should be: "This project is about trustworthy processing logic, not just storing expense records."

## 10. Interview pitch
Short pitch:

ExpenseFlow is a backend-focused expense processing project that turns messy transaction exports into categorized, validated, auditable reports using deterministic rules.

Expanded pitch:

The first demo processes a synthetic CSV with 22 transaction rows. It categorizes known merchants, flags unknown and conflicting rows for review, reports invalid rows, handles refunds and transfers safely, detects duplicate-looking rows without deleting them, and validates the processed total against an expected total. The important part is that every row is accounted for and every financial total is calculated deterministically. AI is intentionally out of the MVP, because totals and validation need to be trustworthy before any future AI assistance is added.

Portfolio positioning:

ExpenseFlow demonstrates backend judgment: clear contracts, deterministic domain logic, edge-case handling, validation, auditability, and acceptance-test-driven scope. It is intentionally not a generic CRUD app and not an AI wrapper.

## 11. Acceptance criteria for the demo
The demo is successful when:
- It uses the MVP CSV-shaped input contract from `docs/input-output-contract.md`.
- It uses the synthetic mixed-behavior dataset from `docs/demo-dataset-design.md`.
- It provides expected total `258248.00` separately from the CSV.
- It produces all report sections required by the input/output contract.
- It accounts for all 22 source rows.
- It produces source row count `22`, valid count `19`, review count `6`, invalid count `3`, excluded-from-totals count `2`, and potential duplicate count `1`.
- It calculates processed total `258248.00`.
- It reports expected total validation status `match`.
- It calculates trusted category total sum `189149.00`.
- It keeps rows 11, 12, and 13 out of trusted category totals because they require review and have no trusted final category.
- It excludes rows 14, 15, 20, 21, and 22 from processed total.
- It shows rows 20, 21, and 22 as invalid with visible reasons.
- It flags row 19 as a potential duplicate without removing it.
- It records deterministic rule matches and review reasons in audit details.
- It passes or directly supports the relevant acceptance tests in `docs/acceptance-tests.md`, especially AT-001, AT-004 through AT-024, AT-025, AT-026, and AT-028.
- It does not use AI for categorization, totals, validation, explanations, or reconciliation.
- It does not expose real financial data or personal identifiers.

## Assumptions
- The first runnable demo interface is still undecided.
- The first actual CSV fixture file has not been created yet.
- The demo story uses the main mixed-behavior dataset because it best demonstrates portfolio value in one run.
- Row 19 remains included in processed and category totals under the current dataset design, while also being flagged for review as a potential duplicate.

## Risks
- If the first implementation tries to include frontend, persistence, AI, PDF, or Excel support, the demo may drift away from the MVP.
- If audit details are too thin, the demo may look like simple categorization rather than trustworthy processing.
- If processing counts do not clearly distinguish primary statuses from flags, duplicate and exclusion behavior may be confusing.
- If actual public fixtures are created later without review, synthetic data safety could be weakened.
