---
name: backend-architecture-review
description: Review or propose ExpenseFlow backend design, including architecture, module boundaries, APIs, persistence, background jobs, validation, auditability, testing, maintainability, MVP architecture, and future architecture.
---

# Backend Architecture Review Skill

Use this skill when reviewing or proposing backend design.

## When to use
Use when the task involves:
- Architecture
- Module boundaries
- APIs
- Persistence
- Background jobs
- Validation
- Auditability
- Testing
- Maintainability

## Principles
- Avoid overengineering.
- Prefer modular monolith initially.
- Keep domain logic away from controllers.
- Keep deterministic business rules testable.
- Keep infrastructure replaceable.
- Preserve auditability.
- Design for future AI integration without coupling the core to a specific provider.

## Steps
1. Identify product requirements.
2. Identify core modules.
3. Define responsibilities.
4. Define data flow.
5. Identify key abstractions.
6. Identify risks.
7. Define testing strategy.
8. Separate MVP architecture from future architecture.

## Output
Return:
- Proposed modules
- Responsibilities
- Data flow
- Key abstractions
- Risks
- Testing strategy
- MVP vs future architecture
