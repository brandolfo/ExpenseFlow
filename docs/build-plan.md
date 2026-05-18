# Build Plan

## 1. Purpose
This document turns the accepted backend architecture into an implementation sequence for the first ASP.NET Core / .NET vertical slice of ExpenseFlow.

It is a build plan, not application code. It defines the order of work, milestone boundaries, expected tests, commit shape, and implementation decisions needed to move from documentation to a runnable MVP while preserving the product constraints in `docs/input-output-contract.md`, `docs/demo-dataset-design.md`, `docs/acceptance-tests.md`, `docs/demo-story.md`, and `docs/backend-architecture.md`.

## 2. Build principles
- Keep commits small and reviewable.
- Build one vertical slice before expanding.
- Do not add AI in the MVP.
- Do not add database persistence in the first slice.
- Do not add authentication in the first slice.
- Do not add frontend in the first slice.
- Do not add PDF or Excel parsing in the first slice.
- Keep deterministic logic testable outside HTTP endpoints.
- Keep every source row visible in the output.
- Keep parsing, categorization, validation, reporting, and audit logic separated.
- Prefer explicit behavior over clever abstractions.
- Use synthetic data only for committed fixtures.
- Treat acceptance tests as the MVP release gate.

## 3. Technology decisions for implementation
### .NET version
Use .NET 10 LTS, matching the accepted backend architecture decision.

If a local machine cannot use .NET 10 immediately, stop and document the tooling blocker before falling back to .NET 8 LTS. The portfolio target remains .NET 10 LTS.

### ASP.NET Core API style
Use ASP.NET Core Minimal APIs for the first slice.

Reason:
- The first API surface is small.
- Minimal APIs keep HTTP plumbing light.
- The important behavior should live in application/domain code, not controllers.
- Controllers can be added later if the API grows.

### Test framework
Use xUnit for unit and integration tests.

Reason:
- It is common in .NET backend projects.
- It works well with ASP.NET Core integration testing.
- It keeps the test stack familiar for portfolio reviewers.

Use `Microsoft.AspNetCore.Mvc.Testing` or the current ASP.NET Core test host package for API-level integration tests.

### CSV parsing approach
Use CsvHelper for the first real parser implementation.

Decision:
- Do not hand-roll CSV parsing beyond trivial tests.
- CsvHelper is a focused library that handles headers, quoted values, empty values, and row-level parsing more safely than ad hoc string splitting.
- Keep CsvHelper isolated inside `ExpenseFlow.Infrastructure` behind `ITransactionFileParser`.

This is a justified dependency because CSV parsing is core to the MVP input contract.

### First request/response style
Use a JSON request containing raw CSV text for the first API endpoint.

Recommended first request shape conceptually:

```json
{
  "sourceName": "synthetic-april-demo.csv",
  "csvText": "date,code,description,amount,installment,source_type,notes\n...",
  "expectedTotal": 258248.00
}
```

Reason:
- Easier to test than multipart upload.
- Avoids local file path security and environment differences.
- Keeps the first endpoint deterministic and self-contained.
- Still exercises the real parsing, validation, categorization, totals, and reporting workflow.

Deferred options:
- Multipart upload can be added later when file upload UX matters.
- Local file path input can exist in a CLI or development harness later, but it should not be the first API contract.

### Fixture strategy
Create actual synthetic CSV fixture files in the first coding phase, after the solution skeleton exists.

Decision:
- Milestone 2 should create committed synthetic fixtures based on `docs/demo-dataset-design.md`.
- Fixtures should include main mixed-behavior, happy path, invalid rows, and total mismatch scenario data where useful.
- Fixture files must be reviewed against the demo data policy before commit.
- Real local data must stay outside version control.

### Local run strategy
Use the local .NET SDK only:
- `dotnet restore`
- `dotnet build`
- `dotnet test`
- `dotnet run --project src/ExpenseFlow.Api`

Do not add Docker, database containers, cloud services, queues, or background workers for the first slice.

### Test timing
Start tests with the skeleton and domain work, not after the whole API is complete.

Decision:
- Milestone 1 should include a basic build and health endpoint test.
- Milestones 3 through 6 should add unit/application tests with the behavior.
- Milestone 7 should add API mapping tests.
- Milestone 8 should harden the release gate with full integration coverage.

