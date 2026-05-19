# PDF Ingestion Plan

## 1. Purpose

ExpenseFlow MVP v0.1 is complete as a deterministic ASP.NET Core backend for CSV expense processing. The PDF phase expands supported source inputs while preserving the existing product principle: turn messy financial expense files into structured, categorized, validated, auditable reports.

This document is the main source of truth for the first PDF ingestion phase. It consolidates the former temporary PDF triage questions into a resolved first-phase implementation contract.

Role lenses used for this plan:

- Product Manager Agent: scope, non-goals, acceptance criteria, MVP versus future work.
- Document Extraction Agent: statement structure, section markers, traceability, extraction warnings.
- Data Engineer Agent: normalization, date/amount formats, row quality, synthetic fixtures.
- Backend Architect Agent: application boundary, pipeline reuse, infrastructure replacement.
- Domain Expert Agent: transaction semantics, signed amounts, expected-total behavior.
- QA Agent: fixture coverage, no silent row loss, unsupported-file behavior.
- Security Agent: privacy, public fixture rules, logging and evidence exposure.
- AI Architect Agent: LLM and runtime agent exclusions, future AI boundary.

Skills applied:

- PDF Statement Ingestion Skill
- Backend Architecture Review Skill
- Test Case Generation Skill
- Expense Domain Analysis Skill
- AI Agent Design Skill

These role files and skills are planning/review lenses only. They are not a runtime multi-agent architecture and must not be represented as application-level agents inside ExpenseFlow.

The PDF phase must preserve this data flow:

```text
PDF statement -> deterministic extraction -> normalized transaction rows -> existing ExpenseFlow deterministic processing pipeline
```

PDF extraction must not categorize transactions, calculate financial totals, validate expected totals, call AI, or bypass existing parsing/categorization/reporting boundaries.

## 2. Resolved PDF Phase Decisions

The first PDF phase targets a synthetic ICBC-like credit card statement family with two public synthetic variants:

- `icbc-visa-like-v1`
- `icbc-mastercard-like-v1`

These variants are structurally inspired by private reference observations, but committed fixtures and docs must use only synthetic public data. They must not copy real statement data, real merchants, real dates, real amounts, real card/account details, names, IDs, addresses, screenshots, extracted text, or PDF metadata.

First implementation order:

- The Visa-like variant can be implemented first.
- The first Visa-like extraction slice can be single-page.
- Mastercard-like support is part of the PDF phase.
- Mastercard-like multi-page support is required before the PDF phase is considered complete.

PDF characteristics:

- First-phase PDFs are text-selectable.
- Scanned/image-only PDFs fail as unsupported.
- OCR is out of scope.
- Encrypted/password-protected PDFs fail as unsupported unless a later decision changes that.
- Arbitrary bank/card PDFs are out of scope.

Financial validation:

- `expectedTotal` remains caller-provided.
- Extracted statement totals, if detected, are metadata/evidence only.
- Extracted totals must not become trusted validation input during the first PDF phase.
- Existing deterministic report logic continues to calculate processed totals and expected-total validation.

Pipeline boundary:

- CSV behavior must remain unchanged.
- PDF ingestion must feed the existing deterministic pipeline.
- PDF extraction must not create a separate financial logic path.
- Start with an internal application service before adding a public HTTP endpoint.
- Do not replace `ITransactionFileParser` during the first PDF implementation.
- Add a PDF-specific application boundary such as `IPdfStatementExtractor`.
- If a normalized row abstraction is needed, introduce the smallest possible application-level model.
- Do not perform a broad CSV pipeline refactor before it is necessary.

Later endpoint shape, when implemented:

```http
POST /api/expense-reports/process-pdf
Content-Type: application/json
```

Request fields:

- `sourceName`
- `expectedTotal`
- `pdfBase64`
- optional `statementShapeHint`

Endpoint hardening:

- The first public PDF endpoint will use a 5 MB maximum file size.
- The limit does not need to be enforced until endpoint work begins.
- Authentication, persistence, frontend, Docker/cloud, background jobs, and production hardening remain out of scope for the PDF phase unless separately accepted.

Library candidates:

