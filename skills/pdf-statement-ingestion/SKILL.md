---
name: pdf-statement-ingestion
description: Scope and design ExpenseFlow PDF statement ingestion before implementation, including document structure, transaction extraction, normalization into the existing pipeline, traceability, privacy, failure modes, and tests.
---

# PDF Statement Ingestion Skill

Use this skill when planning, reviewing, or scoping PDF statement ingestion for ExpenseFlow.

## When to use
Use when the task involves:
- PDF credit card or bank statements
- Document ingestion scope
- Extracted transaction field mapping
- Statement totals and metadata
- Document traceability
- Extraction failure modes
- Synthetic or redacted statement fixtures

## Principles
- Do not implement PDF support before product scope, privacy rules, and extraction assumptions are clear.
- Treat PDF statements as sensitive untrusted input.
- Use synthetic or redacted samples for committed fixtures and docs.
- Preserve evidence for extracted transactions, such as page number, source text, and extraction warnings.
- Normalize extracted transactions into the existing deterministic processing pipeline where possible.
- Do not use AI to calculate totals or validate financial correctness.
- Do not add OCR, external APIs, persistence, or LLM integration unless separately scoped.

## Steps
1. Identify the statement type, issuer assumptions, and whether the sample is synthetic or redacted.
2. Identify transaction table fields and statement metadata needed for validation.
3. Map extracted fields to ExpenseFlow transaction candidates.
4. Define source traceability for each transaction and file-level warning.
5. Separate deterministic extraction from ambiguous interpretation.
6. Define validation and review behavior for missing, duplicated, split, or malformed extracted rows.
7. Identify privacy and retention risks.
8. Define fixture and test coverage before implementation.
9. Separate PDF-phase MVP requirements from later OCR, external API, and LLM-assisted analysis.

## Output
Return:
- Statement assumptions
- Field mapping
- Normalization plan
- Traceability requirements
- Privacy risks
- Extraction failure modes
- Test scenarios
- MVP vs future scope
- Recommended next step
