# PDF Ingestion Plan

## 1. Purpose

ExpenseFlow MVP v0.1 is complete as a deterministic ASP.NET Core backend for CSV expense processing. The MVP proves the core workflow: receive structured transaction rows, validate them, categorize known patterns with deterministic rules, surface ambiguous or invalid rows, calculate totals in code, validate against an expected total, and return an auditable report.

PDF ingestion is the next phase because credit card statements often arrive as PDFs rather than clean CSV exports. This phase should expand the set of supported source inputs while keeping the existing processing model intact.

PDF ingestion is a source ingestion expansion, not a replacement for the deterministic pipeline. The first PDF workflow should extract statement transactions, normalize them into the same conceptual transaction input model used by the CSV MVP, and then feed those normalized rows into the existing parsing, validation, categorization, totals, reporting, and audit flow.

The important architectural rule is:

```text
PDF statement -> extraction -> normalized transaction rows -> existing ExpenseFlow processing pipeline
```

PDF extraction should not categorize transactions, calculate financial totals, validate expected totals, or bypass review behavior. It should produce traceable candidate rows that the existing deterministic workflow can process.

## 2. Product scope

The first PDF iteration should support one narrow source type:

- Digitally generated credit card statement PDFs.
- Text-selectable PDFs, not scanned or image-only documents.
- One known statement shape chosen before coding.
- One currency/amount format for the first slice.
- One transaction table layout with stable date, description, and amount fields.

The first supported statement shape should be a synthetic statement modeled after a realistic credit card statement, with a clear transaction table and optional statement metadata such as statement period and statement total. It should not try to cover arbitrary banks, cards, locales, page layouts, or scanned documents.

Successful extraction means:

- The PDF can be opened and read without a password.
- The expected transaction table region is found.
- Each visible transaction line in the supported table shape becomes one normalized transaction row.
- Required fields can be mapped to `date`, `description`, and `amount`.
- Optional fields can be mapped when present.
- Every extracted or suspicious candidate line is either normalized or preserved as an extraction warning/review candidate.
- The normalized rows can be passed into the existing ExpenseFlow processing workflow.

Partial extraction means:

- Some transaction rows are normalized, but one or more candidate lines have missing, malformed, split, duplicated, or ambiguous fields.
- Repeated headers, footers, page totals, or statement summary lines are detected but require warnings.
- Multi-page continuation works for some pages but produces warnings for page transitions.
- Statement total or metadata cannot be found, while transaction rows can still be extracted.

Partial extraction should not pretend to be fully trusted. The response should make extraction warnings visible before or alongside the normal report.

Rows or candidate lines should require review when:

- A line looks transaction-like but cannot be confidently split into date, description, and amount.
- A description is split across lines and cannot be joined deterministically.
- A sign convention is unclear, such as payments, refunds, credits, or reversals.
- The same candidate appears more than once after extraction.
- The transaction date is missing, malformed, or inconsistent with the statement period.
- The amount has unsupported formatting, currency symbols, or ambiguous debit/credit columns.

The workflow should fail fast when:

- The PDF is encrypted or password protected.
- The PDF is scanned/image-only and has no extractable text.
- The supported statement layout cannot be detected.
- No transaction table can be found.
- The file is not a PDF.
- The file is empty, malformed, or too large for the configured limit once such a limit exists.

The output should make these things visible:

- Input format: `pdf`.
- Source name.
- Statement shape or source hint used, if any.
- Extraction status: success, partial, or failed.
- Extraction warnings.
- Normalized row count.
- Candidate lines that could not be normalized.
- Traceability for each normalized row.
- The normal ExpenseFlow report generated from normalized rows when extraction is processable.

## 3. Non-goals

The first PDF iteration does not include:

