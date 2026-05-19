# ExpenseFlow

ExpenseFlow is a portfolio-grade ASP.NET Core backend that turns messy CSV expense exports into categorized, validated, auditable reports.

It is intentionally not a generic CRUD app and not a ChatGPT wrapper. The MVP demonstrates deterministic financial data processing first: parsing, validation, categorization rules, review visibility, totals, expected-total validation, structured API responses, and integration tests.

## What It Does

ExpenseFlow accepts raw CSV text and an optional expected total, then returns a structured expense report.

The deterministic workflow is:

```text
CSV input -> row validation -> deterministic categorization -> review/exclusion detection -> totals -> report -> API response
```

The current MVP:

- accepts the documented CSV shape with required `date`, `description`, and `amount` columns
- preserves source row numbers and raw values for auditability
- categorizes known merchants and stable patterns with deterministic seed rules
- flags unknown, ambiguous, conflicting, duplicate-looking, refund-like, and transfer-like rows for review or exclusion
- keeps invalid rows visible instead of dropping them
- calculates processed totals and trusted category totals in code
- compares processed totals against an externally provided expected total
- returns a structured JSON report through a Minimal API endpoint
- uses synthetic fixtures only
- does not use AI in the MVP workflow

## Why This Project Exists

Expense exports are often structured enough to process, but still messy enough that manual spreadsheet work becomes repetitive and easy to get wrong. ExpenseFlow focuses on the backend problem behind that workflow: turn transaction rows into a report that is explainable, testable, and trustworthy.

For portfolio purposes, the project is designed to show backend judgment:

- clear input/output contracts
- deterministic financial logic
- edge-case handling without silent assumptions
- auditability from source row to report
- clean project boundaries
- fixture-backed integration tests
- responsible future AI positioning

## Current MVP Status

Milestone 9 is complete. The deterministic MVP is implemented and documented for portfolio review.

Implemented:

- .NET solution and ASP.NET Core Minimal API
- CSV parser using CsvHelper behind an application abstraction
- deterministic categorization and review detection
- totals, category summaries, expected-total validation, and MVP report generation
- `POST /api/expense-reports/process`
- unit and integration tests
- fixture-backed release gate
- portfolio docs, demo script, API examples, interview pitch, and architecture summary

Next step: future feature work should start from the roadmap, not by expanding the MVP boundary casually. The planned PDF statement ingestion phase is scoped in [docs/pdf-ingestion-plan.md](docs/pdf-ingestion-plan.md).

## Tech Stack

- .NET 10
- ASP.NET Core Minimal APIs
- C#
- CsvHelper for CSV parsing
- xUnit
- Microsoft.AspNetCore.Mvc.Testing for API integration tests

No database, authentication, frontend, Docker, cloud infrastructure, PDF parsing, Excel parsing, background jobs, or AI provider is required for the MVP.

## Architecture Overview

ExpenseFlow uses a pragmatic modular monolith:

```text
ExpenseFlow.Api
  HTTP request/response mapping and DI composition

ExpenseFlow.Application
  workflow orchestration, parser/reporting abstractions, deterministic processing services

ExpenseFlow.Domain
  transaction statuses, categories, report concepts, audit concepts, validation concepts

ExpenseFlow.Infrastructure
  CSV parser implementation and replaceable external adapters
```

The first endpoint stays thin: it validates request shape, calls the application processing service, and maps the report to API DTOs. Parsing, categorization, totals, report generation, and audit decisions live outside the API handler.

See [docs/architecture-summary.md](docs/architecture-summary.md) for the public-friendly architecture explanation.

## Repository Structure

```text
/
  README.md
  AGENTS.md
  docs/
    architecture-summary.md
    api-examples.md
    demo-script.md
    interview-pitch.md
    build-plan.md
    acceptance-tests.md
    demo-dataset-design.md
    pdf-ingestion-plan.md
    decisions.md
  backend/
    ExpenseFlow.sln
    src/
      ExpenseFlow.Api/
      ExpenseFlow.Application/
      ExpenseFlow.Domain/
      ExpenseFlow.Infrastructure/
    tests/
      ExpenseFlow.UnitTests/
      ExpenseFlow.IntegrationTests/
    testdata/
      demo-main.csv
      demo-happy-path.csv
      demo-invalid-rows.csv
      demo-total-mismatch.csv
```

## Run Locally

Restore dependencies:

```powershell
cd backend
dotnet restore
```

Build:

```powershell
cd backend
dotnet build
```

Run tests:

```powershell
cd backend
dotnet test
```

