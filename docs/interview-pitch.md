# ExpenseFlow Interview Pitch

## 30-Second Pitch

ExpenseFlow is a backend-focused expense processing project built with ASP.NET Core and .NET. It takes a messy but structured CSV expense export, validates every row, categorizes known merchants with deterministic rules, flags ambiguous or unsafe rows for review, calculates totals in code, validates against an expected total, and returns an auditable JSON report. The MVP deliberately avoids AI, persistence, auth, and frontend work so the first slice proves trustworthy backend processing.

## Two-Minute Technical Explanation

The first vertical slice processes a synthetic 22-row CSV fixture. The API accepts raw CSV text and an optional expected total in JSON. The application service orchestrates parsing, deterministic categorization, and report generation.

CsvHelper is isolated in Infrastructure behind `ITransactionFileParser`. The parser validates required headers, preserves source row numbers, and splits valid parsed rows from invalid rows. The categorization engine applies seed rules for known merchants and stable patterns, while unknown merchants, ambiguous payment services, category conflicts, refunds, transfers, and duplicate-looking rows stay visible as review or excluded outcomes.

The report generator calculates processed totals and trusted category totals deterministically. Invalid rows, refund-like rows, and transfer-like rows are excluded from totals. Review-required rows without trusted categories do not inflate category totals. Expected total validation supports match, mismatch, and missing expected total. The API returns metadata, counts, totals, validation, category summary, transaction details, review items, invalid rows, excluded rows, and audit summary.

The release gate is covered by unit and integration tests using committed synthetic fixtures. The important proof is that no source row is silently dropped and no AI is used for financial correctness.

## Backend Skills Demonstrated

- ASP.NET Core Minimal API design
- C# and .NET 10 solution structure
- modular monolith boundaries
- application orchestration outside HTTP handlers
- domain modeling for statuses, categories, validation, reports, and audit
- Infrastructure adapter for CSV parsing with CsvHelper
- deterministic rule evaluation
- row-level validation and file-level validation
- total calculation with decimal values
- expected total validation
- audit-friendly API response mapping
- xUnit unit tests and API integration tests
- fixture-backed release gate
- product-driven scope control

## Architecture Decisions Worth Discussing

API is thin:

The endpoint validates request shape, calls the application service, and maps the report to DTOs. It does not own parsing, categorization, totals, or audit decisions.

Application owns the workflow:

`DeterministicExpenseReportProcessingService` coordinates parser, categorization engine, and report generator.

Domain stays independent:

Domain models transaction statuses, categories, report totals, validation status, and audit concepts without referencing ASP.NET Core, CsvHelper, database libraries, or AI providers.

Infrastructure is replaceable:

CsvHelper is isolated in `ExpenseFlow.Infrastructure`. Future file parsers can be added behind the same application-facing parser boundary.

AI is future-ready, not MVP behavior:

The architecture leaves room for future review assistance, but the deterministic MVP does not call AI and does not depend on provider packages.

## Trade-Offs

Raw CSV text in JSON:

Chosen because it is easy to test, avoids local file path and multipart complexity, and exercises the full workflow. Multipart upload can be added later.

Seed rules in code:

Good for an MVP and portfolio demo because behavior is explicit and testable. A future rule store can be added after rule management becomes a real need.

No persistence:

The first slice proves processing behavior. Persistence would add infrastructure before the core workflow needed it.

Exact expected-total comparison:

The fixture uses decimal values and exact equality. If future inputs introduce rounding or currency precision issues, tolerance should be explicit and tested.

Refunds and transfers excluded from totals:

This avoids silently treating non-spending movements as ordinary expenses. Future versions can add user-configurable treatment after manual review exists.

## What Was Intentionally Not Built

- generic expense CRUD screens
- database persistence
- authentication or user accounts
- frontend dashboard
- Docker or cloud deployment
- PDF parsing
- Excel parsing
- multipart upload
- manual correction workflow
- background jobs
- AI categorization
- AI total validation
- AI reconciliation

These omissions keep the MVP focused on the hardest trust problem first: deterministic file-to-report processing.

## How This Differs From A CRUD App

A CRUD expense app stores and edits records. ExpenseFlow processes a messy input file and explains how each source row became part of a report. The core value is transformation, validation, review visibility, deterministic totals, and auditability.

The demo is interesting because the input includes invalid rows, unknown merchants, a category conflict, refund-like and transfer-like movements, installments, and a duplicate-looking row. The system must account for all of them without silently guessing or dropping data.

## How This Differs From A ChatGPT Wrapper

The MVP does not use AI. Known merchant categorization, row validation, total calculation, expected total validation, review detection, and audit output are deterministic.

Future AI could help only after deterministic processing has marked rows as review-required. AI suggestions would need structured output, confidence, validation, and audit records. AI would not calculate totals or decide financial correctness.

## Possible Interview Questions

### Why did you use Minimal APIs?

The API surface is small and the behavior belongs in application/domain code. Minimal APIs keep the first endpoint lightweight while still supporting clean request/response contracts and integration testing.

### Why no database?

Persistence is not needed to prove the first vertical slice. The MVP goal is deterministic processing from CSV input to report output. Adding a database early would distract from parsing, validation, totals, and auditability.

### How do you know no rows are dropped?

The parser preserves source row numbers, the report has processing counts, and tests assert that every source row from the fixture appears in transaction details, review items, invalid rows, or excluded rows.

### How are invalid rows handled?

Missing description, invalid date, and invalid amount become visible invalid rows. They keep raw values and errors, do not contribute to totals, and do not stop processable rows from producing a report.

### Why are refunds and transfers excluded?

In the MVP, they are not ordinary spending. Excluding them prevents misleading totals while keeping them visible for review. Future refund/transfer policy can be refined once manual correction exists.

### Why not let AI categorize unknown merchants?

Because the first trust boundary is deterministic. Unknown or ambiguous rows should be visible instead of guessed. AI can later suggest categories for review-required rows, but it should not become the source of financial truth.

### What would you improve next?

I would add a manual correction workflow with persisted correction history, then use repeated corrections to suggest new deterministic rules. After that, AI suggestions could be added for review-required rows behind a clear application boundary.
