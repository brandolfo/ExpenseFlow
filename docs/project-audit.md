# Project Audit

> Historical planning audit: this document was written during the pre-architecture/pre-MVP phase. Some findings are intentionally outdated because the deterministic CSV MVP and scoped synthetic PDF ingestion phase have since been completed. Current project status lives in `README.md`; accepted decisions live in `docs/decisions.md`; the current architecture summary lives in `docs/architecture-summary.md`; and current PDF scope lives in `docs/pdf-ingestion-plan.md`.

## Executive summary
ExpenseFlow is on the right track. The project has a clear product thesis, strong safety constraints, and a useful backend-centered MVP direction: transform messy structured transaction exports into categorized, validated, auditable reports.

The main risk is no longer lack of vision. The main risk is staying in documentation mode too long while several practical MVP decisions remain unresolved. Before backend architecture begins, the project should define the first supported input shape, report output shape, synthetic demo dataset, deterministic rule examples, and acceptance tests.

Current recommendation: do not begin backend architecture yet. Run one more product-to-test narrowing pass, then architecture can start with much better footing.

## What is strong
- The project avoids the generic CRUD trap by centering file processing, categorization, validation, reporting, and auditability.
- The MVP is correctly framed as a deterministic file-to-report workflow without AI.
- The AI design is responsible: AI is later, structured, auditable, confidence-aware, and review-first.
- The domain model has strong trust rules: no silent row drops, deterministic totals, explicit review states, and audit trails.
- The first user is clear: Santiago, a backend developer who wants both a personal tool and a portfolio-grade project.
- The portfolio angle is credible because the project can demonstrate parsing, normalization, rules, validation, edge cases, testing, and auditability.
- The agent system is useful as a product-thinking tool rather than a gimmick.
- Real financial data is consistently prohibited from public examples.

## What is weak
- The MVP still lacks a concrete input contract.
- The MVP still lacks a concrete report output contract.
- The synthetic demo dataset has not been designed yet.
- Acceptance tests have not been written as executable behavior expectations.
- The initial category taxonomy exists, but it has not been validated against a demo dataset or user workflow.
- Refunds, transfers, installments, duplicate-looking rows, invalid rows, and excluded-from-total behavior are identified but not resolved enough for implementation.
- Manual review is repeatedly described as important, but the MVP does not yet say what the user can actually do during review.
- The README is now slightly stale because it still says the repository is in "product discovery and agent setup" even though MVP, domain, and AI design docs now exist.
- The docs are getting detailed enough that the project could start feeling heavier than the MVP itself.

## Agent findings

### Founder Agent

#### Contradictions
- The project says it is still in product discovery, but it has already made MVP, domain, taxonomy, and AI design decisions.
- The MVP says keep scope small, while the documentation set already includes fairly advanced AI behavior.

#### Premature decisions
- AI design may be too detailed before the deterministic MVP input/output contract exists.
- The initial taxonomy may be more settled than the current validation evidence supports.

#### Missing decisions
- Which one workflow is the first demo story?
- What exact demo output proves backend value?
- What must be done before architecture can start?

#### Scope creep
- AI, correction history, audit depth, future reporting, and portfolio positioning could pull attention away from building the core deterministic workflow.

#### Weak assumptions
- "Backend-first can be compelling" remains true but unvalidated.
- "Synthetic data is enough" is plausible, but the dataset does not exist yet.

#### Unclear MVP boundaries
- Manual correction is domain-important but not clearly in or out of the MVP.
- Reporting is in the MVP, but advanced exports are in a later phase.

#### Risks for portfolio value
- Too much documentation and no working demo could read as planning rather than engineering.
- If the first demo is not crisp, the product may seem abstract.

#### Risks for implementation speed
- Starting architecture before input/output examples will create churn.
- Trying to solve all edge cases at once will delay the first useful version.

### Product Manager Agent

#### Contradictions
- Product discovery lists the initial category taxonomy as unresolved, while the decision log now accepts an initial category taxonomy.
- The roadmap places "Reporting" in Phase 4, but the MVP already requires a structured report.

#### Premature decisions
- The category list is useful, but may be premature without synthetic sample transactions.
- The AI-assisted workflow is designed before the first deterministic workflow is validated.

#### Missing decisions
- First supported input format and required fields.
- MVP report sections and field-level output.
- Expected total behavior when missing.
- What the MVP means by "manual review."
- What "done" means for first useful version.

#### Scope creep
- Future users, AI assistance, correction history, and reporting exports could distract from Santiago's first workflow.

#### Weak assumptions
- The current manual workflow is inferred, not confirmed by an interview.
- Category usefulness is assumed, not validated.

#### Unclear MVP boundaries
- The MVP says "one supported transaction file shape" but that shape is not defined.
- The MVP says "clear handling of invalid rows" but invalid-row report behavior is not yet specific.

#### Risks for portfolio value
- Without acceptance criteria tied to a concrete dataset, the demo may be hard to explain.

#### Risks for implementation speed
- Ambiguous requirements will cause architecture and test design rework.