- Arbitrary bank/card PDF support.
- Scanned PDF support.
- OCR.
- LLM extraction.
- LLM analysis.
- AI categorization.
- AI total validation.
- AI reconciliation.
- Database persistence.
- Authentication or user accounts.
- Frontend or dashboard.
- Manual correction workflow.
- Background jobs or queues.
- Docker, cloud deployment, or production infrastructure.
- Automatic processing of real private files in public fixtures.
- Runtime multi-agent architecture.
- Application-level AI agents.
- Bank integrations or account syncing.
- General document intelligence for every statement layout.

The repo-local files in `agents/` are role-based planning lenses only. They are not runtime agents inside ExpenseFlow and should not be represented as application architecture.

## 4. Privacy and data policy

PDF statements are high-sensitivity documents. Real credit card PDFs and raw personal statements must remain local and private.

Real statements:

- Must never be committed to the repository.
- Must never be pasted into docs, issues, tests, examples, prompts, screenshots, logs, or public artifacts.
- Should be stored only in ignored local folders such as `local-input/` or another explicitly ignored private path.
- Should not be sent to external APIs or LLM providers during this phase.
- Should not be used to create public fixtures unless fully transformed into synthetic data.

Anonymized statements:

- Are risky because statement structure can still reveal personal, bank, account, merchant, or location information.
- May be used locally for exploration only after manual review.
- Should not be committed unless the anonymization is complete enough to remove names, account numbers, card numbers, addresses, tax IDs, real merchants, real transaction amounts, real dates that identify the user, barcodes, QR codes, statement IDs, and hidden PDF metadata.
- Should be treated as private by default.

Synthetic PDF fixtures:

- Are the preferred public fixture strategy.
- May be committed if all names, merchant descriptions, transaction codes, card details, addresses, statement numbers, and amounts are synthetic.
- Should mimic layout complexity without copying a real statement exactly.
- Should include realistic edge cases such as multi-page tables, repeated headers, credits/payments, installments, malformed rows, and total mismatch scenarios.

Local-only files:

- Should live under ignored folders such as `local-input/`, `local-output/`, or `private-samples/` if those folders are added to `.gitignore`.
- Should not be referenced by absolute path in committed docs or tests.
- Should not be required for automated tests or release gates.

Gitignore expectations:

- Before real local PDF experiments begin, ignored local input/output folders should exist.
- Generated extraction output from private PDFs should be ignored.
- Temporary PDF extraction artifacts should be ignored.

Redaction expectations:

- Redaction must remove visible text and hidden metadata.
- Redaction must not rely only on black rectangles over text if the underlying text remains extractable.
- Redacted examples should be inspected with text extraction before being considered safe.
- Prefer synthetic recreation over redacting real statements for committed fixtures.

External API and LLM restrictions:

- No external API or LLM provider should receive real statement content in this phase.
- No API keys, tokens, credentials, or provider configuration should be added.
- Future external calls require explicit privacy, retention, redaction, provider terms, and cost decisions.

Must never be committed:

- Real credit card statements.
- Raw personal PDF statements.
- Full card numbers, account numbers, tax IDs, addresses, phone numbers, emails, names, credentials, API keys, tokens, bank identifiers, or statement IDs.
- Extracted text from real statements.
- Screenshots of real statements.
- Generated reports from real statements.

Can be safely committed:

- Synthetic PDF fixtures.
- Synthetic extracted text fixtures.
- Synthetic normalized-row fixtures.
- Test expectations based on synthetic data.
- Documentation describing the policy without real examples.

## 5. PDF input model

The backend should conceptually receive:

- `sourceName`: the original PDF filename or caller-provided label.
- `pdfBytes` or `pdfBase64`: the uploaded PDF content.
- `expectedTotal`: optional user-provided expected total, separate from extracted content.
- `statementMetadata`: optional caller-provided metadata such as statement period, issuer label, currency, or account nickname.
- `sourceHints`: optional hints for statement type, issuer/card family, language, date format, decimal format, or table layout.

The first implementation should not require authentication, persistence, cloud storage, or background processing. PDF bytes should be processed for a single request and not stored unless a later persistence decision explicitly changes that.