## 4. First vertical slice definition
The first runnable end-to-end slice processes the main 22-row synthetic dataset with expected total `258248.00` and returns the MVP report.

The slice must support:
- Supported CSV input.
- Required column validation.
- Row validation.
- Deterministic categorization.
- Review item detection.
- Excluded rows.
- Invalid rows.
- Processed total.
- Category totals.
- Expected total validation.
- Audit summary.
- No AI.

Required output proof:
- Source rows: `22`.
- Valid rows: `19`.
- Review rows: `6`.
- Invalid rows: `3`.
- Excluded-from-totals rows: `2`.
- Potential duplicate rows: `1`.
- Processed total: `258248.00`.
- Trusted category total sum: `189149.00`.
- Expected total validation: `match`.

## 5. Milestones
### Milestone 0 - Repository implementation readiness
| Field | Plan |
| --- | --- |
| Goal | Prepare the repository for implementation without adding application logic. |
| Files/projects likely affected | `.gitignore`, `README.md`, possibly `docs/build-plan.md` follow-up edits. |
| Behavior delivered | Clear implementation guardrails, local ignored folders, and confirmed source/test folder plan. |
| Tests expected | No product tests yet. Optionally verify `git status` and README links. |
| Acceptance tests covered | None directly. Supports AT-026 by preventing accidental real-data commits. |
| Suggested commit message | `Prepare repository for implementation` |
| Done criteria | `.gitignore` includes local private input/output folders; README points to build plan; no application logic exists. |

Milestone notes:
- Add ignore entries for `local-input/`, `local-output/`, and any private scratch folders before real local files are used.
- Do not create `.NET` projects in this milestone unless Milestone 1 is being done in the same explicitly requested implementation task.

### Milestone 1 - .NET solution skeleton
| Field | Plan |
| --- | --- |
| Goal | Create the ASP.NET Core / .NET solution structure with no business logic. |
| Files/projects likely affected | `ExpenseFlow.sln`, `src/ExpenseFlow.Api`, `src/ExpenseFlow.Application`, `src/ExpenseFlow.Domain`, `src/ExpenseFlow.Infrastructure`, `tests/ExpenseFlow.UnitTests`, `tests/ExpenseFlow.IntegrationTests`. |
| Behavior delivered | Solution builds; API has a health endpoint; project references are wired. |
| Tests expected | Health endpoint integration test; solution build test via `dotnet build`; test runner works via `dotnet test`. |
| Acceptance tests covered | None of the product behavior yet. Establishes harness for AT-001 through AT-028. |
| Suggested commit message | `Create .NET solution skeleton` |
| Done criteria | `dotnet build` and `dotnet test` pass; no expense processing logic exists; project boundaries match `docs/backend-architecture.md`. |

Suggested references:
- `Api` references `Application`.
- `Application` references `Domain`.
- `Infrastructure` references `Application` and `Domain` if implementing application ports.
- `Api` references `Infrastructure` for dependency registration only.
- Tests reference the projects they verify.

### Milestone 2 - Synthetic fixtures
| Field | Plan |
| --- | --- |
| Goal | Create public synthetic CSV fixtures matching the dataset design. |
| Files/projects likely affected | A future synthetic fixture folder such as `testdata/` or `samples/`, unit/integration test fixture helpers. |
| Behavior delivered | Fixture files exist for repeatable tests and demos; no real data is committed. |
| Tests expected | Static/privacy review of fixture content; fixture row-count tests. |
| Acceptance tests covered | AT-001, AT-005, AT-017, AT-019, AT-026, AT-028 as test inputs. |
| Suggested commit message | `Add synthetic CSV fixtures` |
| Done criteria | Main, happy path, invalid rows, and mismatch scenario fixtures match `docs/demo-dataset-design.md`; no real personal financial data appears. |

Fixture set:
- `synthetic-april-main.csv`: all 22 rows.
- `synthetic-april-happy-path.csv`: rows 1 through 10 and 16 through 18.
- `synthetic-april-invalid-rows.csv`: rows 1, 20, 21, and 22.
- Mismatch can reuse the main fixture with expected total `260000.00`; a separate CSV is not required unless it improves test clarity.