### Domain Expert Agent

#### Contradictions
- "Uncategorized review" appears in the category taxonomy, but the domain model also says it is not a trusted final category.
- Manual correction is part of the domain, but correction history may be deferred; this is acceptable, but needs clearer MVP language.

#### Premature decisions
- The taxonomy includes categories such as Travel and Education before there is evidence they appear in the first dataset.
- The domain model discusses transaction states such as excluded from totals before the exclusion rules are defined.

#### Missing decisions
- Refund behavior.
- Transfer/payment behavior.
- Installment behavior.
- Duplicate-looking transaction behavior.
- Negative amount behavior.
- Whether invalid rows affect processed totals.

#### Scope creep
- Modeling every financial edge case can turn the MVP into a full personal finance rules engine.

#### Weak assumptions
- Known merchant rules are assumed to be enough for first value, but no first rule set exists.
- Expected total validation is assumed important, but the source of the expected total is not defined.

#### Unclear MVP boundaries
- It is unclear which edge cases must be fully handled versus only surfaced for review.

#### Risks for portfolio value
- If domain rules remain vague, the backend may look like generic parsing plus labels instead of real business logic.

#### Risks for implementation speed
- Unresolved financial treatment rules can block tests and report logic.

### AI Architect Agent

#### Contradictions
- AI is out of the MVP, but AI design is already extensive. This is not wrong, but it should stay explicitly future-facing.
- High-confidence AI "may be allowed as a suggestion" while the project strongly says no full automation without review. The language should remain conservative until review policy is implemented.

#### Premature decisions
- Structured AI output is detailed before implementation architecture or provider selection. This is acceptable as a guardrail, but should not drive architecture prematurely.
- Prompt/version audit requirements may be too detailed for pre-MVP.

#### Missing decisions
- When AI becomes eligible to enter the roadmap.
- Whether AI will be behind a feature flag or optional workflow later.
- What exact human review threshold accepts or rejects AI suggestions.

#### Scope creep
- AI summaries, merchant normalization, rule recommendations, and taxonomy suggestions could expand the product before deterministic value exists.

#### Weak assumptions
- AI will help classification enough to justify cost and privacy exposure. This should be validated after deterministic review items exist.

#### Unclear MVP boundaries
- MVP excludes AI, but docs should keep AI references from leaking into MVP acceptance tests.

#### Risks for portfolio value
- If AI receives too much attention, reviewers may misread the project as an AI wrapper.

#### Risks for implementation speed
- Designing for AI provider abstractions too early could complicate the first architecture.

### Backend Architect Agent

#### Contradictions
- AGENTS.md says "prefer modular monolith" when implementation begins, while other docs say not to choose architecture yet. This is mostly compatible, but should be treated as a future default, not a current decision.

#### Premature decisions
- Mentions of Swagger/API docs, async workflows, clean architecture, and modular monolith are directionally reasonable but should not become architecture before product contracts are nailed down.

#### Missing decisions
- Input contract.
- Output contract.
- Report artifact shape.
- Validation behavior for each row state.
- Acceptance tests.
- Error and warning language.
- Synthetic dataset.

#### Scope creep
- Auditability, async processing, AI, correction history, and reporting exports could push the design beyond a first backend implementation.

#### Weak assumptions
- The first workflow may not require async processing, but async workflows are already part of the portfolio promise.

#### Unclear MVP boundaries
- It is unclear whether the first implementation should be a CLI, API, local service, or documented behavior. This can wait, but not much longer.

#### Risks for portfolio value
- Architecture without concrete examples could look overengineered.

#### Risks for implementation speed
- Starting architecture now would force guesses about contracts and test cases.

### QA Agent

#### Contradictions
- The MVP says acceptance criteria exist, but many criteria still refer to "once that rule is decided."
- The domain model includes example totals, but notes that totals are illustrative; that is safe, but acceptance tests will need deterministic expected values.

#### Premature decisions
- None severe. QA mostly sees the current docs as useful pre-test material.

#### Missing decisions
- Test dataset.
- Expected outputs for each synthetic transaction.
- Expected total calculations.
- Handling of empty file, missing fields, invalid dates, invalid amounts, duplicates, refunds, transfers, and category conflicts.
- Priority of tests for MVP versus future.

#### Scope creep
- Testing AI failure modes before deterministic workflow tests could distract from the MVP.

#### Weak assumptions
- Manual review being acceptable is untested.
- Category accuracy expectations are not quantified.

#### Unclear MVP boundaries
- It is unclear whether manual correction is tested in MVP or deferred.

#### Risks for portfolio value
- Without a strong acceptance-test document, the project may not visibly demonstrate testing discipline.

#### Risks for implementation speed
- Missing expected outputs will slow implementation and create ambiguity.

### Technical Writer Agent

#### Contradictions
- README is behind the current state of the project.
- Roadmap phase names can be misread because "Reporting" is later while MVP already includes a report.

#### Premature decisions
- The docs are polished enough that they may imply more certainty than exists.