The API shape is not accepted yet. Options to evaluate later include:

- JSON with base64 PDF content for testability.
- Multipart upload for more realistic file upload behavior.
- An internal application service first, with an endpoint added only after extraction and normalization behavior is tested.

The first PDF input model should still preserve the existing expectation that expected total is caller-supplied and optional. Extracting the statement total from the PDF can be attempted later, but it should not be required for the first iteration.

## 6. Extraction strategy

The conceptual pipeline is:

```text
PDF file
-> text/table extraction
-> candidate transaction lines
-> normalized transaction rows
-> existing ExpenseFlow processing pipeline
```

The extraction layer should map statement content into the existing required fields:

- `date`: transaction date from the statement row.
- `description`: merchant, transaction, payment, refund, fee, or movement description.
- `amount`: signed decimal value using the normalized ExpenseFlow amount format.

It should also map optional fields when available:

- `code`: authorization, reference, voucher, operation, or transaction code.
- `installment`: installment marker such as `03/06` when present.
- `source_type`: deterministic extraction hint such as `purchase`, `refund`, `payment`, `fee`, `transfer`, `adjustment`, or `unknown`.
- `notes`: extraction notes, warnings, or statement-specific context safe to expose.

Extraction is not categorization. A PDF extractor may identify text, table cells, candidate line structure, and source hints, but it must not decide final expense categories.

Extraction is not total validation. It may optionally extract a statement total as metadata in a later iteration, but processed total and expected-total comparison must remain deterministic report behavior.

Extraction should not silently discard candidate rows. If a line looks transaction-like but cannot be normalized, the extractor should return a warning or invalid candidate with traceability instead of dropping it.

Extracted rows that cannot be confidently normalized should become review or invalid candidates later. The normalized-row model should carry enough raw evidence for downstream validation to explain why a row became invalid or review-required.

For the first iteration, the safest flow is to normalize extracted PDF transactions into the same shape the CSV parser already produces conceptually, then reuse the existing deterministic processing service.

## 7. Traceability strategy

Every normalized row from a PDF should preserve extraction evidence:

- `sourceName`: original statement source name.
- `inputFormat`: `pdf`.
- `statementShape`: supported shape identifier or source hint used.
- `sourcePage`: page number where the candidate was found.
- `extractionOrder`: stable row order across the PDF.
- `rawExtractedText`: safe raw line or table-cell text, when safe to expose.
- `rawExtractedCells`: optional cell-level values for table extraction.
- `extractionMethod`: text extraction, table extraction, layout rule, or later OCR if ever added.
- `extractionConfidence`: optional deterministic score or classification, if the extractor can provide meaningful confidence.
- `extractionWarnings`: split line, repeated header, ambiguous sign, unsupported amount format, missing field, page continuation, duplicate candidate, or similar.
- `normalizedFields`: date, description, amount, optional code, installment, source type, and notes.

Traceability helps debugging because PDF failures are often layout failures, not business rule failures. Page and raw evidence make it possible to see whether the extractor missed a row, merged two rows, included a footer as a transaction, or misunderstood debit/credit signs.

Traceability supports auditability because the final report can explain not only how a row was categorized, but also where the row came from in the original statement.

Traceability supports review because ambiguous or invalid rows can point back to the source page and extracted evidence without requiring the user to search the whole PDF.

Privacy note: raw extracted text from real statements may be sensitive. In public fixtures it can be synthetic. In private processing, raw evidence should be minimized, masked where appropriate, and not logged by default.

## 8. Error handling strategy

Use three levels of error and warning behavior:

- File-level failures: the PDF cannot produce a meaningful extraction result.
- Extraction warnings: the PDF can produce rows, but with issues that must remain visible.
- Row-level invalid/review cases: normalized rows enter the existing processing pipeline but fail validation or require review.

Unsupported PDF:

- File-level failure.
- Return a structured error explaining that the statement shape is unsupported.
- Do not attempt generic guessing across arbitrary layouts in the first iteration.

