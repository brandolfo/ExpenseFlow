# Backend Architecture

## 1. Architecture goals
ExpenseFlow's MVP backend architecture should support the first vertical slice from `docs/demo-story.md`: process the 22-row synthetic mixed-behavior CSV-shaped dataset with expected total `258248.00` and produce a categorized, validated, auditable expense report.

Product goals:
- Transform one supported CSV transaction file into a structured report.
- Account for every source row.
- Categorize known merchants and stable patterns deterministically.
- Surface unknown, conflicting, invalid, duplicate-looking, refund-like, and transfer-like rows for visible review or invalid reporting.
- Calculate processed totals and category totals deterministically.
- Validate processed total against an optional expected total.
- Preserve auditability for row outcomes, rule matches, review reasons, invalid rows, and total validation.
- Keep AI out of the deterministic MVP workflow.

Portfolio goals:
- Demonstrate strong ASP.NET Core / .NET backend engineering.
- Show product-driven architecture rather than generic CRUD.
- Keep domain rules testable outside HTTP endpoints.
- Keep parsing, normalization, validation, categorization, reporting, and audit responsibilities separated.
- Use a buildable structure for one developer.
- Create useful interview discussion points around contracts, edge cases, validation, deterministic rules, auditability, testing, and future AI integration.

## 2. Non-goals
The MVP architecture explicitly does not include:
- Microservices.
- Frontend.
- Authentication for the first slice.
- Real OpenAI integration in the deterministic MVP.
- AI-generated totals, AI validation, or AI categorization in the MVP.
- PDF parsing.
- Excel parsing in the first slice. Excel import can be added later behind a file parser boundary.
- Bank integrations or account sync.
- Persistent multi-user accounts.
- Complex infrastructure.
- Distributed messaging, background workers, queues, or event sourcing.
- Database-first design.
- Manual correction workflow.
- Production deployment design.

## 3. Proposed architecture style
Recommendation: use a modular monolith with a pragmatic hybrid of Clean Architecture and vertical slice organization.

Why modular monolith:
- The MVP is one product workflow, not a distributed system.
- The core value is deterministic processing, not service orchestration.
- One developer can build, test, and explain it.
- It supports future growth without prematurely adding network boundaries.

How to use Clean Architecture:
- Keep `Domain` independent from ASP.NET Core, infrastructure, file systems, AI providers, databases, and serialization concerns.
- Keep application workflow orchestration in `Application`.
- Keep CSV parsing and future external integrations in `Infrastructure`.
- Keep HTTP concerns in `Api`.

How to use vertical slices:
- Organize application behavior around use cases such as `ProcessExpenseFile`.
- Let each use case own its request, response, validation orchestration, and report-generation flow.
- Avoid building broad generic service layers that do not serve the first workflow.

Why not traditional layered architecture only:
- A traditional layered design can work, but it often pushes domain behavior into services with vague boundaries.
- ExpenseFlow needs explicit boundaries around parsing, validation, deterministic rules, review detection, totals, reporting, and auditability.

Why not full Clean Architecture ceremony:
- The MVP should avoid excessive abstractions, CQRS frameworks, mediator libraries, and repository patterns before persistence exists.
- The goal is clean boundaries, not architecture theater.

## 4. Target .NET stack
Recommended MVP stack:
- ASP.NET Core backend.
- .NET 10 LTS as the primary target.
- Minimal APIs for the first slice.
- Built-in dependency injection, logging, configuration, and OpenAPI support where useful.
- xUnit or NUnit for tests; prefer xUnit if no team preference exists.
- `Microsoft.AspNetCore.Mvc.Testing` or equivalent ASP.NET Core test host for integration tests.
- Add a CSV parsing library only if implementation needs robust CSV behavior beyond simple demo rows. CsvHelper is a reasonable single-purpose option; avoid hand-rolled CSV parsing when quoted values, delimiters, or escaping matter.

.NET version note:
- As of May 18, 2026, Microsoft's official .NET support policy lists .NET 10 as LTS with support ending November 14, 2028. Source: [Microsoft .NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy).
- If a local machine cannot use .NET 10 immediately, .NET 8 LTS can be a temporary compatibility fallback, but the portfolio target should remain .NET 10 LTS.

API style recommendation:
- Use Minimal APIs for the first slice because the API surface is small and the important behavior belongs in application/domain code.
- Keep endpoints thin: bind request data, call the application use case, map result to HTTP response.
- If the API grows into multiple resource areas, controllers can be introduced later without changing the core processing model.

