# Decision Log

## Template

### YYYY-MM-DD - Decision title
- Decision:
- Context:
- Alternatives considered:
- Consequences:
- Status:

## Decisions

### Initial - Start with product discovery before coding
- Decision: The project will first define agents, product scope, assumptions, and risks before creating application code.
- Context: The project is intended to be both useful and portfolio-grade. Premature coding could lead to overengineering or a generic CRUD app.
- Alternatives considered: Start with .NET solution immediately.
- Consequences: Slower initial coding, but clearer product direction.
- Status: Accepted.

### 2026-05-18 - Define MVP as deterministic file-to-report workflow
- Decision: The MVP will focus on one backend-first workflow that transforms one structured transaction export into a categorized, validated, auditable expense report.
- Context: The discovery documents and agent review converged on a small product scope that demonstrates backend value without making implementation decisions. The MVP must stay useful for Santiago while avoiding scope creep, generic CRUD behavior, premature AI, and premature technical choices.
- Alternatives considered: Start with PDF parsing, add AI categorization immediately, build a broader expense management app, or begin with framework and database decisions.
- Consequences: The first version remains small enough to build and validate. PDF support, AI assistance, dashboards, user accounts, and broader finance features are deferred. The product must prove trust through deterministic rules, total validation, review flags, auditability, and synthetic demo data.
- Status: Accepted.

### 2026-05-18 - Define initial domain model and category taxonomy
- Decision: ExpenseFlow will model source rows, transactions, transaction states, categorization rules, review items, manual corrections, expense reports, validation, and audit trails as core domain concepts. The initial category taxonomy will stay small and review-oriented.
- Context: The MVP requires clear domain language before implementation. Domain Expert and QA review emphasized that every row must be accounted for, totals must be deterministic, known merchant rules must take priority over AI, and manual correction is part of the business domain.
- Alternatives considered: Leave the domain implicit until coding, start with a database-shaped model, start with a larger accounting taxonomy, or treat manual review as a later implementation concern.
- Consequences: The product has clearer boundaries for future architecture and testing without choosing architecture or storage. Some detailed behaviors, such as refund handling and installment grouping, remain documented as domain decisions to refine before implementation.
- Status: Accepted.

### 2026-05-18 - Define AI as review-first assistance for ambiguity
- Decision: AI will be introduced later as structured, auditable assistance for ambiguous interpretation, starting with suggestions for transactions already marked as review required by deterministic processing.
- Context: ExpenseFlow must demonstrate responsible AI use without allowing AI to replace financial logic. The MVP remains deterministic. AI Architect, Domain Expert, Security, and QA review emphasized that AI must not calculate totals, validate financial correctness, override known rules, or finalize low-confidence classifications without review.
- Alternatives considered: Use AI for all categorization, use AI to reconcile totals, allow AI to create categories automatically, or omit AI design until implementation.
- Consequences: Deterministic processing remains useful even when AI fails or is disabled. Future AI output must be structured, confidence-aware, auditable, privacy-conscious, and safe to reject. Known merchant rules take priority over AI suggestions.
- Status: Accepted.

### 2026-05-18 - Require MVP contracts and acceptance tests before architecture
- Decision: Backend architecture work will wait until the MVP input/output contract, synthetic demo dataset, and acceptance tests are defined.
- Context: The project audit found that the product direction is strong, but implementation readiness is limited by unresolved practical decisions: first input shape, report output shape, deterministic rule examples, demo data, and expected behavior for key edge cases.
- Alternatives considered: Begin backend architecture immediately, continue adding broad product documentation, or start implementation with unresolved contracts.
- Consequences: Architecture starts later, but with less guessing and lower rework risk. The next documentation work should narrow the MVP into concrete examples and testable behavior instead of expanding future scope.
- Status: Accepted.

### 2026-05-18 - Define MVP input/output contract
- Decision: The first supported input format will be CSV. Required columns are `date`, `description`, and `amount`. Optional columns are `code`, `installment`, `source_type`, and `notes`. Expected total is provided separately by the caller/user when processing starts, not inside the CSV. Excel and PDF are out of scope for the first implementation.
- Context: The project audit identified input and output ambiguity as the main blocker before backend architecture. Product Manager, Domain Expert, Data Engineer, QA, and Backend Architect review supported a concrete product contract that keeps the MVP small and testable.
- Alternatives considered: Support Excel immediately, include expected total inside the CSV, accept multiple localized input shapes in the MVP, or leave input/output details for architecture to decide.
- Consequences: Architecture and implementation can proceed later with fewer guesses. The MVP is narrower and easier to test. Localized date formats, comma decimal separators, Excel, PDF, manual correction workflow, and advanced import flexibility are deferred.
- Status: Accepted.

