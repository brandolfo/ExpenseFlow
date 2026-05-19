# ExpenseFlow Agent Instructions

## Project summary
ExpenseFlow is a backend-focused product for processing expense files, categorizing transactions, validating totals, and generating structured reports. The project is both a useful personal tool and a portfolio-grade backend project.

## Product principle
Do not build a generic CRUD app. Every decision should support the core product goal: turning messy financial expense files into structured, categorized, validated reports.

## Collaboration model
Codex acts as the coding and project assistant. It can use the role definitions in /agents when asked to reason from a specific perspective.

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
- The next intended direction is PDF statement ingestion, but PDF parsing must be scoped before implementation.
- Treat PDF statements as untrusted and sensitive input.
- Use only synthetic or carefully redacted statement samples in committed files.
- Preserve document traceability: page number, extracted text/span evidence, source filename, and extraction warnings where available.
- PDF extraction should normalize transactions into the existing deterministic processing pipeline instead of creating a separate financial logic path.
- Do not add OCR, external APIs, LLM integration, persistence, auth, frontend, Docker/cloud, or manual correction workflow unless explicitly scoped and documented.

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
- Use Product Manager Agent to define PDF phase scope and non-goals before implementation.
- Use Document Extraction Agent and Data Engineer Agent for PDF statement structure, extraction assumptions, normalization, and source traceability.
- Use Domain Expert Agent to decide transaction semantics after extraction, not document parsing mechanics.
- Use Backend Architect Agent to keep PDF ingestion behind replaceable application/infrastructure boundaries.
- Use QA Agent to define fixture, extraction, normalization, and no-silent-row-loss tests.
- Use Security Agent before any real statement, external API, local file storage, or AI provider is introduced.
- Use AI Architect Agent only after deterministic PDF extraction and review boundaries are clear.
- Use Technical Writer Agent to keep docs aligned with implemented behavior and future plans.
- Avoid multi-agent review for mechanical edits, formatting, dependency bumps, or straightforward test fixes.

## Documentation rules
Every major decision should be recorded in /docs/decisions.md with:
- Date
- Decision
- Context
- Alternatives considered
- Consequences

## Current phase
Current phase: MVP v0.1 complete. Next phase can begin with PDF statement ingestion scope, extraction design, privacy rules, and test planning.
Do not implement PDF support, OCR, external APIs, LLM integration, persistence, auth, frontend, Docker/cloud, or new product behavior until explicitly scoped and requested.
