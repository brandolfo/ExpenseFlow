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