### 2026-05-18 - Define MVP row treatment and review visibility
- Decision: Manual review visibility is in the MVP, but manual correction workflow is deferred to Phase 2. Installments are processed as individual rows. Duplicate-looking rows are surfaced for review and not removed automatically. Negative/refund-like rows are visible and excluded from totals by default until refund policy is refined. Transfers/payments are visible, require review, and are not silently treated as ordinary spending.
- Context: The domain model and audit identified refunds, transfers, installments, duplicates, and review behavior as unresolved implementation blockers. The MVP needs deterministic, visible treatment rules without expanding into a full correction system.
- Alternatives considered: Automatically remove duplicates, group installments, include refunds in category totals by default, treat transfers as ordinary spending, or implement manual correction in the MVP.
- Consequences: The first workflow preserves auditability and avoids silent financial assumptions. Some report totals may require review context, but the behavior is safer and easier to explain. Phase 2 can add correction history and richer treatment rules.
- Status: Accepted.

### 2026-05-18 - Define synthetic MVP demo dataset design
- Decision: The MVP demo dataset will be synthetic CSV-shaped data using the input/output contract columns. The main mixed-behavior dataset will contain 22 synthetic rows covering categorized transactions, unknown merchants, invalid rows, refund-like negative amounts, transfer/payment-like rows, installment-like descriptions, duplicate-looking rows, total validation, category summaries, and auditability.
- Context: The project audit required a concrete demo dataset before backend architecture. Product Manager, Domain Expert, Data Engineer, QA, and Security review supported a synthetic dataset that proves MVP behavior without using real financial data.
- Alternatives considered: Use real personal exports locally, create only a happy path dataset, defer dataset design until implementation, or create actual CSV files immediately.
- Consequences: Architecture and future acceptance tests can rely on concrete examples and deterministic expected outcomes. Actual CSV files are still deferred until explicitly requested. The dataset remains safe for public portfolio use because all merchants, codes, and examples are synthetic.
- Status: Accepted.

### 2026-05-18 - Define MVP acceptance tests as architecture entry gate
- Decision: The deterministic MVP workflow now has executable-style acceptance tests covering the supported CSV contract, demo dataset behavior, row validation, deterministic categorization, review handling, totals, report completeness, auditability, no-AI scope, and synthetic data safety. Backend architecture is allowed only after the input/output contract, synthetic dataset design, and acceptance tests exist and remain aligned.
- Context: The project audit identified acceptance tests as the final practical blocker before architecture. QA, Product Manager, Domain Expert, and Backend Architect review all support grounding future implementation in testable behavior rather than premature technical design.
- Alternatives considered: Begin backend architecture without acceptance tests, write implementation tests before product-level acceptance tests, or keep acceptance criteria only as broad prose in the MVP scope.
- Consequences: Future architecture can start with a clearer release gate and less guessing. The tests do not choose framework, database, endpoint, module structure, or application code. Any architecture proposal must preserve the documented deterministic behavior and auditability requirements.
- Status: Accepted.

### 2026-05-18 - Define first portfolio demo story and vertical slice
- Decision: The first portfolio demo will process the 22-row synthetic mixed-behavior dataset with separately provided expected total `258248.00` and produce the MVP report showing row accounting, deterministic categorization, review items, invalid rows, excluded rows, totals, expected-total validation, and audit details.
- Context: The project audit identified the need for a concise demo narrative and success path before implementation. The input/output contract, demo dataset design, and acceptance tests now provide enough source material to define a concrete first vertical slice without choosing backend architecture.
- Alternatives considered: Lead with the happy path dataset only, build a broader product tour, start with a frontend/dashboard demo, or move directly into backend architecture without a demo story.
- Consequences: The project has a clearer portfolio story and first implementation target. The demo remains backend-focused and intentionally excludes frontend, persistence, AI, PDF/Excel support, actual fixture files, application code, test code, and architecture decisions until explicitly requested.
- Status: Accepted.

### 2026-05-18 - Define ASP.NET Core modular monolith backend stack
- Decision: ExpenseFlow will be designed as a portfolio-grade ASP.NET Core / .NET backend using .NET 10 LTS, a pragmatic modular monolith, Minimal APIs for the first slice, testable domain/application logic, and replaceable infrastructure boundaries. The recommended solution structure is `ExpenseFlow.Api`, `ExpenseFlow.Application`, `ExpenseFlow.Domain`, and `ExpenseFlow.Infrastructure`, with unit and integration test projects.
- Context: The MVP now has a concrete input/output contract, synthetic dataset design, acceptance tests, and demo story. Santiago's primary professional stack is .NET backend development, so the architecture should demonstrate .NET backend skill while remaining buildable by one developer.
- Alternatives considered: Continue without choosing a stack, use microservices, use a single unstructured API project, choose a frontend-first architecture, or introduce AI/provider infrastructure in the MVP.
- Consequences: Future implementation work should assume ASP.NET Core / .NET unless a new decision changes it. The MVP remains deterministic and excludes microservices, frontend, authentication, persistence, PDF/Excel parsing, real AI integration, and complex infrastructure. Architecture can now move into a build-plan phase without creating source code yet.
- Status: Accepted.