Encrypted or password-protected PDF:

- File-level failure.
- Do not request or store passwords in the first iteration.
- Report that password-protected PDFs are unsupported.

Scanned/image-only PDF:

- File-level failure for the first iteration.
- Report that OCR is future work.
- Do not add OCR fallback silently.

No transaction table found:

- File-level failure.
- Include statement shape/source hint used and extraction warnings if safe.
- Do not produce an empty trusted report unless the document is truly a valid empty statement and that behavior has been explicitly scoped.

Partial extraction:

- Processable with extraction warnings if at least one transaction row can be normalized and unresolved candidates are visible.
- The output should indicate partial extraction status.
- The existing report may still run, but users should see that extraction completeness needs review.

Malformed transaction row:

- Preserve source page and raw evidence.
- Normalize what can be normalized.
- Convert missing or invalid required fields into row-level invalid behavior later, matching existing invalid-row visibility.

Total not found:

- Extraction warning or metadata absence, not a file-level failure.
- Expected total can still be provided separately by the caller.
- If no expected total is provided, the existing report should use `not_provided`.

Expected total mismatch after extraction:

- Existing deterministic report behavior.
- Do not use AI to reconcile the mismatch.
- The mismatch should point users to extraction warnings, invalid rows, review items, and audit details.

Extraction library failure:

- File-level failure if extraction cannot continue.
- Generic user-facing error without stack traces or local paths.
- Internal logs should not include full raw statement text by default.

File too large:

- Future file-level failure once size limits are configured.
- The first plan should define the need for limits, but implementation can choose values later.

## 9. Testing strategy

Tests should be designed before implementation and should use synthetic fixtures only.

Synthetic PDF fixture strategy:

- Create one synthetic PDF for the first supported statement shape.
- Include a clear transaction table with date, description, and amount.
- Include optional fields where useful: code, installment, source type hints, notes.
- Include a statement summary/total area if testing total extraction later.
- Include repeated headers/footers and at least one multi-page variant before claiming multi-page support.
- Keep all merchants, card details, account references, statement numbers, and amounts synthetic.

Extraction unit tests:

- Detect supported statement shape.
- Extract text/table rows from the known synthetic layout.
- Ignore repeated headers, page footers, and summary labels.
- Preserve page number and extraction order.
- Preserve raw evidence for each extracted row.
- Produce warnings for malformed, split, or ambiguous lines.
- Reject unsupported/encrypted/scanned fixture types when practical.

Integration tests: PDF to normalized rows:

- Given a supported synthetic PDF, produce normalized rows with required `date`, `description`, and `amount`.
- Preserve optional `code`, `installment`, `source_type`, and `notes` when present.
- Preserve extraction traceability for each row.
- Keep unresolved candidate lines visible.
- Verify no candidate transaction line is silently dropped.

End-to-end tests: PDF to report:

- Supported synthetic PDF produces the same report semantics as the equivalent CSV rows.
- Processed totals are calculated by existing deterministic logic.
- Expected total match and mismatch behavior matches the CSV baseline.
- Invalid extracted rows remain visible.
- Review-required rows remain visible.
- No AI, external API, persistence, auth, frontend, Docker/cloud, or OCR dependency is required.

Privacy tests:

- Public PDF fixtures contain only synthetic data.
- No real card numbers, account numbers, names, addresses, tax IDs, credentials, emails, or statement IDs appear.
- No hidden PDF metadata contains real personal data.
- Private local folders and extraction output folders are ignored by git before real local tests begin.

Required PDF-specific scenarios:

- No silent row dropping.
- Unsupported PDF behavior.
- Encrypted PDF behavior, if a practical synthetic encrypted fixture is created.
- Scanned/image-only PDF reported as unsupported/future.
- Malformed transaction table.
- Repeated header/footer handling.
- Multi-page statement handling.
- Split descriptions across lines.
- Debit/credit sign ambiguity.
- Payments/refunds/credits visible and safely handled.
- Duplicate-looking extracted rows flagged, not removed.
- Extraction warning visibility in API/application result.

