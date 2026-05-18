---
name: expense-domain-analysis
description: Analyze ExpenseFlow transaction domains, including categories, merchant patterns, ambiguous descriptions, categorization rules, validation, installments, refunds, duplicates, totals, manual review, and categorization accuracy.
---

# Expense Domain Analysis Skill

Use this skill when analyzing transactions, categories, merchant patterns, categorization rules, validation, installments, refunds, duplicates, or totals.

## When to use
Use when the task involves:
- Transaction categories
- Merchant matching
- Ambiguous descriptions
- Validation rules
- Expected vs processed totals
- Manual review
- Categorization accuracy

## Steps
1. Identify the transaction source and format.
2. Normalize descriptions conceptually.
3. Separate deterministic cases from ambiguous cases.
4. Identify category candidates.
5. Identify edge cases.
6. Define validation requirements.
7. Decide whether AI assistance is appropriate.
8. Recommend review behavior.

## Output
Return:
- Domain observations
- Candidate categories
- Deterministic rules
- Ambiguous cases
- Validation risks
- Review requirements
- Recommended next step