### 2026-05-18 - Define first ASP.NET Core vertical slice build plan
- Decision: The first implementation sequence will follow `docs/build-plan.md`. The first API contract will accept JSON containing raw CSV text plus optional expected total, the first parser implementation will use CsvHelper behind `ITransactionFileParser`, xUnit will be used for tests, synthetic CSV fixtures will be created in the first coding phase after the solution skeleton, and tests will begin with the skeleton/domain milestones rather than waiting until the API is complete.
- Context: The architecture document left several build-plan details open: request style, parser choice, fixture timing, and test order. Product, backend, domain, data, QA, DevOps, and technical writing review all favor an incremental, testable, one-developer path that proves the first vertical slice before expanding scope.
- Alternatives considered: Start with multipart upload, accept local file paths through the API, hand-roll CSV parsing, delay fixtures until late integration testing, delay tests until after the endpoint exists, or combine all implementation work into one large commit.
- Consequences: The first runnable MVP can be built in small commits while preserving auditability and deterministic behavior. Multipart upload, local file path workflows, Excel/PDF parsing, database persistence, authentication, frontend, AI integration, Docker, and cloud deployment remain deferred.
- Status: Accepted.

### 2026-05-18 - Separate backend implementation under backend directory
- Decision: The .NET solution, source projects, test projects, and future public synthetic backend fixtures will live under `backend/`, while product documents, agent definitions, and skills remain at the repository root.
- Context: After creating the initial .NET skeleton, the root repository mixed backend implementation files with the project operating system files. A dedicated backend directory makes ownership and commands clearer without changing product scope.
- Alternatives considered: Keep `ExpenseFlow.sln`, `src/`, and `tests/` at the repository root, or move product documentation under a separate documentation workspace instead.
- Consequences: Backend commands should be run from `backend/`. Documentation should reference `backend/src`, `backend/tests`, and `backend/testdata` for implementation paths. Product scope, architecture style, and MVP exclusions remain unchanged.
- Status: Accepted.

### 2026-05-18 - Implement CSV parsing behind application parser abstraction
- Decision: Milestone 4 uses CsvHelper inside `ExpenseFlow.Infrastructure` and exposes parsing through `ITransactionFileParser` in `ExpenseFlow.Application`. The domain remains independent from CsvHelper. Parsing validates required headers and row fields, preserves source row numbers and raw values, and does not perform categorization, totals, report generation, or AI behavior.
- Context: The first MVP input is CSV, and the parser needs reliable header handling, row validation, and audit-preserving raw values without leaking infrastructure dependencies into domain logic.
- Alternatives considered: Hand-roll CSV parsing in application/domain code, expose CsvHelper types through the application boundary, or combine parsing with categorization and reporting in one workflow step.
- Consequences: CSV-specific behavior stays replaceable and testable behind an application abstraction. Domain code remains infrastructure-independent. Later milestones can build deterministic categorization, totals, and report generation on top of parsed rows without changing parser responsibilities.
- Status: Accepted.

### 2026-05-18 - Implement seed deterministic categorization in Application
- Decision: Milestone 5 uses an application-facing `ICategorizationRuleEngine` with an in-code seed deterministic implementation for categorization and review rules R001 through R017 from the demo dataset, while R018 remains covered by parser row validation. The engine takes valid parsed transaction candidates, returns row-level transaction outcomes, records matched rule IDs and patterns, flags category conflicts and potential duplicates, and marks refund-like and transfer/payment-like rows as excluded from totals later. It does not calculate totals, generate reports, call AI, or use persistence.
- Context: The MVP needs deterministic categorization and review detection before totals and report generation can be implemented. The seed rules are part of the first vertical slice and should remain testable without HTTP, database, or infrastructure dependencies.
- Alternatives considered: Put seed rules in Infrastructure, make rules database-backed immediately, combine categorization with report generation, or defer rule IDs and audit evidence until the reporting milestone.
- Consequences: Categorization behavior is simple, deterministic, and covered by unit and fixture-backed tests. Rule storage can evolve later, while Milestone 6 can consume explicit transaction outcomes without changing parser behavior or adding AI.
- Status: Accepted.