### Milestone 3 - Domain model and enums
| Field | Plan |
| --- | --- |
| Goal | Define domain concepts needed by processing without infrastructure dependencies. |
| Files/projects likely affected | `src/ExpenseFlow.Domain`, `tests/ExpenseFlow.UnitTests`. |
| Behavior delivered | Row statuses, categories, validation statuses, rule result concepts, transaction/report concepts. |
| Tests expected | Unit tests for status/category values, row outcome invariants, and report count semantics. |
| Acceptance tests covered | Supports AT-004 through AT-024. |
| Suggested commit message | `Define expense processing domain model` |
| Done criteria | Domain project contains no ASP.NET Core, file system, CsvHelper, database, or AI provider dependency. |

Domain concepts to model:
- Source row reference.
- Raw source row values.
- Parsed/validated transaction candidate.
- Row status and flags.
- Category.
- Review reason.
- Validation error.
- Rule match/result.
- Report totals and validation status.

### Milestone 4 - CSV parsing and row validation
| Field | Plan |
| --- | --- |
| Goal | Parse supported CSV input and validate required columns/fields while preserving source row identity. |
| Files/projects likely affected | `ExpenseFlow.Application`, `ExpenseFlow.Infrastructure`, `ExpenseFlow.Domain`, unit tests, integration tests. |
| Behavior delivered | CSV text becomes raw rows; missing columns are file-level errors; invalid fields become invalid rows. |
| Tests expected | Parser tests, required column tests, invalid date/description/amount tests, optional column tests. |
| Acceptance tests covered | AT-001, AT-002, AT-003, AT-009, AT-010, AT-011, AT-027, AT-028 partial. |
| Suggested commit message | `Parse CSV input and validate rows` |
| Done criteria | Source row numbers and raw values are preserved; invalid rows are visible and excluded from totals later; no row is silently dropped. |

Important details:
- Preserve raw string values for invalid row reporting.
- Keep expected total separate from CSV parsing.
- Missing optional columns must not block processing.
- Unsupported input format should be represented as a structured file-level error.

### Milestone 5 - Deterministic categorization and review detection
| Field | Plan |
| --- | --- |
| Goal | Implement seed deterministic rules and review detection from the demo dataset. |
| Files/projects likely affected | `ExpenseFlow.Domain`, `ExpenseFlow.Application`, unit tests. |
| Behavior delivered | Known merchants categorize; unknown merchants review; conflicts review; refunds/transfers exclude; duplicates flag; installments preserved. |
| Tests expected | Unit tests for each seed rule, conflict tests, unknown fallback tests, refund/transfer tests, duplicate tests, installment tests. |
| Acceptance tests covered | AT-004, AT-007, AT-008, AT-012, AT-013, AT-014, AT-015, AT-016, AT-025 partial. |
| Suggested commit message | `Implement deterministic categorization rules` |
| Done criteria | Demo rows 1 through 19 receive expected category/review/exclusion/duplicate outcomes; no AI is used. |

Seed behavior:
- Implement rules R001 through R018 from `docs/demo-dataset-design.md`.
- Keep rule IDs or equivalent rule names visible for audit.
- Treat category conflicts as review-required.
- Keep row 19 included in processed/category totals while flagged as potential duplicate, matching the current dataset design.

### Milestone 6 - Totals and report generation
| Field | Plan |
| --- | --- |
| Goal | Produce the MVP report from parsed, validated, categorized, and reviewed rows. |
| Files/projects likely affected | `ExpenseFlow.Application`, `ExpenseFlow.Domain`, unit/application tests. |
| Behavior delivered | Processed total, category totals, expected total validation, processing counts, transaction details, review items, invalid rows, audit summary. |
| Tests expected | Application tests for main, happy path, invalid rows, expected total absent, and mismatch scenarios. |
| Acceptance tests covered | AT-005, AT-006, AT-017, AT-018, AT-019, AT-020, AT-021, AT-022, AT-023, AT-024, AT-028. |
| Suggested commit message | `Generate MVP expense reports` |
| Done criteria | Main dataset report matches expected counts/totals and includes all required report sections. |

Report expectations:
- Main processed total: `258248.00`.
- Category total sum: `189149.00`.
- Expected total status: `match` when expected total is `258248.00`.
- Mismatch status and difference when expected total is `260000.00`.
- `not_provided` when expected total is absent.