- Use PdfPig as the first PDF text extraction library candidate for implementation.
- Reason: PdfPig is a .NET-compatible, open-source, Apache 2.0 licensed PDF reading/extraction library suitable for text-selectable PDFs.
- Do not attempt OCR or complex arbitrary table extraction in the first iteration.
- Use QuestPDF as the first candidate for generating synthetic PDF fixtures if code-generated fixtures are needed.
- QuestPDF is for synthetic fixture generation only, not extraction.

AI boundary:

- LLM extraction is out of scope.
- LLM analysis is out of scope.
- No runtime multi-agent architecture.
- No application-level AI agents.
- Future LLM work requires a separate decision.

## 3. PDF Phase Variants

### `icbc-visa-like-v1`

First implementation can start with this variant.

Supported assumptions:

- Text-selectable PDF.
- Single-page for the first extraction implementation.
- Transaction table appears on the first page.
- Later legal or informational content, if present, is outside the active transaction section.

Conceptual columns:

- `date`
- `code` / `comprobante`
- `description`
- `pesos`
- `dolares`

Date format:

- `dd.MM.yy`

Transaction section:

- Starts around the `FECHA` / `COMPROBANTE` / `DETALLE DE TRANSACCION` / `PESOS` / `DOLARES` header.
- Stops before summary/footer markers such as `Total Consumos`, `Impuesto de Sellos`, `Saldo Actual`, and `Pago Minimo`.
- Lines outside the active transaction section are not transaction rows.

Amount and sign behavior:

- Positive pesos amounts represent ordinary spending or fees.
- Trailing-minus amounts must be handled as negative-like values.
- Negative-like rows represent payment/refund/credit/adjustment-like rows and must remain visible.
- Dolares values should be preserved as foreign-currency evidence/metadata in the first phase unless a later decision accepts full multi-currency processing.

### `icbc-mastercard-like-v1`

This variant is part of the PDF phase but does not need to be the first extraction commit.

Supported assumptions:

- Text-selectable PDF.
- May be multi-page.
- Multi-page transaction extraction is required before PDF phase completion.
- The document may contain a consolidated summary before transaction detail.
- Later legal or informational content is outside the active transaction section.

Conceptual columns:

- `date`
- `description` / `detail`
- `nro cupon` / `code`
- `pesos`
- `dolares`

Date format:

- `dd-MMM-yy` with Spanish month abbreviations.

Transaction section:

- Starts at `DETALLE DEL MES`.
- Active transaction subsections are `Compras del Mes`, `Debitos Automaticos`, and `Cuotas del Mes`.
- Transaction rows may continue across pages.
- Stops at `TOTAL TITULAR`.
- Lines outside the active transaction section are not transaction rows.

Amount, installment, and sign behavior:

- Positive pesos amounts represent ordinary spending or fees.
- Negative amounts represent refund/payment/credit-like rows.
- Installment values can be extracted from patterns like `01/06` or `06/06` inside descriptions.
- Dolares values should be preserved as foreign-currency evidence/metadata in the first phase unless a later decision accepts full multi-currency processing.

### Source Type Hints

PDF extraction may derive deterministic `source_type` hints:

- `purchase`
- `payment`
- `refund`
- `fee`
- `adjustment`
- `automatic_debit`
- `installment`
- `unknown`

These hints must not override downstream deterministic validation, categorization, or reporting rules. When extraction cannot derive a hint safely, use `unknown`.

## 4. Extraction, Normalization, and Traceability

The extraction strategy should use deterministic section markers. Headers, repeated headers, page footers, statement summaries, legal text, and informational text must not become transactions.

Candidate transaction lines must not be silently ignored. Each transaction-like line in the active section must become one of:

- a normalized row,
- an invalid extracted row,
- or an extraction warning with traceability.

Normalized PDF rows should map to the existing conceptual CSV fields:

- `date`
- `description`
- `amount`
- optional `code`
- optional `installment`
- optional `source_type`
- optional `notes`

PDF-specific traceability should be preserved internally:

- source filename or caller-provided `sourceName`
- statement shape identifier
- page number
- extraction order
- field-level extracted values
- extraction warnings
- extraction method/library
- short evidence snippets where safe

For private PDFs:

- Do not return full raw extracted text by default.
- Do not log full raw extracted text.
- Do not commit extracted private text.
- API responses, when added later, should expose only safe evidence: page number, extraction order, field-level values, warnings, and short masked snippets if necessary.

Assumptions:

