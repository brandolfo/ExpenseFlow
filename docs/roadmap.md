# Roadmap

> Current status: the deterministic CSV MVP, scoped synthetic PDF ingestion for `icbc-visa-like-v1` and `icbc-mastercard-like-v1`, and GitHub Actions Backend CI are complete. Future work below is directional only and requires separate scope and decision documentation before implementation.

## Implemented

- Deterministic CSV file-to-report backend workflow.
- Scoped synthetic text-selectable PDF ingestion for the two accepted ICBC-like variants.
- Fixture-backed unit and integration test release gate.
- GitHub Actions Backend CI running restore, build, and test from `backend/`.
- Portfolio docs, API examples, demo script, interview pitch, architecture summary, and synthetic-data guardrails.

## Future Candidates

- Manual correction workflow with correction history.
- Persisted report history after product value is proven.
- Richer deterministic rule management.
- Export formats for generated reports.
- Excel input parser behind the parser boundary.
- Additional deterministic PDF variants after separate scope, synthetic fixtures, and tests.
- OCR only after a separate decision.
- Responsible AI suggestions only for already review-required rows.
- Frontend or dashboard once backend behavior is stable.
- Authentication and multi-user support after product value is proven.
- Deployment and observability hardening after separate scope and decisions.

## Still Out Of Scope

- Arbitrary bank/card PDF support.
- Real/private statement processing in public artifacts.
- AI for totals, deterministic validation, or final financial decisions.
- Runtime multi-agent architecture.
- Docker/cloud deployment without a future accepted decision.
