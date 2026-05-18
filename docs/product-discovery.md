# Product Discovery

## Purpose
This document consolidates a simulated product discovery meeting for ExpenseFlow using four role agents:
- Founder Agent
- Product Manager Agent
- UX Researcher Agent
- Domain Expert Agent

The goal is to define the first useful version of ExpenseFlow without making technical implementation decisions.

## Meeting summary
ExpenseFlow should start with a narrow, trustworthy workflow: transform one exported transaction file into a categorized, validated expense report using deterministic rules for known patterns, explicit review flags for uncertain transactions, and deterministic total validation. The first version should prove backend value through data processing, validation, rules, auditability, and clear reporting, not through broad feature count or premature AI usage.

## Agent perspectives

### Founder Agent

#### Perspective
ExpenseFlow needs to become useful quickly while staying portfolio-grade. The first version should show that Santiago can build a focused backend product with real domain value, not a generic CRUD app or an AI wrapper.

#### Key concerns
- Scope creep could delay the first useful version.
- PDF support, dashboards, accounts, budgets, and advanced AI could distract from the core workflow.
- A broad product could become harder to explain in a portfolio.
- The project must demonstrate backend skill through processing, validation, auditability, and reliable outputs.

#### Recommended scope
- Support a single end-to-end expense processing workflow.
- Focus on exported transaction files, deterministic categorization for known patterns, total validation, and a useful summary.
- Use synthetic demo data for public examples.
- Defer advanced workflows until the first version proves value.

#### Risks
- Building too much before validating the first workflow.
- Choosing features because they sound impressive rather than because they solve Santiago's problem.
- Presenting responsible AI as a headline before the deterministic product foundation exists.

#### Questions to validate
- Which output would actually save Santiago time in the first week of use?
- What is the smallest workflow Santiago would be willing to reuse monthly?
- Which features would make the project more convincing in an interview?
- Which requested features can wait without damaging the first version?

### Product Manager Agent

#### Perspective
The MVP should be defined as a user workflow with clear acceptance criteria, not as a list of technologies. The product should solve the repetitive work of turning transaction files into trusted summaries.

#### Key concerns
- The target user and workflow need sharper validation.
- MVP and future features must stay separate.
- The first version must define what "done" means for categorization, validation, and reporting.
- The product must make uncertainty visible instead of pretending all classifications are correct.

#### Recommended scope
- User provides one transaction export and an expected total or statement total when available.
- ExpenseFlow processes all rows, applies known categorization rules, flags uncertain transactions, and returns a structured report.
- The report includes category totals, processed totals, validation status, and review items.
- The MVP does not require AI assistance.

#### Risks
- A report that is technically correct but not useful to the user.
- Categories that are too detailed too early.
- No clear acceptance criteria for "reliable enough."
- Confusing future features with MVP requirements.

#### Questions to validate
- What file exports does Santiago currently have access to?
- What categories does Santiago actually use when reviewing expenses?
- Does Santiago care more about category totals, merchant totals, monthly totals, or validation status?
- What should happen when a transaction cannot be categorized confidently?
- What minimum report would feel useful enough to keep using?

### UX Researcher Agent

#### Perspective
The first version should reduce the user's current manual friction while preserving trust. Since the first user is technical, the experience can be backend-first, but the workflow still needs to feel clear, inspectable, and repeatable.

#### Key concerns
- The current manual workflow is still partly assumed.
- Trust may depend more on transparency than automation.
- Santiago may need to inspect unresolved transactions before trusting a report.
- The product should avoid hiding messy data problems behind polished output.

#### Recommended scope
- Map Santiago's current workflow before adding extra features.
- Capture the before-and-after story: export, clean, categorize, validate, summarize.
- Make uncertain transactions and validation mismatches visible.
- Prioritize a report format that Santiago can inspect and reuse.

#### Risks
- Designing for an imagined workflow rather than Santiago's real one.
- Producing outputs that look complete but do not answer the user's actual questions.
- Making review feel like failure instead of a normal trust-building step.

#### Questions to validate
- How does Santiago currently export and review expenses?
- What does Santiago do manually today in a spreadsheet or notes?
- Which steps are annoying, repetitive, or easy to get wrong?
- What does Santiago check before trusting his own totals?
- How much manual review is acceptable for uncertain transactions?

### Domain Expert Agent

#### Perspective
Expense processing is messy because transaction descriptions, merchants, refunds, installments, fees, duplicate rows, date formats, and totals can vary. The first version should explicitly model this mess instead of assuming clean input.

#### Key concerns
- Known merchant patterns should use deterministic rules.
- Ambiguous transactions should be flagged for review.
- Totals must always be calculated deterministically.
- No transaction should be silently ignored.
- Refunds, negative amounts, duplicate transactions, and installments need clear treatment, even if some are deferred.

#### Recommended scope
- Define a small starting category taxonomy.
- Support deterministic categorization for known merchant or description patterns.
- Mark unmatched or conflicting transactions as review required.
- Validate processed totals against an expected total when available.
- Include processing counts: total rows, processed rows, categorized rows, review rows, invalid rows.

#### Risks
- Incorrect category assignment creates false confidence.
- Too many categories reduce accuracy and usefulness.
- Edge cases such as refunds or duplicate rows may distort reports.
- Total validation without clear rules may confuse users.

