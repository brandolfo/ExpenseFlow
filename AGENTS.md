# ExpenseFlow Agent Instructions

## Project summary
ExpenseFlow is a backend-focused product for processing expense files, categorizing transactions, validating totals, and generating structured reports. The project is both a useful personal tool and a portfolio-grade backend project.

## Product principle
Do not build a generic CRUD app. Every decision should support the core product goal: turning messy financial expense files into structured, categorized, validated reports.

## Collaboration model
Codex acts as the coding and project assistant. It can use the role definitions in /agents when asked to reason from a specific perspective.

Role agents are repo-local planning and review lenses only. They are not runtime application agents, and ExpenseFlow should not claim or implement a runtime multi-agent architecture unless a separate future decision explicitly accepts that scope.

Future Codex prompts should follow `docs/codex-prompting-standard.md`, especially for source-of-truth priority, decision authority, expected diff, forbidden diff, blocker protocol, self-review, and stop condition guidance.

Available role agents:
- Founder Agent
- Product Manager Agent
- UX Researcher Agent
- Domain Expert Agent
- Document Extraction Agent
- AI Architect Agent
- Backend Architect Agent
- Data Engineer Agent
- QA Agent
- Security Agent
- DevOps Agent
- Technical Writer Agent
- Marketing Agent

## Working rules
- Inspect the repository from disk before planning changes and treat committed docs/code as the source of truth.
- Keep changes small and reviewable.
- Do not start coding before product scope is clear.
- Do not make irreversible technical decisions without documenting the reason.
- Prefer simple, shippable versions over perfect architecture.
- Always distinguish MVP from future features.
- Always identify assumptions.
- Always identify risks.
- Always preserve auditability.
- Never silently ignore transactions.
- Never use real personal financial data in public files.
- Never commit secrets, API keys, tokens, credentials, or real bank/card data.
- Use synthetic data for demos.

## PDF statement phase rules
- MVP v0.1 is complete as a deterministic CSV expense-processing backend.
- Scoped synthetic PDF ingestion is complete for `icbc-visa-like-v1` and `icbc-mastercard-like-v1`.
- Future feature work must start from explicit scope and decision documentation.
- Treat PDF statements as untrusted and sensitive input.
- Use only synthetic or carefully redacted statement samples in committed files.
- Preserve document traceability: page number, extracted text/span evidence, source filename, and extraction warnings where available.
- PDF extraction should normalize transactions into the existing deterministic processing pipeline instead of creating a separate financial logic path.
- Arbitrary PDFs, OCR, external APIs, LLM integration, persistence, auth, frontend, Docker/cloud, real/private statement processing, or manual correction workflow remain future work unless explicitly scoped and documented.

## AI usage rules
- Do not use AI to calculate financial totals.
- Do not use AI for deterministic validation.
- Use deterministic rules for known merchants and known patterns.
- Use AI only for ambiguous classification, summaries, explanations, or recommendations.
- Every AI decision must be traceable.
- AI output must be structured.
- AI suggestions must support confidence and human review.
- Invalid AI output must fail safely.

## Product decision rules
Before adding a feature, answer:
1. Who benefits from this?
2. What pain does it solve?
3. Is it required for the first useful version?
4. Can it be validated with less effort?
5. Does it help Santiago demonstrate backend skill?

## Engineering decision rules
When implementation begins:
- Assume ASP.NET Core / .NET as the primary backend stack from the accepted backend architecture decision onward.
- Prefer a modular monolith unless there is a strong reason not to.
- Keep domain logic out of controllers.
- Keep parsing, categorization, validation, reporting, and audit logic separated.
- Favor testable application logic.
- Use tests for domain and application behavior.
- Keep infrastructure replaceable behind interfaces.
- Design for observability and auditability.

## Agent usage rules
- Use role agents only when the lens materially reduces product, architecture, QA, security, documentation, AI-boundary, or scope risk.
- Use Product Manager Agent and Founder Agent to define feature scope, non-goals, MVP-vs-future boundaries, and portfolio value before new product behavior is added.
- Use Backend Architect Agent to review module boundaries, endpoint shape, deterministic processing boundaries, infrastructure isolation, and overengineering risk.
- Use QA Agent to define release gates, regression coverage, fixture behavior, no-silent-row-loss checks, and failure modes.
- Use Security Agent before any real/private statement experiment, external API, local file storage, retention policy, AI provider, secrets handling, or deployment work.
- Use Technical Writer Agent to keep README, architecture docs, decision logs, API examples, and future prompts aligned with implemented behavior and `docs/codex-prompting-standard.md`.
- Use DevOps Agent for CI/release gate automation, reproducible local commands, health checks, environment handling, and observability planning; do not add Docker/cloud/deployment scope unless explicitly accepted.
- Use Document Extraction Agent and Data Engineer Agent for future PDF variants, extraction hardening, private local PDF experiments, normalization, source traceability, and data quality risks.
- Use Domain Expert Agent to decide transaction semantics after extraction or parsing, not document parsing mechanics.
- Use AI Architect Agent only for future AI-assisted review, structured outputs, guardrails, confidence behavior, provider boundaries, and failure handling; do not use it to add runtime agents or AI to deterministic totals/validation.
- Use UX Researcher Agent when user workflow, trust, review, or manual correction needs are unclear.
- Use Marketing Agent for portfolio, CV, LinkedIn, and interview positioning after implemented behavior is known.
- Align role-agent work with relevant repo-local skills when they fit the task; avoid skill or multi-agent ceremony for mechanical edits, formatting, dependency bumps, or straightforward test fixes.

## Skill usage rules
- Use Product Discovery Skill with Founder Agent or Product Manager Agent for new feature scope, non-goals, assumptions, and acceptance criteria.
- Use Backend Architecture Review Skill with Backend Architect Agent for module boundaries, endpoints, infrastructure isolation, maintainability, and testability.
- Use Test Case Generation Skill with QA Agent for release gates, edge cases, fixture coverage, failure modes, and regression risks.
- Use Expense Domain Analysis Skill with Domain Expert Agent for categorization, transaction semantics, refunds, duplicates, totals, and review behavior.
- Use PDF Statement Ingestion Skill with Document Extraction Agent and Data Engineer Agent for future PDF variants, traceability, privacy, extraction, and normalization work.
- Use Portfolio Positioning Skill with Technical Writer Agent or Marketing Agent for README, demos, interview explanations, and public project positioning.
- Use AI Agent Design Skill with AI Architect Agent only for future AI-assisted review/suggestion boundaries; do not use it to design runtime application agents.

## Documentation rules
Every major decision should be recorded in /docs/decisions.md with:
- Date
- Decision
- Context
- Alternatives considered
- Consequences

## Current phase
Current phase: deterministic CSV MVP complete, and scoped synthetic PDF ingestion complete for `icbc-visa-like-v1` and `icbc-mastercard-like-v1`.
Future feature work must start from explicit scope and decision docs. Do not implement arbitrary PDF support, OCR, external APIs, LLM integration, persistence, auth, frontend, Docker/cloud, real/private statement processing, or new product behavior until explicitly scoped and requested.
