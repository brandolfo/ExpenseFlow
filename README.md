# ExpenseFlow

ExpenseFlow is a backend-focused expense intelligence product that turns messy financial transaction files into categorized, validated, and useful reports.

## Current phase
Product discovery and agent setup. This repository intentionally contains no application code, .NET solution, database choice, endpoint design, or production infrastructure yet.

## Goals
- Build a useful personal tool for analyzing expenses from exported financial files.
- Create a serious portfolio project that demonstrates backend engineering depth.
- Show backend value through data processing, validation, rules, auditability, asynchronous workflows, testing, clean architecture, and responsible AI.
- Use agents as role-based collaborators for product and engineering reasoning.
- Keep deterministic financial logic separate from AI-assisted interpretation.

## Non-goals
- Do not build a generic CRUD app.
- Do not create application source code during the discovery phase.
- Do not choose a database, architecture style, endpoints, entities, or libraries before the relevant decisions are documented.
- Do not use real personal financial data in public files.
- Do not allow AI to calculate totals or replace deterministic validation.

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
1. Interview the first user about the current expense analysis workflow.
2. Define the smallest useful MVP workflow.
3. Document assumptions, risks, and initial product decisions.
4. Define synthetic demo data requirements.
5. Only after scope is clear, begin technical architecture decisions.

## Data warning
No real financial data should be committed. Use synthetic demo data only, and keep any local real files outside version control.