Local development approach:
- Run and test locally with the .NET SDK.
- No Docker required for the first slice.
- No database required for the first slice.
- Use appsettings only for non-secret local configuration.
- Use synthetic local inputs and outputs; keep real local data out of the repository.

## 5. Module boundaries
First vertical slice modules:
- API boundary.
- File ingestion / CSV parsing.
- Transaction normalization.
- Validation.
- Deterministic categorization.
- Review item detection.
- Audit trail.
- Reporting/result generation.
- Future AI integration boundary.
- Future export/reporting boundary.

These are logical modules. They do not all need separate projects. The recommended project structure groups them into `Api`, `Application`, `Domain`, and `Infrastructure`.

## 6. Responsibilities per module
### API boundary
Owns:
- HTTP request and response shape.
- Upload or request binding for the supported CSV input.
- Accepting expected total as separate request data.
- Returning the structured MVP report or file-level error.
- OpenAPI metadata for portfolio readability.

Must not own:
- CSV parsing rules.
- Financial totals.
- Categorization rules.
- Domain validation.
- Audit decisions.

### File ingestion / CSV parsing
Owns:
- Reading CSV content.
- Validating presence of required columns before row processing.
- Preserving source row numbers.
- Producing raw row representations for downstream validation.
- Reporting file-level failures such as unsupported format, empty file, or missing required columns.

Must not own:
- Business categorization.
- Category totals.
- Expected total validation.
- AI suggestions.

### Transaction normalization
Owns:
- Trimming and normalizing descriptions for matching.
- Preserving raw input values for audit output.
- Converting valid date and amount values into typed values.
- Keeping invalid raw values available for invalid row reporting.
- Normalizing optional `source_type`, `installment`, and `code` values.

Must not own:
- Deciding final category.
- Dropping invalid rows.
- Hiding source values needed for auditability.

### Validation
Owns:
- Required column validation.
- Required field validation for `date`, `description`, and `amount`.
- MVP date format validation.
- MVP decimal amount validation.
- Invalid row creation.
- Completeness checks that every source row has a visible outcome.

Must not own:
- Merchant category choice.
- AI suggestions.
- Silent correction of malformed input.

### Deterministic categorization
Owns:
- Applying known merchant rules.
- Applying stable description keyword rules.
- Applying source marker rules when safe.
- Recording matched rule IDs or rule names.
- Detecting category conflicts.

Must not own:
- Expected total validation.
- Manual correction workflow.
- AI provider calls.
- Guessing when rules are insufficient.

### Review item detection
Owns:
- Creating review items for unknown merchants.
- Creating review items for category conflicts.
- Creating review items for duplicate-looking rows.
- Creating review/exclusion outcomes for refund-like and transfer-like rows.
- Preserving review reasons from deterministic processing.

Must not own:
- Manual correction.
- AI final decisions.
- Removing duplicate-looking rows.

### Audit trail
Owns:
- Recording file metadata relevant to processing.
- Recording source row counts and processing counts.
- Recording applied deterministic rules.
- Recording invalid-row reasons.
- Recording review-required decisions.
- Recording exclusion reasons.
- Recording total validation status.
- Providing enough detail to explain report results.

Must not own:
- Business rule decisions themselves.
- Long-term persistence in the first slice.
- Sensitive data expansion.

### Reporting/result generation
Owns:
- Building the MVP report shape.
- Producing report metadata.
- Producing processing counts.
- Producing totals and validation result.
- Producing category summary.
- Producing transaction details, review items, invalid rows, and audit summary.

Must not own:
- Parsing raw files.
- Applying deterministic rules.
- Calling AI.

### Future AI integration boundary
Owns later:
- Sending only eligible review-required transactions to an AI assistant.
- Validating structured AI output.
- Recording AI suggestion audit details.
- Keeping provider details replaceable.

Must not own in the MVP:
- Any deterministic categorization.
- Totals.
- Expected total validation.
- Required field validation.
- Final decisions without review.

## 7. First vertical slice data flow
1. API receives a CSV input and optional expected total.
2. API sends the request to the application use case, such as `ProcessExpenseFile`.
3. File ingestion validates the file is supported CSV-shaped input and contains required columns.
4. File ingestion creates raw source rows with row numbers and raw field values.
5. Validation and normalization evaluate each source row.
6. Invalid rows are retained as invalid report items and excluded from totals.
7. Valid rows move through deterministic categorization and review detection.
8. Categorized rows receive trusted categories and matched rule audit entries.
9. Unknown, conflicting, duplicate-looking, refund-like, and transfer-like rows receive visible review or exclusion outcomes.
10. Totals are calculated from rows eligible for processed totals.
11. Category totals are calculated from rows eligible for trusted category totals.
12. Expected total validation runs if expected total was provided.
13. Audit summary is assembled from processing events and decisions.
14. Reporting/result generation returns the full MVP report to the API.
15. API returns the report as structured JSON for the first slice.