### 2026-05-18 - Generate MVP reports with exact deterministic totals
- Decision: Milestone 6 uses an application-facing deterministic report generator that consumes parser invalid rows plus categorization outcomes, calculates processed totals and trusted category totals with exact decimal comparison, and produces the MVP report model with metadata, totals, expected-total validation, category summary, transaction details, review items, invalid rows, excluded rows, and audit summary. Missing expected total returns `not_provided`; provided expected total uses exact decimal equality; mismatches include an absolute difference. Refund-like and transfer/payment-like rows remain visible but excluded from processed and category totals. Review-required rows without a trusted category remain visible and included in processed totals when their categorization outcome allows it, but they do not inflate category totals.
- Context: The MVP needs totals and report generation after parsing and deterministic categorization, while preserving auditability and strict separation from API, persistence, and AI behavior.
- Alternatives considered: Calculate totals during parsing or categorization, introduce a hidden tolerance for decimal comparison, include review-required uncategorized rows in category totals, subtract refund rows from category totals, or wait for the API endpoint before creating a report model.
- Consequences: Totals are deterministic, auditable, and fixture-tested before HTTP exposure. Category summaries remain trusted by excluding uncategorized review rows. Future refund policy, manual correction workflow, API response mapping, persistence, exports, and AI assistance remain deferred.
- Status: Accepted.

### 2026-05-18 - Expose first processing API with raw CSV text JSON request
- Decision: Milestone 7 exposes `POST /api/expense-reports/process` as a Minimal API endpoint that accepts JSON containing `sourceName`, optional `expectedTotal`, and raw `csvText`. API DTOs live in `ExpenseFlow.Api`, while the deterministic workflow is orchestrated by an application service. The endpoint returns `200 OK` with a structured report when the CSV shape is processable, even if some rows are invalid or require review. It returns `400 Bad Request` for request/file-level failures such as missing request body, missing `sourceName`, empty `csvText`, missing required CSV headers, empty CSV, or malformed CSV. The endpoint does not persist files or reports, does not require authentication in the first slice, and does not call AI.
- Context: The build plan selected raw CSV text in JSON for the first API because it is easy to test, deterministic, and avoids multipart upload and local file path concerns while still exercising the real parser, categorizer, totals, report, and audit workflow.
- Alternatives considered: Multipart upload, local file path input, putting processing orchestration directly in the Minimal API handler, returning `400` for row-level invalid data, leaking domain objects directly as the API contract, adding persistence, or adding authentication before the first slice.
- Consequences: The first API is small, testable, and portfolio-readable. File-level failures are distinct from row-level invalid data. Endpoint DTOs can evolve independently from domain internals. Multipart upload, persistence, authentication, and production hardening remain future work.
- Status: Accepted.

### 2026-05-18 - Use fixture-backed integration tests as the MVP release gate
- Decision: Milestone 8 treats endpoint-level and application-level tests using the committed synthetic fixtures as the deterministic MVP release gate. The gate must prove restore/build/test success, full source-row accounting, fixture safety, deterministic totals, expected-total validation, visible invalid/review/excluded rows, and no AI, database, authentication, frontend, Docker, or cloud dependency.
- Context: The build plan and acceptance tests define AT-001 through AT-028 as the P0 release boundary. By Milestone 8, the processing workflow exists end-to-end, so release readiness should be enforced by executable tests rather than by prose alone.
- Alternatives considered: Keep acceptance coverage as manual review only, rely only on unit tests, rely only on API smoke tests, or postpone fixture-backed release validation until portfolio polish.
- Consequences: The MVP has a concrete, repeatable release gate. Future changes that drop rows, alter deterministic totals, weaken synthetic data safety, or add out-of-scope infrastructure should fail tests before release.
- Status: Accepted.

### 2026-05-19 - Treat README and demo docs as portfolio entrypoints
- Decision: Milestone 9 positions `README.md` as the main portfolio entrypoint and adds concise demo, API, interview, and architecture documents as supporting artifacts. The public story emphasizes deterministic backend processing, auditability, release-gate tests, and responsible future AI without claiming AI is used in the MVP.
- Context: The MVP implementation and release gate are complete, so recruiters, interviewers, and backend developers need a fast path to understand the product value, run the API, inspect examples, and discuss architecture trade-offs without reading every planning document.
- Alternatives considered: Leave the README as a milestone log only, rely on the long architecture document for portfolio review, create broad marketing copy without technical proof, or overemphasize future AI before the deterministic workflow is explained.
- Consequences: The project is easier to review and demo while keeping scope honest. Future documentation should keep the README practical, keep examples aligned with tests, and clearly separate implemented MVP behavior from roadmap ideas.
- Status: Accepted.