#### Missing decisions
- A concise demo narrative.
- A "current status" section that distinguishes decided, unresolved, and future.
- A documentation index so readers know which docs matter now.

#### Scope creep
- Multiple agent and skill docs are useful internally, but public readers may find the repo heavy unless README guides them.

#### Weak assumptions
- Recruiters or interviewers will read long docs. The eventual portfolio presentation needs a shorter path.

#### Unclear MVP boundaries
- Public-facing docs should separate MVP, future AI, and future architecture more strongly.

#### Risks for portfolio value
- The project could look like planning without delivery until a demo dataset and executable workflow exist.

#### Risks for implementation speed
- More docs without a build gate can delay architecture and implementation.

## Contradictions found
- Product discovery says the initial category taxonomy is unresolved, but the decision log accepts an initial category taxonomy.
- Roadmap says reporting is Phase 4, but MVP requires a structured expense report.
- README says the repository is in "product discovery and agent setup," but the repo now has MVP scope, domain model, and AI design.
- "Uncategorized review" is listed as a category while also described as not a trusted final category.
- MVP acceptance criteria mention some behaviors that are still undecided, such as refunds and negative amounts.
- AI is excluded from the MVP, but AI design is detailed enough that readers could think it is near-term unless the docs keep it clearly future-facing.

## Premature decisions found
- Initial category taxonomy before a synthetic dataset and user workflow validation.
- Detailed AI structured output before deterministic MVP acceptance tests.
- Portfolio assumptions involving Swagger/API docs before deciding the first product interface.
- Async workflow as a portfolio value before proving the first workflow needs it.
- "Prefer modular monolith" is acceptable as a future default, but should not become an architecture decision yet.

## Missing decisions
- First supported input format.
- Required input columns.
- Optional input columns.
- First synthetic demo dataset.
- Expected output report shape.
- First deterministic merchant rules.
- First category taxonomy validation.
- Refund handling.
- Transfer/payment handling.
- Installment handling.
- Duplicate-looking transaction handling.
- Invalid row behavior in report totals.
- Expected total behavior when absent.
- Manual review behavior in MVP.
- Whether manual correction is in MVP or Phase 2.
- Acceptance tests for the first workflow.
- Demo story and success path.
- Documentation index or reader path.

## Scope creep risks
- Adding AI before deterministic processing is built.
- Solving PDF parsing before structured exports.
- Modeling too many financial edge cases before the first demo.
- Building dashboards or user accounts before report generation.
- Treating correction history as required for MVP.
- Designing async/background processing before proving the workflow requires it.
- Expanding the taxonomy before validating a small category set.

## Documentation bloat risks
- The docs are becoming more detailed than the current product evidence.
- Future AI content may distract from deterministic MVP work.
- Agent and skill files are useful for collaboration, but public readers need a guided path.
- Decision logs are helpful, but unresolved decisions need a sharper next-step document.
- Too many conceptual docs without synthetic examples could slow implementation.

## Scores

| Area | Score | Rationale |
| --- | ---: | --- |
| MVP clarity | 7/10 | The core workflow is clear, but input/output contracts and several edge-case behaviors are unresolved. |
| Portfolio value | 8/10 | The direction strongly demonstrates backend value, but it needs a concrete demo dataset and acceptance tests to become convincing. |
| Implementation readiness | 5/10 | Architecture should wait until the first input shape, report shape, deterministic rules, and acceptance tests are defined. |

## Recommended changes before architecture
1. Create `docs/input-output-contract.md` defining the first supported transaction file shape, required fields, optional fields, row statuses, report sections, and expected total behavior.
2. Create `docs/demo-dataset-design.md` with synthetic transactions and expected processing results.
3. Create `docs/acceptance-tests.md` covering the MVP happy path and core edge cases.
4. Decide whether manual correction is excluded from MVP or included in a minimal form.
5. Decide MVP behavior for refunds, transfers, installments, duplicate-looking rows, invalid rows, and missing expected total.
6. Clarify that "Uncategorized review" is a review state, not a trusted final category.
7. Update README current phase and add a short documentation reading path.
8. Adjust roadmap wording so MVP reporting and later export/reporting work are not confused.
9. Keep backend architecture blocked until the input/output contract and acceptance tests are accepted.

## Decisions that should be added to docs/decisions.md
Clearly supported now:
- Backend architecture should not begin until the MVP input/output contract, synthetic demo dataset, and acceptance tests are defined.

Not ready to add yet:
- First input format.
- Required columns.
- Report output shape.
- Refund behavior.
- Transfer behavior.
- Duplicate handling.
- Manual correction scope.
- Architecture style.
- Database choice.
- Endpoint design.
- AI provider or implementation approach.

## Audit conclusion
ExpenseFlow is directionally strong and worth continuing. The product has a meaningful backend core and a responsible AI boundary. The next move should be a narrowing move, not an architecture move: turn the MVP into concrete input examples, output examples, and acceptance tests.

Once those are done, backend architecture can begin with much less guessing.