### Milestone 7 - API endpoint
| Field | Plan |
| --- | --- |
| Goal | Expose the first Minimal API endpoint for processing CSV text into a report. |
| Files/projects likely affected | `ExpenseFlow.Api`, `ExpenseFlow.Application`, `ExpenseFlow.Infrastructure`, integration tests. |
| Behavior delivered | API accepts JSON with `sourceName`, `csvText`, and optional `expectedTotal`; returns report or structured error. |
| Tests expected | API integration tests for success, missing columns, invalid rows, total mismatch, unsupported format if represented in the request contract. |
| Acceptance tests covered | AT-001, AT-002, AT-003, AT-017, AT-019, AT-020, AT-021, AT-024, AT-027, AT-028. |
| Suggested commit message | `Expose expense report processing endpoint` |
| Done criteria | Endpoint is thin; business behavior remains in application/domain; integration tests pass through HTTP. |

Recommended endpoint:
- `POST /api/expense-reports/process`

Recommended request style:
- JSON body with raw CSV text.

Recommended response style:
- `200 OK` with report for valid processable CSV input, even when some rows are invalid/review-required.
- `400 Bad Request` with structured file-level error for missing required columns, empty input, unsupported format, or malformed request.

### Milestone 8 - Integration tests and release gate
| Field | Plan |
| --- | --- |
| Goal | Complete P0 acceptance coverage and verify the first vertical slice as a release candidate. |
| Files/projects likely affected | `tests/ExpenseFlow.IntegrationTests`, `tests/ExpenseFlow.UnitTests`, fixture helpers, README test instructions. |
| Behavior delivered | Full dataset runs through API/application; release gate is enforced by tests. |
| Tests expected | P0 acceptance-mapped tests for AT-001 through AT-028 where applicable. |
| Acceptance tests covered | All P0 tests: AT-001 through AT-028. |
| Suggested commit message | `Cover MVP acceptance release gate` |
| Done criteria | `dotnet test` passes; no silent row dropping; no AI; no real data; main demo report matches expected output. |

Release gate emphasis:
- Full source row accounting.
- Deterministic totals.
- Required report sections.
- Invalid row visibility.
- Review item visibility.
- Synthetic data safety.

### Milestone 9 - Portfolio polish
| Field | Plan |
| --- | --- |
| Goal | Make the MVP easy to run, explain, and review as a portfolio project. |
| Files/projects likely affected | `README.md`, `docs/demo-story.md`, possible `docs/demo-script.md`, API examples, architecture summary. |
| Behavior delivered | Clear run instructions, demo command/request example, expected output summary, next-step roadmap. |
| Tests expected | Documentation examples should match passing tests; optionally smoke-test documented commands. |
| Acceptance tests covered | Supports public demonstration of AT-001 through AT-028. |
| Suggested commit message | `Document MVP demo workflow` |
| Done criteria | A reviewer can run tests, run the API, submit the synthetic demo input, and understand the output without reading every internal doc. |

Polish should not add new product scope.

## 6. Test strategy by milestone
| Milestone | Test focus | Acceptance tests |
| --- | --- | --- |
| 0 | Repository readiness and ignore rules. | Supports AT-026 |
| 1 | Build, health endpoint, test harness. | Infrastructure for all tests |
| 2 | Fixture row counts and synthetic safety. | AT-001, AT-005, AT-017, AT-019, AT-026, AT-028 |
| 3 | Domain concepts and invariants. | Supports AT-004 through AT-024 |
| 4 | CSV parsing, required columns, invalid rows. | AT-001, AT-002, AT-003, AT-009, AT-010, AT-011, AT-027, AT-028 |
| 5 | Categorization, review, exclusions, duplicates, installments. | AT-004, AT-007, AT-008, AT-012, AT-013, AT-014, AT-015, AT-016, AT-025 |
| 6 | Report sections, counts, totals, validation, audit. | AT-005, AT-006, AT-017, AT-018, AT-019, AT-020, AT-021, AT-022, AT-023, AT-024, AT-028 |
| 7 | API request/response behavior. | AT-001, AT-002, AT-003, AT-017, AT-019, AT-020, AT-021, AT-024, AT-027, AT-028 |
| 8 | Full P0 release gate. | AT-001 through AT-028 |
| 9 | Demo instructions and portfolio verification. | Supports all visible demo acceptance criteria |

Testing order:
1. Keep fast domain/application unit tests close to rule and validation logic.
2. Add application workflow tests before API endpoint tests.
3. Use API integration tests to prove wiring, request/response shape, and release-gate behavior.
4. Do not rely only on controller/API tests for financial rules.