## 10. Architecture proposal

PDF ingestion should live at the ingestion boundary, not inside categorization, totals, reporting, or API mapping.

Recommended direction:

- Keep `ExpenseFlow.Domain` independent from PDF libraries.
- Put application-facing abstractions in `ExpenseFlow.Application`.
- Put PDF library-specific implementation in `ExpenseFlow.Infrastructure`.
- Keep `ExpenseFlow.Api` thin: bind request, call application use case, map result.
- Reuse existing deterministic processing after PDF rows are normalized.
- Preserve the existing CSV workflow unchanged.

Possible application abstractions:

- `IExpenseInputAdapter`: converts a supported source input into normalized transaction rows plus ingestion metadata and warnings.
- `ITransactionFileParser`: may remain the CSV parser abstraction while a broader adapter abstraction is introduced above it.
- `IPdfStatementExtractor`: extracts PDF-specific candidate rows and traceability.
- `INormalizedTransactionRowSource`: a result model containing normalized rows, invalid candidates, extraction warnings, and source metadata.

The exact names are not accepted decisions yet. The key boundary is that PDF extraction should produce normalized transaction candidates, and the existing processing service should consume those candidates through a format-neutral path.

Two architecture options should be evaluated before coding:

1. Extend the parser boundary.
   - Add a PDF implementation beside the CSV parser.
   - Pro: simple mental model.
   - Con: the current parser abstraction may assume CSV text and headers too strongly.

2. Add an input adapter layer above parsers.
   - CSV adapter converts CSV text into normalized rows.
   - PDF adapter extracts and normalizes PDF rows.
   - Existing processing service consumes normalized rows.
   - Pro: clearer separation between source ingestion and deterministic processing.
   - Con: requires a small refactor around the existing CSV flow.

Prefer the option that changes the least while making PDF support clean and testable. Do not refactor the core pipeline casually.

PDF extraction maps into the existing parser/processing service by producing the equivalent of:

- source row number or extraction order
- raw values
- parsed candidate values where possible
- validation errors or extraction warnings
- source metadata

A new normalized row abstraction may be needed if the current CSV parser result is too CSV-specific. It should be introduced only to remove duplication and protect the core pipeline from source-format coupling.

Candidate PDF library choices should be evaluated later, not accepted in this plan. Possible .NET options to research include:

- UglyToad.PdfPig for text extraction.
- iText or commercial libraries if licensing and use case justify them.
- Pdfium-based options if rendering is needed later.

OCR libraries and cloud document-intelligence services are future work unless a later decision explicitly adds them.

## 11. LLM future seam

Future LLM analysis can be added later, after deterministic PDF extraction and normalization work reliably.

LLM may later help with:

- Explaining review-required transactions in plain language.
- Suggesting categories for already review-required rows.
- Recommending new deterministic merchant rules from repeated reviewed decisions.
- Summarizing spending patterns after deterministic totals exist.
- Interpreting ambiguous merchant descriptions after privacy review.

LLM must not:

- Extract PDF rows for the first PDF iteration.
- Calculate financial totals.
- Validate expected totals.
- Reconcile mismatches as financial truth.
- Override deterministic rules.
- Silently finalize categories.
- Receive real statement content without explicit privacy, redaction, provider, retention, and cost decisions.
- Become a runtime multi-agent architecture for this phase.

Future provider seam:

- Provider details should live behind an application-facing abstraction.
- Infrastructure should contain provider implementations.
- Tests should use a fake provider.
- AI output should be structured, validated, confidence-aware, auditable, and safe to reject.
- Redaction/anonymization should happen before external calls.
- Provider terms and retention behavior must be reviewed before real data is used.

No LLM dependency should be part of the PDF ingestion release gate. The PDF phase should pass with deterministic extraction, deterministic normalization, and the existing deterministic ExpenseFlow report pipeline only.

