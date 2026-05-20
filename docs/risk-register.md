# Risk Register

| ID | Risk | Impact | Probability | Mitigation | Owner Agent | Status |
| --- | --- | --- | --- | --- | --- | --- |
| R-001 | Scope creep | High | High | Keep completed CSV/PDF scope explicit, maintain the roadmap, and require new scope/decision docs before feature expansion. | Founder Agent | Open |
| R-002 | Becoming a generic CRUD app | High | Medium | Tie every feature to expense file processing, categorization, validation, reporting, or auditability. | Product Manager Agent | Open |
| R-003 | Overusing AI | High | Medium | Use deterministic rules for totals, validation, and known patterns; reserve AI for ambiguous interpretation. | AI Architect Agent | Open |
| R-004 | Poor categorization accuracy | High | Medium | Start with deterministic known patterns, require review for uncertainty, and test category outcomes. | Domain Expert Agent | Open |
| R-005 | Using real personal data by mistake | High | Medium | Use synthetic data publicly, keep private files outside version control, and review fixtures/docs before committing. | Security Agent | Open |
| R-006 | Spending too much time on PDF parsing early | Medium | Low | Scoped PDF ingestion has been completed only for the two accepted synthetic ICBC-like variants; arbitrary PDFs, OCR, LLM extraction, and real/private statement processing remain out of scope until separately decided. | Data Engineer Agent | Mitigated for scoped PDF phase |
| R-007 | Overengineering architecture | Medium | Medium | Prefer simple, shippable architecture and defer infrastructure until product needs are validated. | Backend Architect Agent | Open |
| R-008 | Weak portfolio presentation | Medium | Low | README, demo docs, decision log, and architecture summary now describe the implemented CSV/PDF release; keep them aligned as scope changes. | Technical Writer Agent | Mitigated |
| R-009 | Lack of tests | High | Low | Fixture-backed unit, integration, architecture-boundary, and release-gate tests now cover the implemented release; keep regression tests current before expanding behavior. | QA Agent | Mitigated; regression risk remains open |
| R-010 | No clear demo story | Medium | Low | The synthetic CSV and scoped PDF workflows are documented as the portfolio demo path; keep examples aligned with tested behavior. | Marketing Agent | Mitigated |
| R-011 | Documentation drift | Medium | Medium | Mark historical planning docs clearly, keep README as the public entrypoint, and update roadmap/risk docs when decisions or implementation status changes. | Technical Writer Agent | Open |
| R-012 | Arbitrary PDF expansion | High | Medium | Do not broaden beyond accepted synthetic variants without new scope, synthetic fixtures, extraction assumptions, privacy rules, and release-gate tests. | Product Manager Agent | Open |
| R-013 | PDF normalization overfits synthetic fixture markers | Medium | Medium | Current PDF normalization is intentionally scoped to synthetic ICBC-like fixtures. Before adding a third PDF shape or real/private statement experiments, review candidate detection so transaction-like lines inside active sections are not ignored solely because they lack current synthetic prefixes; add fixture-backed tests for malformed transaction-like lines inside active sections. | Data Engineer Agent | Open |