## 8. Processing flow
1. Start a processing run with source name, input format, and expected total status.
2. Reject unsupported formats before row processing.
3. Reject or clearly report empty files.
4. Validate required columns.
5. Parse rows without silently skipping any source row.
6. Preserve raw values and source row numbers.
7. Validate each row's required fields.
8. Convert valid fields to typed values.
9. Mark missing description, invalid date, or invalid amount rows as `invalid`.
10. Apply deterministic rule processing to valid rows.
11. Apply review and exclusion treatment rules.
12. Calculate processing counts.
13. Calculate processed total and category totals.
14. Validate expected total status as `match`, `mismatch`, `not_provided`, or `not_applicable`.
15. Generate the report.
16. Verify all source rows are visible in the report output.

## 9. Categorization flow
Deterministic rules run first and are the only categorization mechanism in the MVP.

Recommended rule order:
1. Required field validation.
2. Exclusion detectors for refund-like and transfer/payment-like rows.
3. Duplicate-looking row detection as a flag.
4. Known merchant rules.
5. Description keyword rules.
6. Source marker rules.
7. Category conflict detection.
8. Review fallback for no safe match.

Conflict behavior:
- If two or more category rules match different categories, the transaction becomes `review_required`.
- Conflicting rule IDs or rule names are recorded in audit details.
- The row remains included in processed total unless another rule excludes it.
- The row does not contribute to trusted category totals.

Unknown merchant behavior:
- A valid row with no safe deterministic match becomes `review_required`.
- The row remains included in processed total unless excluded by another treatment rule.
- The row does not contribute to trusted category totals.
- Audit details record a no-safe-match reason.

Future AI boundary:
- AI may later receive only transactions already marked `review_required`.
- AI suggestions must be structured, confidence-aware, and review-first.
- Known deterministic rules remain higher priority than AI.
- AI failure must not block deterministic report generation.
- AI must not calculate totals or validate correctness.

Review-required behavior:
- `review_required` is a visible outcome, not a failure.
- Manual correction is deferred to a later phase.
- Reports should make review reason and suggested next action visible.

## 10. Validation flow
Required column validation:
- The file must contain exact MVP column names `date`, `description`, and `amount`.
- Missing optional columns must not block processing.
- Missing required columns produce a file-level validation result and no fake trusted report.

Invalid row handling:
- Missing or invalid `date`, `description`, or `amount` creates an `invalid` row.
- Invalid rows remain visible in the invalid rows section.
- Invalid rows are excluded from processed totals and category totals.
- Invalid rows retain raw values and validation errors.

Expected total behavior:
- Expected total is supplied separately from the CSV.
- If present, compare it deterministically with processed total.
- If absent, report `not_provided` and still produce a useful report.
- If there are no processable valid rows, report `not_applicable` for total validation.
- Mismatch does not hide report details.

Processed total calculation:
- Include rows marked eligible for processed total.
- Exclude refund-like rows under the MVP default.
- Exclude transfer/payment-like rows under the MVP default.
- Exclude invalid rows.
- Include review-required rows unless a treatment rule excludes them.
- Include potential duplicate rows unless another treatment rule excludes them.

No silent row dropping:
- Every source row must appear in transaction details, review items, invalid rows, or another explicit documented outcome.
- Completeness validation should fail visibly if row accounting breaks.

## 11. Auditability strategy
The MVP should record enough audit information to explain the report without requiring a database in the first slice.

Auditable items:
- Input file metadata: source name, input format, generated-at timestamp, synthetic-data flag when known.
- Row count: source rows, valid rows, categorized rows, review rows, invalid rows, excluded-from-totals rows, potential duplicates.
- Invalid rows: source row number, raw values, validation errors.
- Applied rules: rule ID or rule name, source row number, match type, resulting category or treatment.
- Review-required decisions: source row number, reason, matched conflict rules where applicable.
- Exclusion decisions: refund-like, transfer-like, invalid, or other documented treatment.
- Total validation: expected total status, processed total, category total, expected total if provided, mismatch difference when applicable.
- Processing summary: warnings, counts, and completeness validation.