This phase is not implementing runtime AI agents.

## 12. First implementation milestones

### PDF-0 - Privacy and fixture guardrails

| Field | Plan |
| --- | --- |
| Goal | Prepare the repository and policy for safe PDF work before any parser code. |
| Expected changes | Add or verify ignored local folders for private PDFs and extraction outputs; document synthetic fixture rules if needed. |
| Tests | `git status`; static review that no real PDFs or extracted real text are committed. |
| Explicit non-goals | No PDF library, no endpoint, no extraction code, no OCR, no LLM, no real fixtures. |
| Suggested commit message | `Prepare PDF privacy guardrails` |

### PDF-1 - Ingestion boundary design

| Field | Plan |
| --- | --- |
| Goal | Define the minimal application abstraction for source ingestion without breaking CSV behavior. |
| Expected changes | Add design notes or small contracts only when implementation begins; decide whether to extend parser boundary or add input adapter layer. |
| Tests | Existing CSV tests must continue passing; add contract tests only if code is introduced. |
| Explicit non-goals | No PDF library, no OCR, no LLM, no broad refactor. |
| Suggested commit message | `Define input ingestion boundary` |

### PDF-2 - Synthetic PDF fixture strategy

| Field | Plan |
| --- | --- |
| Goal | Design the first synthetic PDF statement fixture and expected normalized rows. |
| Expected changes | Fixture design doc or generated synthetic fixture only after privacy review. |
| Tests | Static synthetic-data review; expected row count and expected normalized fields. |
| Explicit non-goals | No real PDFs, no anonymized real statements committed, no arbitrary bank support. |
| Suggested commit message | `Design synthetic PDF fixture` |

### PDF-3 - PDF text extraction prototype

| Field | Plan |
| --- | --- |
| Goal | Extract text/table candidates from the one supported synthetic statement shape. |
| Expected changes | Infrastructure PDF extractor implementation behind an application boundary. |
| Tests | Unit tests for supported fixture, unsupported PDF, scanned/image-only unsupported behavior if practical, encrypted unsupported behavior if practical. |
| Explicit non-goals | No categorization changes, no report changes, no external APIs, no OCR, no LLM. |
| Suggested commit message | `Extract synthetic PDF transaction candidates` |

### PDF-4 - Normalize extracted rows

| Field | Plan |
| --- | --- |
| Goal | Convert extracted candidate lines into normalized transaction rows with traceability. |
| Expected changes | Normalization logic for date, description, amount, optional code/installment/source type/notes, and extraction warnings. |
| Tests | PDF-to-normalized-row tests, malformed row tests, repeated header/footer tests, split-line warning tests. |
| Explicit non-goals | No category changes, no total calculation inside extraction, no manual correction workflow. |
| Suggested commit message | `Normalize PDF transaction rows` |

### PDF-5 - PDF service or endpoint integration

| Field | Plan |
| --- | --- |
| Goal | Feed normalized PDF rows into the existing processing pipeline through a thin application/API surface. |
| Expected changes | Application service and possibly Minimal API endpoint for PDF input. |
| Tests | Integration tests proving CSV behavior unchanged and PDF rows enter the existing deterministic report workflow. |
| Explicit non-goals | No persistence, no auth, no frontend, no background jobs, no LLM. |
| Suggested commit message | `Process PDF statements through existing pipeline` |

### PDF-6 - End-to-end PDF report tests

| Field | Plan |
| --- | --- |
| Goal | Establish the PDF release gate using synthetic fixtures. |
| Expected changes | E2E tests for supported PDF, partial extraction, expected total match/mismatch, no silent row dropping, and extraction traceability. |
| Tests | `dotnet restore`, `dotnet build`, `dotnet test`; PDF-specific fixture tests. |
| Explicit non-goals | No broad layout support, no OCR, no LLM, no real data. |
| Suggested commit message | `Cover PDF ingestion release gate` |

### PDF-7 - Documentation and demo update

