---
name: ai-agent-design
description: Design AI-assisted behavior inside ExpenseFlow, including AI categorization, structured model output, confidence, guardrails, tool usage, fallback behavior, auditability, failure handling, and cost control.
---

# AI Agent Design Skill

Use this skill when designing AI-assisted behavior inside ExpenseFlow.

## When to use
Use when the task involves:
- AI categorization
- Structured model output
- Confidence
- Guardrails
- Tool usage
- Fallback behavior
- Auditability
- Cost control

## Principles
- AI should not calculate financial totals.
- AI should not replace deterministic validation.
- AI is useful for ambiguous interpretation, summaries, explanations, and recommendations.
- AI output must be structured.
- AI decisions must be auditable.
- Low-confidence decisions require review.

## Steps
1. Define the AI use case.
2. Explain why deterministic rules are not enough.
3. Define inputs.
4. Define structured output.
5. Define confidence behavior.
6. Define invalid output handling.
7. Define fallback behavior.
8. Define audit data to store.
9. Define cost-control strategy.

## Output
Return:
- AI use case
- Inputs
- Structured output
- Guardrails
- Failure modes
- Fallback behavior
- Audit requirements
- Cost controls
