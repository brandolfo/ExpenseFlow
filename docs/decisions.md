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