| Field | Plan |
| --- | --- |
| Goal | Document how the supported synthetic PDF workflow works and how it differs from future OCR/LLM work. |
| Expected changes | README, API examples, demo docs, architecture summary updates if implementation exists. |
| Tests | Documentation examples align with passing tests. |
| Explicit non-goals | Do not claim arbitrary PDF support, OCR, LLM analysis, or runtime multi-agent architecture. |
| Suggested commit message | `Document PDF ingestion workflow` |

## 13. Open questions

- What exact credit card statement shape should be supported first?
- Should the first public PDF fixture be fully synthetic, or is there a safe need for an anonymized fixture? Recommendation: use synthetic.
- Which PDF extraction library best fits .NET 10, licensing, table extraction needs, and portfolio clarity?
- Should the first API accept base64 PDF JSON, multipart upload, or should implementation start with an application service before adding HTTP?
- Which ignored local folders should hold private statement experiments?
- Should expected total continue to be caller-provided only in the first PDF iteration?
- Should statement total extraction be attempted in the first iteration or deferred?
- How should multi-page PDFs be scoped: first supported immediately or introduced after single-page extraction works?
- How should repeated headers and page footers be identified for the first statement shape?
- How should debit and credit columns map to signed amounts?
- How should payments, refunds, credits, taxes, and interest charges be represented as `source_type` hints?
- Should scanned PDFs fail as unsupported, or should a future OCR milestone be planned separately? Recommendation: fail as unsupported for the first PDF iteration.
- What maximum file size should be accepted?
- What raw extraction evidence can be safely returned in API responses for private PDFs?
- Does the existing parser result model already support source-format traceability, or is a normalized row abstraction needed?
- Should the decision log be updated when PDF-0 starts, or only after a specific implementation boundary is accepted?

## 14. Recommended next prompt

Recommended intelligence level: high.

Use this exact next prompt to start PDF-0, but do not execute it yet:

```text
Before coding, inspect the repository from disk and treat it as the source of truth.

Read:
- AGENTS.md
- README.md
- docs/pdf-ingestion-plan.md
- docs/backend-architecture.md
- docs/architecture-summary.md
- docs/decisions.md
- docs/risk-register.md
- docs/demo-data-policy.md
- backend/.gitignore or .gitignore files that exist
- agents/product-manager-agent.md
- agents/document-extraction-agent.md
- agents/data-engineer-agent.md
- agents/backend-architect-agent.md
- agents/qa-agent.md
- agents/security-agent.md
- skills/pdf-statement-ingestion/SKILL.md
- skills/backend-architecture-review/SKILL.md
- skills/test-case-generation/SKILL.md

Use these role definitions only as planning/review lenses:
- Product Manager Agent
- Document Extraction Agent
- Data Engineer Agent
- Backend Architect Agent
- QA Agent
- Security Agent

Apply these skills:
- PDF Statement Ingestion Skill
- Backend Architecture Review Skill
- Test Case Generation Skill

Implement PDF-0 only from docs/pdf-ingestion-plan.md.

Explicit non-goals:
- Do not implement PDF parsing.
- Do not add a PDF library.
- Do not create PDF fixtures yet.
- Do not add OCR.
- Do not add external APIs.
- Do not add LLM integration.
- Do not add database persistence.
- Do not add authentication.
- Do not add frontend.
- Do not add Docker/cloud deployment.
- Do not create runtime AI agents or runtime multi-agent architecture.

Tasks:
1. Verify existing gitignore coverage for local private files.
2. Add focused ignore rules for local private PDF inputs and generated extraction outputs if missing.
3. Add a short documentation note only if needed to clarify that real statements must remain local/private.
4. Do not modify backend source code.
5. Run a status/diff check.

Expected outputs:
- Files changed.
- Privacy guardrails added or confirmed.
- What was intentionally not changed.
- Whether it is safe to commit.

Keep changes small and reviewable.
Suggested commit message: Prepare PDF privacy guardrails
```
