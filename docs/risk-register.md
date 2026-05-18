# Risk Register

| ID | Risk | Impact | Probability | Mitigation | Owner Agent | Status |
| --- | --- | --- | --- | --- | --- | --- |
| R-001 | Scope creep | High | High | Define MVP boundaries, maintain a roadmap, and challenge each new feature against first-user value. | Founder Agent | Open |
| R-002 | Becoming a generic CRUD app | High | Medium | Tie every feature to expense file processing, categorization, validation, reporting, or auditability. | Product Manager Agent | Open |
| R-003 | Overusing AI | High | Medium | Use deterministic rules for totals, validation, and known patterns; reserve AI for ambiguous interpretation. | AI Architect Agent | Open |
| R-004 | Poor categorization accuracy | High | Medium | Start with deterministic known patterns, require review for uncertainty, and test category outcomes. | Domain Expert Agent | Open |
| R-005 | Using real personal data by mistake | High | Medium | Use synthetic data publicly, add future ignore rules for local files, and review examples before committing. | Security Agent | Open |
| R-006 | Spending too much time on PDF parsing early | Medium | High | Prefer CSV or Excel exports first and document PDF support as a future decision. | Data Engineer Agent | Open |
| R-007 | Overengineering architecture | Medium | Medium | Prefer simple, shippable architecture and defer infrastructure until product needs are validated. | Backend Architect Agent | Open |
| R-008 | Weak portfolio presentation | Medium | Medium | Create a clear demo story, README, decision log, architecture narrative, and recruiter-friendly explanation. | Technical Writer Agent | Open |
| R-009 | Lack of tests | High | Medium | Define acceptance tests and domain tests before broadening behavior. | QA Agent | Open |
| R-010 | No clear demo story | Medium | Medium | Define a synthetic end-to-end workflow and explain the backend value it demonstrates. | Marketing Agent | Open |