Run the API:

```powershell
cd backend
dotnet run --project src/ExpenseFlow.Api
```

Health check:

```http
GET http://localhost:5000/health
```

## API Endpoint

```http
POST http://localhost:5000/api/expense-reports/process
Content-Type: application/json
```

Request shape:

```json
{
  "sourceName": "demo-main.csv",
  "expectedTotal": 258248.00,
  "csvText": "date,code,description,amount,installment,source_type,notes\n..."
}
```

PowerShell example using the full synthetic fixture from a second terminal at the repository root:

```powershell
$csv = Get-Content -Raw .\backend\testdata\demo-main.csv
$body = @{
  sourceName = "demo-main.csv"
  expectedTotal = 258248.00
  csvText = $csv
} | ConvertTo-Json -Depth 5

Invoke-RestMethod `
  -Uri "http://localhost:5000/api/expense-reports/process" `
  -Method Post `
  -ContentType "application/json" `
  -Body $body
```

See [docs/api-examples.md](docs/api-examples.md) for success, invalid-row, missing-header, missing-expected-total, and mismatch examples.

## Example Response Summary

Processing `backend/testdata/demo-main.csv` with expected total `258248.00` returns:

| Field | Expected value |
| --- | ---: |
| Source rows | 22 |
| Valid rows | 19 |
| Review-required rows | 6 |
| Invalid rows | 3 |
| Excluded-from-totals rows | 2 |
| Potential duplicate rows | 1 |
| Processed total | 258248.00 |
| Trusted category total | 189149.00 |
| Expected total validation | Match |

The response includes:

- report metadata
- processing counts
- totals
- expected total validation
- category summary
- transaction details
- review items
- invalid rows
- excluded rows
- audit summary

## Demo Dataset

The committed fixtures in `backend/testdata/` are synthetic and public-safe:

- `demo-main.csv`: 22-row mixed-behavior dataset for the main demo
- `demo-happy-path.csv`: all categorized rows, no review or invalid rows
- `demo-invalid-rows.csv`: one valid row plus invalid date, missing description, and invalid amount
- `demo-total-mismatch.csv`: same rows as main, intended for mismatch validation with expected total `260000.00`

No real financial data should be committed. Keep any private local files outside version control.

## Release Gate

Before treating the deterministic MVP as release-ready:

- `dotnet restore` succeeds from `backend/`
- `dotnet build` succeeds from `backend/`
- `dotnet test` succeeds from `backend/`
- `demo-main.csv` works end-to-end through `POST /api/expense-reports/process`
- public fixtures remain synthetic
- no source row is silently dropped
- invalid, review-required, excluded, duplicate-looking, and installment rows remain visible
- expected total validation covers match, mismatch, and missing expected total behavior
- the MVP has no AI, database, authentication, frontend, Docker, cloud, PDF, or Excel dependency

## Intentionally Out Of Scope

The MVP deliberately excludes:

- AI integration
- database persistence
- authentication and user accounts
- frontend/dashboard
- Docker or cloud deployment
- PDF parsing
- Excel parsing
- manual correction workflow
- background jobs
- generic CRUD expense management
- financial advice, budgeting, forecasting, or alerts

Future AI can assist review-required rows later, but it must not calculate totals, validate totals, override deterministic rules, or silently finalize ambiguous classifications.

## Future Roadmap

Possible future work:

- manual correction workflow and correction history
- persisted report history
- richer rule management
- export formats for generated reports
- Excel input parser behind the parser boundary
- deterministic PDF statement ingestion from the scoped synthetic ICBC-like variants
- responsible AI suggestions for already review-required transactions
- frontend or dashboard once backend behavior is stable
- authentication and multi-user support after product value is proven
- CI/CD and deployment hardening

## Portfolio And Interview Positioning

The strongest interview story is:

ExpenseFlow demonstrates backend engineering through a trustworthy file-to-report workflow. It parses a synthetic CSV, validates rows, applies deterministic rules, surfaces ambiguity instead of guessing, calculates totals without AI, validates against an expected total, and returns an auditable JSON report. The code is split across API, Application, Domain, and Infrastructure projects, with unit and integration tests guarding the release gate.

Useful docs for review:

- [docs/demo-script.md](docs/demo-script.md)
- [docs/api-examples.md](docs/api-examples.md)
- [docs/interview-pitch.md](docs/interview-pitch.md)
- [docs/architecture-summary.md](docs/architecture-summary.md)
- [docs/acceptance-tests.md](docs/acceptance-tests.md)