First slice storage:
- In-memory audit records are enough for one processing run.
- Return audit details in the report response.
- Persistent audit history can be added later with storage decisions.

## 12. Error handling strategy
Invalid files:
- Return a structured file-level validation error.
- Include reason, such as unsupported format or empty file.
- Do not throw raw exceptions to users.

Missing columns:
- Return a structured validation error listing missing required columns.
- Do not attempt partial trusted processing when required columns are absent.

Malformed amounts:
- Treat affected rows as invalid.
- Preserve raw amount text.
- Exclude affected rows from totals.
- Continue processing other rows.

Empty files:
- Return a clear invalid input result.
- Expected total validation should be `not_applicable`.
- No rows should be invented.

Unexpected exceptions:
- Log with correlation or processing run identifier.
- Return a generic error response that does not expose stack traces or local paths.
- Avoid losing already collected audit context where safe.

Partial processing failures:
- Prefer row-level invalid/reportable outcomes when the file shape is usable.
- Use file-level failure only when the input cannot be meaningfully processed at all.
- Never silently skip rows because one row failed.

## 13. Testing strategy
Testing should map directly to `docs/acceptance-tests.md`.

Unit tests:
- Required field validators: AT-009, AT-010, AT-011.
- Deterministic categorization rules: AT-004, AT-007, AT-008, AT-016.
- Refund and transfer treatment: AT-012, AT-013.
- Duplicate-looking row detection: AT-015.
- Installment handling: AT-014.
- Total calculation rules: AT-017, AT-018, AT-020.

Application/service tests:
- Process the happy path dataset: AT-005, AT-006.
- Process the main mixed-behavior dataset: AT-017, AT-018, AT-021, AT-022, AT-023, AT-024.
- Process mismatch expected total: AT-019.
- Process invalid-row dataset: AT-028.
- Verify AI is not invoked in MVP flow: AT-025.

Integration tests:
- Supported CSV input request: AT-001.
- Missing required columns: AT-002.
- Missing optional columns: AT-003.
- Unsupported input formats: AT-027.
- Full report response shape: AT-021.

Possible contract tests:
- Request contract for CSV input plus separate expected total.
- Report contract for required output sections.
- Error contract for file-level validation failures.

Synthetic dataset tests:
- Use synthetic fixture data matching `docs/demo-dataset-design.md` once actual CSV fixtures are explicitly created.
- Keep demo data free of real financial information.
- Verify main summary values: row count `22`, valid count `19`, review count `6`, invalid count `3`, excluded count `2`, potential duplicate count `1`, processed total `258248.00`, category total sum `189149.00`.

## 14. Security/privacy considerations
- Do not commit real financial data.
- Do not commit secrets, API keys, tokens, credentials, card numbers, bank account numbers, addresses, tax IDs, or personal identifiers.
- Public fixtures and examples must use synthetic data only.
- Add local input/output folders to `.gitignore` when they are introduced.
- Treat uploaded files and transaction descriptions as untrusted input.
- Do not log full raw files by default.
- Avoid exposing local paths, stack traces, or sensitive row content in error responses.
- Keep AI disabled in the deterministic MVP.
- Future real-data workflows need explicit retention rules.
- Future AI workflows need data minimization, masking, prompt injection review, and provider/secret handling.

## 15. Local development considerations
Recommended local setup:
- Install the target .NET SDK.
- Run the API locally with `dotnet run` once source code exists.
- Run tests with `dotnet test` once test projects exist.
- Keep first slice infrastructure-free: no Docker, database, queues, or cloud dependencies.
- Use console logging for local development.
- Use OpenAPI/Swagger only if it helps inspect the API, not as a substitute for tests.
- Keep local real input files outside the repository.

Recommended local folders later:
- `samples/` or `testdata/` for synthetic public fixtures only, if explicitly added.
- `local-input/` for private local files, ignored by git.
- `local-output/` for generated local reports, ignored by git.

## 16. Future AI integration seam
AI should be added later behind an application-facing interface, not baked into core deterministic logic.

Recommended future boundary:
- `IExpenseCategorizationAgent` in `Application`.
- Provider implementation in `Infrastructure`.
- Structured request/response contracts that match `docs/ai-agent-design.md`.
- Feature flag or explicit workflow switch to keep AI optional.