#### Questions to validate
- Which transaction fields are usually available in Santiago's exports?
- What are the most common merchant patterns Santiago wants categorized?
- What category taxonomy is useful but not excessive?
- Should refunds reduce category totals, appear separately, or both?
- How should duplicate-looking transactions be surfaced?

## Consolidated discovery

## Target user
The first target user is Santiago, a backend developer from Argentina who wants:
- A useful personal tool to analyze expenses from exported financial files.
- A serious portfolio project that demonstrates backend engineering ability.
- A product that uses agents and AI responsibly, only where they add value.

Early users are likely to be technical or semi-technical people who are comfortable with files, structured reports, API documentation, and explicit review steps. Future users may include freelancers, individuals, small businesses, consultants, accountants, and finance/admin teams.

## Current workflow
Current understanding to validate:
1. The user exports transactions from a bank, card provider, wallet, or spreadsheet.
2. The user opens the file in a spreadsheet or similar tool.
3. The user cleans inconsistent names, dates, amounts, or columns.
4. The user manually groups transactions into categories.
5. The user checks whether totals match the expected statement or source total.
6. The user identifies confusing merchants, refunds, duplicates, or installment payments.
7. The user creates a summary by category, merchant, period, or source.
8. The user repeats similar work whenever a new file arrives.

## Pain points
- Manual categorization is repetitive and easy to forget or apply inconsistently.
- Merchant descriptions are often noisy, abbreviated, duplicated, or unclear.
- Cleaning files by hand is slow and creates room for mistakes.
- It is hard to know whether every transaction was processed.
- Category totals are hard to trust without deterministic validation.
- Ambiguous transactions need review instead of silent guessing.
- Public portfolio demos cannot use real financial data.
- AI-only classification would be hard to audit and risky for financial summaries.

## Desired outcome
The user can provide a transaction export and receive a structured expense report that:
- Accounts for every transaction.
- Categorizes known patterns deterministically.
- Flags ambiguous or invalid transactions for review.
- Calculates totals deterministically.
- Compares processed totals with an expected total when available.
- Shows category summaries and review items.
- Preserves enough processing detail to explain how the report was produced.

## First valuable workflow
1. Provide one transaction export using synthetic demo data or local private data.
2. Provide an expected total when available.
3. Process every row into a normalized transaction list conceptually, without silently dropping any row.
4. Apply deterministic categorization rules for known merchant or description patterns.
5. Mark unmatched, conflicting, invalid, or unclear transactions as requiring review.
6. Calculate processed totals deterministically.
7. Compare processed totals against the expected total when available.
8. Return a structured report with summary totals, category totals, validation status, and review items.
9. Preserve an audit trail that explains rule matches, review flags, invalid rows, and total validation results.

## MVP candidate
The MVP candidate is a backend-first expense file processing workflow without AI.

It should include:
- One supported transaction file shape to start, using synthetic demo data for public examples.
- Required transaction fields defined at the product level, such as date, description, and amount.
- A small starting category taxonomy.
- Deterministic categorization for known patterns.
- Review-required status for uncertain transactions.
- Deterministic total calculation.
- Expected total vs processed total validation when expected total is available.
- A structured expense report with category totals, processing counts, validation status, and review items.
- Clear handling of invalid rows so no transaction disappears silently.
- Documentation that explains the workflow and why it demonstrates backend value.

## Explicit out-of-scope items
For the first useful version, these are out of scope:
- Real financial data in public examples.
- PDF parsing.
- Bank integrations.
- Automatic account syncing.
- User accounts and authentication.
- Budgets, goals, alerts, or forecasting.
- Dashboards or polished frontend screens.
- Multi-user collaboration.
- Payment provider integrations.
- AI-based total calculation.
- AI-based deterministic validation.
- AI categorization for the first workflow.
- Complex tax or accounting compliance.
- Long-term financial advice or recommendations.
- Choosing a framework, database, endpoint design, entity model, or library.

## Assumptions
- Santiago can obtain at least one structured export format before PDF support is needed.
- A backend-first workflow can be compelling if the report, validation, auditability, and documentation are strong.
- A small category taxonomy is more useful early than a detailed taxonomy that creates confusion.
- Deterministic categorization for known patterns is enough to make the first version useful.
- Manual review is acceptable for ambiguous transactions.
- Synthetic data can support a credible public demo.
- Expected total validation is important for trust.
- The first version should prove the workflow before adding AI.

## Validation questions
- What export formats can Santiago reliably get today?
- What fields are present in those exports?
- What does Santiago currently do manually after exporting expenses?
- Which expense categories does Santiago actually care about?
- Which merchants or descriptions appear often enough to justify deterministic rules?
- What report output would Santiago inspect first?
- What makes Santiago trust that no transactions were lost?
- How should uncertain transactions be presented for review?
- What should happen when processed totals do not match expected totals?
- Which edge cases are common enough for the MVP: refunds, installments, duplicate rows, fees, foreign currency, or negative amounts?
- What would make the project persuasive to a backend interviewer?

## Unresolved decisions
- First supported input file shape.
- Required and optional transaction fields.
- Initial category taxonomy.
- Expected total input behavior.
- Treatment of refunds in category totals.
- Treatment of duplicate-looking transactions.
- Treatment of installments.
- MVP report format.
- Review workflow format.
- Acceptance criteria for "first useful version."
- Synthetic demo dataset shape.
- When AI assistance should enter after the deterministic workflow is validated.