## 7. Definition of done for MVP
The MVP is complete when:
- The API can process the main 22-row synthetic CSV fixture through the first endpoint.
- All source rows are accounted for.
- Required column validation works.
- Row validation works for missing description, invalid date, and invalid amount.
- Deterministic categorization matches the seed rules.
- Unknown merchants and ambiguous rows require review.
- Refund-like and transfer-like rows are visible and excluded from processed/category totals.
- Duplicate-looking row 19 is flagged and not removed.
- Installment markers are preserved.
- Processed total is `258248.00` for the main dataset.
- Trusted category total sum is `189149.00`.
- Expected total validation supports `match`, `mismatch`, and `not_provided`.
- All MVP report sections are returned.
- Audit summary explains applied rules, review reasons, invalid rows, exclusions, and total validation.
- No AI is called or required.
- No database is required.
- No real financial data is committed.
- `dotnet build` and `dotnet test` pass.
- P0 acceptance tests in `docs/acceptance-tests.md` pass or are explicitly mapped to passing automated tests.
- README explains how to run tests and the demo.

## 8. What not to build yet
Do not build yet:
- AI integration.
- Database persistence.
- Authentication or authorization.
- Frontend.
- PDF parsing.
- Excel parsing.
- Manual correction workflow.
- Rule management UI.
- Background jobs.
- Docker or cloud deployment unless later justified.
- Multi-user accounts.
- Bank integrations.
- Budgeting, forecasting, alerts, or financial advice.
- Generic CRUD expense record management.
- Provider-specific AI abstractions.
- Complex plugin architecture for parsers or exporters.

## 9. Risk controls
| Risk | Mitigation |
| --- | --- |
| Overengineering | Follow the milestones in order; do not add persistence, auth, CQRS frameworks, background jobs, or Docker in the first slice. |
| Too much documentation before code | After this build plan, move to Milestone 0 or Milestone 1 instead of adding more conceptual docs unless a blocker appears. |
| Fixture mismatch with docs | Generate fixtures directly from `docs/demo-dataset-design.md`; add tests that verify row counts and expected summary values. |
| Totals mismatch | Test totals with the main, happy path, mismatch, absent expected total, and invalid-row scenarios. Use decimal values, not floating-point types. |
| AI scope creep | Keep AT-025 visible in the release gate; do not add AI packages, providers, prompts, or keys in MVP milestones. |
| Parser complexity | Use CsvHelper behind `ITransactionFileParser`; keep the first input contract narrow and exact. |
| Weak demo story | Keep Milestone 9 tied to `docs/demo-story.md`; show row accounting, review items, invalid rows, totals, and audit details. |
| Tests not matching acceptance criteria | Maintain the milestone-to-acceptance-test mapping; require every P0 acceptance behavior to have automated coverage by Milestone 8. |
| Real data exposure | Add ignored local folders before real local testing; commit only synthetic fixtures; review fixture content before commit. |
| Logic leaking into HTTP endpoint | Keep endpoints thin and verify domain/application logic with tests that do not start ASP.NET Core. |

## 10. Recommended first coding prompt
Use this exact prompt for the first implementation step:

```text
Before coding, read AGENTS.md, README.md, docs/backend-architecture.md, docs/build-plan.md, docs/acceptance-tests.md, docs/input-output-contract.md, docs/demo-dataset-design.md, and docs/demo-story.md as the source of truth.

Implement Milestone 0 and Milestone 1 only from docs/build-plan.md.

Do not implement expense processing logic yet.
Do not create synthetic CSV fixtures yet.
Do not add AI, database persistence, authentication, frontend, PDF parsing, Excel parsing, Docker, or cloud deployment.

Tasks:
1. Add repository ignore rules for future local private input/output folders.
2. Create the .NET 10 solution skeleton with:
   - src/ExpenseFlow.Api
   - src/ExpenseFlow.Application
   - src/ExpenseFlow.Domain
   - src/ExpenseFlow.Infrastructure
   - tests/ExpenseFlow.UnitTests
   - tests/ExpenseFlow.IntegrationTests
3. Wire project references according to docs/backend-architecture.md.
4. Add a simple health endpoint in the API.
5. Add a minimal integration test for the health endpoint.
6. Ensure dotnet build and dotnet test pass.
7. Update README only with accurate local build/test instructions.

Keep changes small and reviewable. Do not write business logic yet.
```