Rules:
- Only review-required transactions are eligible for AI assistance.
- AI suggestions are suggestions, not trusted final categories.
- AI output must be validated before use.
- AI cannot override deterministic rules.
- AI cannot calculate totals.
- AI cannot validate expected totals.
- AI failure leaves deterministic report generation intact.
- AI audit records should include input fields sent, output received, confidence, validation status, and final human decision once review exists.

## 17. Future reporting/export seam
The first slice should return structured JSON matching the MVP report shape. Future exports should be adapters around the same report model.

Future boundary:
- `IReportExporter` can be introduced later if needed.
- Excel, CSV summary, PDF, or HTML exports should consume the already-built report result.
- Exporters must not recalculate totals independently.
- Exporters must not hide invalid rows or review items.
- Exporters must preserve auditability or link back to audit details.

Excel parsing is separate from Excel exporting:
- Excel input parsing is deferred and should later be added as another `ITransactionFileParser` implementation.
- Excel report export is a future reporting adapter and should not bloat the first vertical slice.

## 18. Overengineering risks
Do not build yet:
- Microservices.
- CQRS framework or mediator library unless a real complexity appears.
- Event sourcing.
- Background job processing.
- Database persistence.
- Authentication and authorization.
- Frontend dashboard.
- Generic CRUD expense management.
- Full rule management UI.
- Manual correction workflow.
- AI provider integration.
- PDF or Excel parsing.
- Cloud deployment.
- Docker-based local setup unless needed for CI or deployment later.
- Multiple input format abstractions beyond a simple parser boundary.

The architecture should make these future additions possible, but it should not pay their full cost now.

## 19. Recommended solution structure
Recommended structure:

```text
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

docs/
  ...
```

Project responsibilities:
- `ExpenseFlow.Api`: ASP.NET Core host, Minimal API endpoints, request/response mapping, OpenAPI metadata.
- `ExpenseFlow.Application`: use cases, orchestration, ports/interfaces, application result models, workflow-level validation.
- `ExpenseFlow.Domain`: domain concepts, value objects, categories, row statuses, rule result types, domain policies that have no infrastructure dependencies.
- `ExpenseFlow.Infrastructure`: CSV parser implementation, future file adapters, future AI provider adapters, future export adapters, system clock or file system abstractions when justified.
- `ExpenseFlow.UnitTests`: domain and application behavior tests that do not start ASP.NET Core.
- `ExpenseFlow.IntegrationTests`: API and full workflow tests using synthetic data and ASP.NET Core test host.

Challenge to this structure:
- A smaller single-project feature-folder structure could build the first demo faster.
- However, the four-project structure better demonstrates .NET backend boundaries for a portfolio project while remaining manageable for one developer.
- Do not add more projects unless a clear boundary needs it.

## 20. Key abstractions
Use only important abstractions that protect boundaries or testability.

Recommended first-slice abstractions:
- `IExpenseReportProcessor`: application use case for processing one expense file into a report.
- `ITransactionFileParser`: parses supported transaction file content into raw source rows.
- `IRowValidator`: validates required row fields and produces invalid row outcomes.
- `ITransactionNormalizer`: normalizes raw row values while preserving source data for audit.
- `ICategorizationRuleEngine`: applies deterministic categorization rules and reports matches/conflicts.
- `IReviewItemDetector`: identifies review-required outcomes beyond ordinary rule matches.
- `IReportValidator`: validates expected total and completeness.
- `IAuditRecorder`: records processing events for the report audit summary.

Future abstractions:
- `IExpenseCategorizationAgent`: future AI suggestions for review-required transactions.
- `IReportExporter`: future Excel, CSV summary, PDF, or HTML exports.
- `IManualCorrectionService`: future review/correction workflow.

Avoid for the first slice:
- Generic repositories without persistence.
- Unit of work.
- Event bus.
- Rule database.
- Workflow engine.
- Provider-specific AI interfaces.
- Deep inheritance hierarchies for rules.

## 21. Implementation readiness
The project is ready to move to a build-plan phase after this architecture document.

Ready because:
- MVP scope exists.
- Input/output contract exists.
- Demo dataset design exists.
- Acceptance tests exist.
- Demo story and first vertical slice exist.
- Backend stack and architecture style are now defined.

The build plan should still decide:
- The exact first API request and response shape.
- Whether the first runnable demo accepts multipart upload, raw text body, or a simple file path only for local development.
- When to create actual synthetic CSV fixture files.
- Whether to use CsvHelper immediately or start with a minimal parser constrained to the documented demo input.
- The exact test framework choice if not accepted from this recommendation.

These are build-plan details, not blockers to architecture readiness.