- First-phase extraction is deterministic and layout-specific.
- Text-selectable PDFs provide enough text order/position information for the accepted synthetic variants.
- Existing downstream processing can remain responsible for validation, categorization, totals, and report assembly.

Risks:

- PDF text order may differ from visual table order.
- Header/footer markers may appear in legal text or summaries.
- Multi-page continuation can duplicate or drop rows if not tested carefully.
- Sign conventions can corrupt totals if treated as categorization instead of source evidence.
- Raw extracted evidence can leak sensitive data if logged or exposed too broadly.

## 5. Privacy and Fixture Rules

Real/private PDFs:

- Must remain local/private.
- Must never be committed.
- Must never be pasted into docs, issues, tests, examples, prompts, screenshots, logs, or public artifacts.
- Must not be copied into public fixtures, even in anonymized form.
- Must not be sent to external APIs or LLM providers during this phase.

Forbidden committed artifacts:

- Real PDFs.
- Screenshots of real statements.
- Extracted text from real statements.
- Generated reports from real statements.
- Anonymized real statements.
- Real names, accounts, card numbers, transaction merchants, transaction dates, transaction amounts, IDs, addresses, emails, phone numbers, QR/barcode data, statement IDs, or hidden PDF metadata.

Public fixtures:

- Must be fully synthetic.
- May imitate structural patterns from the accepted variants.
- Must not use anonymized real statements.
- Must use synthetic names, merchants, dates, amounts, codes, card/account details, addresses, statement numbers, and metadata.
- Should include edge cases needed for tests, such as repeated headers, summary/footer markers, negative-like rows, automatic debits, installments, malformed candidates, and multi-page continuation.

Private work areas:

- Private PDF inputs, extraction outputs, and local experiments must live under ignored paths.
- Before private experimentation begins, verify ignore coverage with `git status`.
- If a weird duplicate path-like markdown artifact appears, such as a filename resembling an absolute `C:\Users\...\docs\pdf-open-questions.md` path, treat it as accidental, report it, and remove it from the working tree.

## 6. Execution Milestones

### PDF-0 - Privacy Guardrails

Goal: prepare the repository for safe local PDF work before parser code.

Expected changes:

- Verify or add ignored local folders for private PDF inputs and generated extraction outputs.
- Confirm no real PDFs, screenshots, extracted text, generated reports, or private data are in the working tree.

Non-goals:

- No PDF parsing.
- No PDF libraries.
- No fixtures.
- No OCR.
- No LLM.
- No backend source changes unless ignore/documentation guardrails require them.

### PDF-1 - Application Boundary

Goal: define the smallest application boundary for PDF extraction and normalization without breaking CSV behavior.

Expected direction:

- Add a PDF-specific boundary such as `IPdfStatementExtractor`.
- Return normalized rows/results compatible with the existing deterministic processing pipeline.
- Inspect whether a minimal application-level normalized row model is needed.
- Keep `ITransactionFileParser` as the CSV parser abstraction for the first PDF implementation.

Non-goals:

- No broad CSV refactor.
- No public endpoint.
- No categorization/reporting changes unless required for source-format-neutral input.

### PDF-2 - Synthetic Variant Fixtures

Goal: design and create public synthetic fixture coverage for the accepted variants.

Expected direction:

- Start with `icbc-visa-like-v1`.
- Add `icbc-mastercard-like-v1` before PDF phase completion.
- Use QuestPDF as the first fixture-generation candidate if code-generated PDFs are needed.
- Keep fixture data fully synthetic and public-safe.

Non-goals:

- No anonymized real statements.
- No arbitrary bank/card layouts.

### PDF-3 - Deterministic Text Extraction

Goal: extract active transaction candidates from supported synthetic, text-selectable PDFs.

Expected direction:

- Use PdfPig as the first extraction candidate.
- Detect the supported statement shape and active transaction section.
- Ignore headers, repeated headers, footers, summaries, and legal/informational text.
- Preserve page number, extraction order, and warnings.

Non-goals:

- No OCR.
- No LLM.
- No complex arbitrary table extraction.
- No financial totals or categorization inside extraction.

### PDF-4 - Normalization

Goal: convert extracted candidates into normalized transaction rows with source evidence.

Expected direction:

