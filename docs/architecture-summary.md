# ExpenseFlow Architecture Summary

## Purpose

This is the short, public-friendly architecture summary for the implemented MVP. The detailed design remains in `docs/backend-architecture.md`.

ExpenseFlow is a modular monolith built around one backend workflow:

```text
CSV input -> parsing -> validation -> deterministic categorization -> totals/reporting -> API response
```

The architecture is intentionally simple. It proves the processing workflow without adding persistence, authentication, frontend, Docker, cloud infrastructure, PDF/Excel parsing, background jobs, or AI integration.

## Project Boundaries

```text
ExpenseFlow.Api
  Minimal API host, request/response DTOs, endpoint mapping, dependency registration

ExpenseFlow.Application
  use-case orchestration, parser/categorizer/report abstractions, deterministic workflow services

ExpenseFlow.Domain
  transaction states, categories, validation concepts, report totals, audit concepts

ExpenseFlow.Infrastructure
  CsvHelper parser implementation behind the application parser abstraction
```

## Data Flow

1. `ExpenseFlow.Api` receives `sourceName`, optional `expectedTotal`, and raw `csvText`.
2. The endpoint validates request shape and calls `IExpenseReportProcessingService`.
3. `ExpenseFlow.Application` orchestrates parsing, categorization, and report generation.
4. `ExpenseFlow.Infrastructure` parses CSV text behind `ITransactionFileParser`.
5. Valid parsed rows move to deterministic categorization.
6. Invalid rows are carried forward with source row numbers, raw values, and validation errors.
7. Categorization marks rows as categorized, review-required, excluded from totals, or potential duplicates.
8. Report generation calculates processed totals, trusted category totals, expected-total validation, processing counts, review items, excluded rows, invalid rows, and audit summary.
9. `ExpenseFlow.Api` maps the report to explicit API DTOs and returns JSON.

## Why This Shape

The important behavior is not HTTP plumbing. It is deterministic financial processing and auditability. The project boundaries keep that behavior testable without starting ASP.NET Core.

Key choices:

- Minimal API for a small first HTTP surface.
- Application service for orchestration instead of putting workflow logic in the endpoint.
- Domain concepts independent from ASP.NET Core, CsvHelper, database libraries, and AI providers.
- CsvHelper isolated in Infrastructure.
- Report model generated before API mapping so future exports can reuse the same processing result.

## Deterministic MVP Workflow

The MVP handles:

- required column validation
- row validation for date, description, and amount
- known merchant and keyword categorization
- unknown merchant review
- ambiguous payment-service review
- category conflict review
- refund-like and transfer-like exclusion
- duplicate-looking row flags
- installment preservation
- processed totals
- trusted category totals
- expected total match, mismatch, and missing expected total
- audit summary

No source row may be silently ignored.

## Future AI Seam

AI is intentionally not part of the MVP workflow.

The future seam is application-facing: AI could later receive already review-required rows and return structured suggestions. Those suggestions would need validation, confidence, and audit records. AI must not calculate totals, validate expected totals, override deterministic rules, or silently finalize ambiguous rows.

This keeps the MVP credible as deterministic backend processing while still allowing responsible AI expansion later.

## Testing Strategy

The test suite is split by risk:

- Unit tests cover domain concepts, deterministic categorization, totals, validation, and report generation.
- Integration tests cover CSV fixtures, parser behavior, application-level report generation, API request/response behavior, and release-gate checks.
- Public synthetic fixtures in `backend/testdata/` prove the main workflow, happy path, invalid-row behavior, and mismatch behavior.

The release gate requires:

- restore/build/test success
- full source row accounting
- deterministic totals
- visible invalid rows
- visible review items
- visible excluded rows
- synthetic fixture safety
- no AI, database, authentication, frontend, Docker, cloud, PDF, or Excel dependency

## MVP vs Future Architecture

MVP:

- one processing endpoint
- raw CSV text in JSON
- in-code deterministic seed rules
- in-memory report generation
- no persistence
- no auth
- no AI

Future:

- manual correction workflow
- persisted report/correction history
- richer rule management
- export adapters
- Excel parser behind the parser abstraction
- AI suggestions for review-required rows
- authentication and multi-user support after product value is proven
