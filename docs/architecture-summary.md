# ExpenseFlow Architecture Summary

## Purpose

This is the short, public-friendly architecture summary for the implemented MVP. The detailed design remains in `docs/backend-architecture.md`.

ExpenseFlow is a modular monolith built around one backend file-to-report workflow:

```text
CSV input -> parsing -> validation -> deterministic categorization -> totals/reporting -> API response
```

The implemented PDF phase adds a second source-ingestion path without creating a second financial pipeline:

```text
synthetic PDF fixture -> PdfPig extraction -> PDF row normalization -> existing deterministic processing pipeline -> PDF API response
```

The architecture is intentionally simple. It proves the processing workflow without adding persistence, authentication, frontend, Docker, cloud infrastructure, Excel parsing, OCR, LLM integration, background jobs, or external APIs.

## Project Boundaries

```text
ExpenseFlow.Api
  Minimal API host, request/response DTOs, endpoint mapping, dependency registration

ExpenseFlow.Application
  use-case orchestration, parser/categorizer/report abstractions, deterministic workflow services

ExpenseFlow.Domain
  transaction states, categories, validation concepts, report totals, audit concepts

ExpenseFlow.Infrastructure
  CsvHelper parser implementation and PdfPig PDF extractor behind application abstractions
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

## PDF Ingestion Architecture

The PDF endpoint follows this path:

1. `ExpenseFlow.Api` receives `sourceName`, optional `expectedTotal`, `pdfBase64`, and optional `statementShapeHint`.
2. The endpoint validates request shape, supported hint values, valid base64, and the 5 MB decoded PDF limit.
3. `ExpenseFlow.Application` calls `IPdfExpenseReportProcessingService`.
4. `ExpenseFlow.Infrastructure` uses PdfPig inside `PdfPigPdfStatementExtractor` to extract text from text-selectable PDFs.
5. `DeterministicPdfStatementRowNormalizer` supports only `icbc-visa-like-v1` and `icbc-mastercard-like-v1`.
6. ARS rows are converted into existing parsed transaction candidates.
7. USD/non-ARS rows and malformed transaction-like candidates remain visible as invalid or unprocessable rows.
8. The existing categorization and report generator calculate totals, validation, review items, invalid rows, excluded rows, and audit output.

Boundary rules:

- PdfPig is isolated to Infrastructure.
- QuestPDF is isolated to `backend/tools/ExpenseFlow.SyntheticPdfGenerator/` for synthetic fixture generation.
- Application and Domain do not reference PdfPig, QuestPDF, OCR, LLM, database, or API infrastructure.
- Extracted statement totals are metadata/evidence only and are not trusted validation input.
- Supported PDF fixtures are synthetic only; real/private PDFs must not be committed.

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

The CSV release gate requires:

- restore/build/test success
- full source row accounting
- deterministic totals
- visible invalid rows
- visible review items
- visible excluded rows
- synthetic fixture safety
- no AI, database, authentication, frontend, Docker, cloud, or Excel dependency

The PDF phase release gate additionally requires:

- both synthetic PDF variants process through `POST /api/expense-reports/process-pdf`
- Mastercard-like multi-page extraction remains covered
- no extracted or normalized PDF row is silently dropped
- non-ARS rows remain visible and are not counted in ARS totals
- public PDF fixtures remain synthetic
- PdfPig and QuestPDF stay within their accepted boundaries
- OCR, LLM, arbitrary PDF support, persistence, auth, frontend, Docker, cloud, and external APIs remain out of scope

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
- additional PDF variants only after separate scope and fixture decisions
- OCR/LLM assistance only after separate decisions
- AI suggestions for review-required rows
- authentication and multi-user support after product value is proven