- Normalize date, description, signed amount, code, installment, source type, and notes where available.
- Preserve invalid or ambiguous candidates as warnings/invalid extracted rows.
- Handle Visa-like trailing-minus values.
- Handle Mastercard-like Spanish month abbreviations and installment patterns.
- Preserve dolares values as foreign-currency evidence/metadata.

Non-goals:

- No trusted statement-total extraction.
- No advanced multi-currency processing.
- No manual correction workflow.

### PDF-5 - Internal Service Integration

Goal: feed normalized PDF rows into the existing deterministic processing pipeline through an internal application service.

Expected direction:

- Run downstream validation, categorization, totals, expected-total validation, reporting, and audit behavior using existing deterministic logic.
- Prove CSV behavior remains unchanged.
- Keep extraction warnings visible alongside processable reports.

Non-goals:

- No public PDF endpoint yet unless extraction/normalization behavior is stable.
- No persistence, auth, frontend, background jobs, Docker/cloud, external APIs, or AI.

### PDF-6 - Endpoint Addition

Goal: add the first public PDF endpoint after internal behavior is stable.

Accepted preliminary shape:

- `POST /api/expense-reports/process-pdf`
- JSON request with `sourceName`, `expectedTotal`, `pdfBase64`, and optional `statementShapeHint`.
- 5 MB maximum file size.

Non-goals:

- No multipart upload unless separately accepted.
- No endpoint hardening beyond the accepted preliminary shape unless separately scoped.

### PDF-7 - PDF Phase Release Gate

Goal: complete PDF phase verification and docs.

Completion requirements:

- Visa-like synthetic fixture works end-to-end.
- Mastercard-like synthetic fixture works end-to-end.
- Mastercard-like multi-page support is covered.
- Unsupported/scanned behavior is explicit.
- No silent row loss is tested.
- CSV baseline remains unchanged.
- Docs accurately describe implemented behavior and future limits.

## 7. Acceptance Criteria for PDF Phase Completion

The PDF phase is complete only when all of the following are true:

- `icbc-visa-like-v1` synthetic text-selectable PDF is supported.
- `icbc-mastercard-like-v1` synthetic text-selectable PDF is supported.
- Mastercard-like multi-page transaction continuation is supported and tested.
- Public fixtures are fully synthetic.
- Real/private statements and extracted private text are not committed.
- OCR is not implemented and scanned/image-only PDFs fail as unsupported.
- LLM extraction and LLM analysis are not implemented.
- No runtime/application-level AI agent architecture is introduced.
- PdfPig is either used as the first extraction candidate or a later decision records why it was replaced.
- QuestPDF is either used for code-generated synthetic fixtures or a later decision records why it was unnecessary/replaced.
- `expectedTotal` remains caller-provided.
- Extracted statement totals, if detected, remain metadata/evidence only.
- PDF extraction feeds the existing deterministic pipeline.
- CSV behavior remains unchanged.
- Headers, footers, summaries, and legal/informational text do not become transactions.
- Ambiguous or malformed candidate lines become warnings or invalid extracted rows, never silent drops.
- Every normalized PDF row preserves source traceability at least by source name, statement shape, page number, and extraction order.
- Negative-like payment/refund/credit rows are preserved and passed downstream with source type hints.
- Dolares values are preserved as foreign-currency evidence/metadata unless a later decision accepts full multi-currency processing.
- The first public endpoint, if implemented in this phase, follows the accepted preliminary shape and 5 MB size limit.
- Tests cover supported fixtures, unsupported PDFs, scanned/image-only unsupported behavior, malformed rows, repeated headers/footers, multi-page continuation, sign handling, expected-total match/mismatch, and no silent row loss.

## 8. Remaining Deferred Decisions

The following are future-only and must not block or expand the first PDF phase:

- Future OCR support for scanned/image-only PDFs.
- Future LLM extraction or LLM analysis.
- Future arbitrary bank/card statement support.
- Future trusted statement-total extraction.
- Future real/private statement processing beyond local experimentation.
- Future runtime/application-level AI agent architecture.
- Future advanced multi-currency processing.
- Future endpoint hardening beyond the accepted preliminary JSON/base64 shape and 5 MB limit.
- Future multipart upload support.
- Future persistence, authentication, frontend/dashboard, background jobs, Docker/cloud deployment, and production infrastructure.

Any future item above requires a separate decision entry before implementation.
