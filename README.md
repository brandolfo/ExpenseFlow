# ExpenseFlow

ExpenseFlow is a backend-focused expense intelligence product that turns messy financial transaction files into categorized, validated, and useful reports.

## Current phase
Build plan defined; first implementation milestone can begin when explicitly requested. This repository intentionally contains no application code, .NET solution, database choice, endpoint implementation, or production infrastructure yet.

## Goals
- Build a useful personal tool for analyzing expenses from exported financial files.
- Create a serious portfolio project that demonstrates backend engineering depth.
- Show backend value through ASP.NET Core / .NET data processing, validation, deterministic rules, auditability, testing, pragmatic architecture, and responsible future AI.
- Use agents as role-based collaborators for product and engineering reasoning.
- Keep deterministic financial logic separate from AI-assisted interpretation.

## Non-goals
- Do not build a generic CRUD app.
- Do not create application source code until the build plan is accepted and implementation is explicitly requested.
- Do not create a database, endpoints, entities, or libraries before the relevant build decisions are documented.
- Do not use real personal financial data in public files.
- Do not allow AI to calculate totals or replace deterministic validation.

## Reading path
1. `docs/mvp-scope.md`
2. `docs/input-output-contract.md`
3. `docs/domain-model.md`
4. `docs/demo-dataset-design.md`
5. `docs/acceptance-tests.md`
6. `docs/demo-story.md`
7. `docs/ai-agent-design.md`
8. `docs/backend-architecture.md`
9. `docs/build-plan.md`
10. `docs/decisions.md`

## Agent team overview
ExpenseFlow uses role-based agent definitions to guide collaboration:
- Founder Agent protects focus, business value, and execution speed.
- Product Manager Agent turns vision into MVP scope, user stories, and acceptance criteria.
- UX Researcher Agent studies the current workflow, friction, trust issues, and desired outputs.
- Domain Expert Agent models expense processing concepts, rules, validation, and ambiguity.
- AI Architect Agent defines responsible AI use, structured output, guardrails, and auditability.
- Backend Architect Agent designs maintainable backend structure after product scope is clearer.
- Data Engineer Agent handles messy files, parsing risks, normalization, and synthetic datasets.
- QA Agent defines test scenarios, edge cases, acceptance tests, and failure modes.
- Security Agent protects privacy, secrets, financial data, and safe demo practices.
- DevOps Agent enters later for reproducibility, CI/CD, deployment, and operations.
- Technical Writer Agent makes the project understandable and portfolio-ready.
- Marketing Agent positions the project for GitHub, LinkedIn, CVs, and interviews.

## Repository structure
```text
/
  AGENTS.md
  README.md
  docs/
    product-brief.md
    product-discovery.md
    assumptions.md
    decisions.md
    glossary.md
    roadmap.md
    risk-register.md
    demo-data-policy.md
    mvp-scope.md
    domain-model.md
    ai-agent-design.md
    project-audit.md
    input-output-contract.md
    demo-dataset-design.md
    acceptance-tests.md
    demo-story.md
    backend-architecture.md
    build-plan.md
  agents/
    founder-agent.md
    product-manager-agent.md
    ux-researcher-agent.md
    domain-expert-agent.md
    ai-architect-agent.md
    backend-architect-agent.md
    data-engineer-agent.md
    qa-agent.md
    security-agent.md
    devops-agent.md
    technical-writer-agent.md
    marketing-agent.md
  skills/
    product-discovery/SKILL.md
    expense-domain-analysis/SKILL.md
    ai-agent-design/SKILL.md
    backend-architecture-review/SKILL.md
    test-case-generation/SKILL.md
    portfolio-positioning/SKILL.md
```

## Next steps
1. Implement Milestone 0 and Milestone 1 from `docs/build-plan.md` when explicitly requested.
2. Keep commits small and milestone-oriented.
3. Create actual synthetic fixtures only in the planned coding milestone.
4. Keep implementation aligned with the acceptance tests, demo story, and backend architecture.

## Data warning
No real financial data should be committed. Use synthetic demo data only, and keep any local real files outside version control.
