# ExpenseFlow Codex Prompting Standard v2

Date: 2026-05-20

## Purpose

This standard defines how to write future Codex prompts for ExpenseFlow milestones, checkpoints, and small changes.

The priority is not short prompts. The priority is giving Codex enough precise, current, decision-aware context to produce scoped, testable, safe, architecture-aligned output.

Use this document for future work planning and implementation prompts. It is a prompting standard for Codex collaboration, not a runtime application design.

## Current Prompt Strategy Evaluation

The current milestone prompts are working well. Their length is justified because ExpenseFlow has strong product, architecture, privacy, testing, and scope constraints.

What is working:

- Prompts force Codex to inspect the repository before acting.
- They identify source-of-truth documents and reduce stale-context mistakes.
- They repeat non-goals that protect the project from scope creep.
- They distinguish implemented behavior from future work.
- They keep AI, OCR, persistence, auth, frontend, Docker/cloud, and arbitrary PDF support out unless explicitly scoped.
- They use role lenses to improve planning and self-review without implying runtime agents.
- They require tests, documentation alignment, self-review, and safe-to-commit guidance.
- They make milestone boundaries clear enough for small, reviewable changes.

What should remain unchanged:

- Read-from-disk first.
- Explicit source-of-truth files.
- Clear goal, scope, non-goals, tasks, tests, docs, self-review, and final output requirements.
- Repeated high-risk non-goals for major prompts.
- Role lenses for planning/review on meaningful product, architecture, QA, security, documentation, or AI-boundary work.
- Explicit final instructions that say what changed, what was verified, and whether it is safe to commit.

What should improve:

- Add an explicit Decision Authority section to milestone prompts.
- Add Expected Diff and Forbidden Diff sections for implementation prompts.
- Make the Stop Condition explicit.
- State accepted decisions separately from suggestions or historical docs.
- Use a clear source-of-truth priority order when documents conflict.
- Require a short pre-edit plan before full milestone edits.
- Use shorter checkpoint and micro-change prompts when the task is narrow.
- Use role lenses and skills only when they materially reduce risk.

## Prompt Sizes

### Full Milestone Prompt

Use for:

- New feature milestones.
- Architecture-affecting changes.
- Public API changes.
- New parser/extractor/normalizer work.
- Privacy-sensitive work.
- Any change touching financial logic, PDF ingestion, AI boundaries, or release gates.

Recommended intelligence level: High.

Expected detail: long and explicit. Repetition is acceptable when it prevents scope creep.

Required sections:

- Mission / Purpose
- Source of truth
- Source-of-truth priority
- Agent usage clarification
- Role lenses
- Skills
- Accepted decisions
- Scope
- Non-goals
- Decision authority
- Implementation tasks
- Testing requirements
- Documentation requirements
- Expected diff
- Forbidden diff
- Commands
- Pre-edit plan
- Blocker protocol
- Self-review
- Definition of done
- Final output
- Stop condition

### Verification / Checkpoint Prompt

Use for:

- Reviewing current state.
- Checking release readiness.
- Verifying docs/tests after a milestone.
- Auditing scope alignment.
- Investigating a risk without changing code.

Recommended intelligence level: Medium, High if the checkpoint is broad or high-risk.

Expected detail: moderate. Include enough source-of-truth and non-goals to prevent accidental expansion.

Required sections:

- Mission / Purpose
- Source of truth
- Scope
- Non-goals
- Commands
- Self-review
- Final output
- Stop condition

Optional sections:

- Role lenses
- Skills
- Expected diff / Forbidden diff, especially if documentation cleanup is allowed

### Micro-Change Prompt

Use for:

- Typo fixes.
- Marking a doc historical.
- Updating one risk line.
- Small README/API example correction.
- Mechanical formatting with no behavior change.

Recommended intelligence level: Low or Medium.

Expected detail: short, but still explicit about forbidden scope.

Required sections:

- Source of truth, if relevant.
- Exact file or small area to change.
- Non-goals.
- Verification command.
- Final output.

Avoid:

- Full role-lens review.
- Broad source-reading lists.
- Large self-review blocks.
- Repeating every future non-goal unless the file touches that topic.

## Section Guidance

| Section | Standard | Why it helps Codex | When to use |
| --- | --- | --- | --- |
| Mission / Purpose | Keep | Anchors the task around product value instead of file edits. | Always for milestone/checkpoint prompts. |
| Source of truth | Keep | Reduces stale-memory and hallucinated-state risk. | Always. |
| Source-of-truth priority | Add | Resolves conflicts without guessing. | Full milestone prompts and conflict-prone checkpoints. |
| Agent usage clarification | Keep | Prevents role files from becoming imagined runtime architecture. | Any prompt mentioning agents. |
| Role lenses | Keep | Improves product, architecture, QA, security, docs, and AI-boundary review. | Meaningful planning/review work. |
| Skills | Keep | Reuses local project workflows and review heuristics. | When a local skill directly matches the task. |
| Accepted decisions | Add | Separates binding decisions from historical notes or suggestions. | Full milestone prompts and decision-sensitive checkpoints. |
| Scope | Keep | Defines what success includes. | Always. |
| Non-goals | Keep | Prevents scope creep and accidental feature expansion. | Always; compress for micro-changes. |
| Decision authority | Add | Controls what Codex may decide versus what requires new scope. | Standard for full milestone prompts. |
| Implementation tasks | Keep | Makes the expected work concrete and reviewable. | Implementation or doc-edit prompts. |
| Testing requirements | Keep | Keeps behavior guarded and docs/tests aligned. | Any code or behavior-facing doc prompt. |
| Documentation requirements | Keep | Prevents docs from lagging implementation. | Feature, API, behavior, or roadmap changes. |
| Expected diff | Add | Helps Codex constrain edits to likely files. | Full milestone prompts and risky doc changes. |
| Forbidden diff | Add | Makes scope creep visible before it happens. | Full milestone prompts, high-risk checkpoints. |
| Commands | Keep | Standardizes verification and avoids unnecessary build/test runs. | Always. |
| Pre-edit plan | Add | Lets Codex confirm intended edits and expected diff before changing files. | Full milestone prompts. |
| Blocker protocol | Add | Prevents Codex from inventing missing decisions. | Full milestone prompts and decision-sensitive work. |
| Self-review | Keep | Catches scope leaks, missing tests, stale docs, and boundary mistakes. | Full milestone/checkpoint prompts. |
| Definition of done | Add | Gives Codex a concrete completion checklist. | Full milestone prompts. |
| Final output | Keep | Ensures useful handoff and commit guidance. | Always. |
| Stop condition | Add | Prevents Codex from continuing into adjacent work. | Always. |

Avoid sections that ask Codex to invent product strategy, future architecture, providers, policies, or endpoints without accepted scope.

## Source-of-Truth Priority

When project sources conflict, Codex should use this order:

1. Active prompt instructions.
2. `docs/decisions.md` accepted decisions.
3. `README.md` current project status.
4. `docs/architecture-summary.md`.
5. Current feature scope docs, such as `docs/pdf-ingestion-plan.md`.
6. `AGENTS.md`.
7. Historical planning docs as background only.

If the conflict affects implementation scope or behavior, Codex should name the conflict and follow the highest-priority source.

## Decision Authority

Decision Authority should become standard for full milestone prompts.

Codex may decide implementation details that are local, reversible, and consistent with the codebase:

- Class, method, and test names.
- Small folder placement details.
- Private helper shapes.
- Minimal internal DTOs when the contract is not fixed.
- Test organization that matches existing patterns.
- Small refactors needed to keep the change clean.

Codex must not decide new product or architecture direction:

- New product scope.
- New external providers.
- New public endpoint shape unless specified.
- New currency policy.
- New privacy or retention policy.
- New persistence strategy.
- New auth/user model.
- New OCR behavior.
- New AI/LLM behavior.
- New runtime multi-agent architecture.
- New supported PDF variants.

If a task exposes one of those missing decisions, Codex should stop, document the blocker, and recommend the smallest decision needed before implementation.

## Blocker Protocol

If implementation requires a decision outside Codex's authority, Codex must stop before editing further.

It should report:

- What the blocker is.
- Why it blocks the milestone.
- Which source or missing decision caused the blocker.
- The smallest decision needed to continue.

Codex must not invent missing product, architecture, privacy, provider, currency, endpoint, persistence, or AI policy.

## Pre-Edit Plan

For full milestone prompts, Codex should briefly summarize its implementation plan and expected diff before editing files.

The plan should include:

- The intended approach.
- The files or areas likely to change.
- The tests or commands expected.
- Any assumptions or possible blockers.

Keep this concise. The goal is alignment before edits, not a second design document.

## Definition of Done

A milestone is done only when:

- Requested scope is completed.
- Accepted decisions are preserved.
- Forbidden scope is avoided.
- Tests are added or updated at the right level.
- Docs are updated or intentionally left unchanged.
- Architecture, privacy, AI, and runtime-agent boundaries are verified.
- Required commands pass.
- Safe-to-commit output is provided.

## Expected Diff / Forbidden Diff

Expected Diff and Forbidden Diff should become standard for full milestone prompts.

Expected Diff should name likely files, folders, project areas, or artifact types:

```text
Expected diff:
- docs/risk-register.md
- docs/decisions.md, only if a new decision is accepted
- backend/tests/ExpenseFlow.IntegrationTests/*Pdf*
```

Forbidden Diff should identify changes that would indicate scope creep:

```text
Forbidden diff:
- No backend source changes.
- No new package references.
- No new endpoint.
- No changes under backend/tools unless fixture generation is in scope.
- No real/private statement samples.
- No OpenAI, OCR, database, auth, frontend, Docker/cloud, or runtime agent files.
```

This is especially valuable for ExpenseFlow because the project has many tempting adjacent features.

## Repetition Guidance

Repeated non-goals are beneficial in full milestone prompts.

Keep repeating high-risk non-goals when a task is near that boundary:

- No OCR.
- No LLM integration.
- No external APIs.
- No database or persistence.
- No auth.
- No frontend.
- No Docker/cloud.
- No arbitrary PDF support.
- No real/private statement processing.
- No runtime multi-agent architecture.

Why repetition helps:

- It reduces model drift in long prompts.
- It guards against plausible but unwanted "helpful" additions.
- It keeps scope clear across milestones.
- It protects privacy and architecture boundaries.

When to reduce repetition:

- Micro-changes unrelated to those risks.
- Pure Git/tag operations.
- Narrow doc edits where the forbidden area is obvious.

Avoid contradictions. If a historical doc says something different from current scope, name the current source of truth.

## Role Lenses and Skills

Use role files as review and planning lenses only.

Do:

- Say which role lenses should be used and why.
- Use Product Manager Agent for scope and MVP/future separation.
- Use Backend Architect Agent for boundaries, module responsibilities, and overengineering review.
- Use QA Agent for test coverage, failure modes, and no-silent-row-loss.
- Use Security Agent for real data, secrets, privacy, logging, and provider risk.
- Use Technical Writer Agent for docs, roadmap, README, and portfolio clarity.
- Use AI Architect Agent for AI boundaries, structured output, guardrails, and non-use cases.

Do not:

- Claim ExpenseFlow has runtime agents.
- Add application-level AI agents unless separately scoped.
- Use every role for mechanical edits.
- Let role lenses override accepted decisions.

Use skills when they directly fit the task:

- Backend Architecture Review Skill: architecture, boundaries, maintainability, testability.
- Test Case Generation Skill: acceptance tests, edge cases, fixture coverage, regression risks.
- Portfolio Positioning Skill: README, demo, interview, public project story.
- AI Agent Design Skill: future AI boundaries and guardrails, not deterministic MVP logic.

Do not use skills as ceremony for formatting, dependency bumps, tag updates, or tiny wording edits.

## Self-Review Standard

Self-review should stay standard for full milestone and checkpoint prompts. It improves output quality because it forces Codex to evaluate the diff through the same risk lenses used to create the milestone.

Recommended self-review questions:

1. Does this satisfy the exact requested scope?
2. Did it preserve accepted decisions?
3. Did it avoid forbidden features and dependencies?
4. Did it keep deterministic financial logic deterministic?
5. Did it preserve architecture boundaries?
6. Did it protect privacy and synthetic-data rules?
7. Did it add or update tests at the right level?
8. Did it keep documentation aligned with implemented behavior?
9. Is the diff small and reviewable?
10. Is it safe to commit?

For AI-related prompts, add:

- Does this keep AI away from totals, deterministic validation, and known-rule categorization?
- Does this avoid sending real/private financial data to external providers?
- Does this keep role lenses separate from runtime architecture?

## Full Milestone Prompt Template

```text
Recommended intelligence level: High.

Before continuing, inspect the repository from disk and treat it as the source of truth.

Read:
- AGENTS.md
- README.md
- docs/decisions.md
- docs/architecture-summary.md
- [feature-specific docs]
- backend/src/[relevant projects]
- backend/tests/

Agent usage clarification:
Use agent files as role lenses for planning and review.
Do not design or claim a runtime multi-agent architecture.
Do not add application-level AI agents.

Use these role definitions explicitly:
- Product Manager Agent: [why]
- Backend Architect Agent: [why]
- QA Agent: [why]
- Security Agent: [why]
- Technical Writer Agent: [why]
- AI Architect Agent: [only if AI boundary is relevant]

Apply these skills explicitly if relevant:
- Backend Architecture Review Skill
- Test Case Generation Skill
- Portfolio Positioning Skill
- AI Agent Design Skill

Mission / Purpose:
[One paragraph explaining the milestone and product value.]

Accepted decisions:
- [Binding decisions from docs/decisions.md or current source-of-truth docs.]

Scope:
- [What this milestone must do.]

Non-goals:
- [High-risk exclusions.]

Decision authority:
Codex may decide:
- [Local implementation choices.]

Codex may not decide:
- [Product/architecture/provider/privacy/policy choices.]

Implementation tasks:
1. [Task]
2. [Task]
3. [Task]

Testing requirements:
- [Unit/integration/fixture/release-gate expectations.]

Documentation requirements:
- [Docs that must be updated or intentionally left unchanged.]

Expected diff:
- [Likely files/folders.]

Forbidden diff:
- [Files/features/dependencies that must not change.]

Commands:
- [Required verification commands.]
- Do not run [expensive/unneeded commands] unless [condition].

Pre-edit plan:
- Briefly summarize the intended implementation plan and expected diff before editing files.

Blocker protocol:
- If implementation requires a decision outside Codex's authority, stop and report the blocker, why it blocks, and the smallest decision needed.

Self-review:
Before finishing, review with the requested role lenses:
1. Scope
2. Architecture boundaries
3. Tests
4. Privacy/security
5. Docs
6. AI/runtime-agent boundaries

Definition of done:
- Requested scope completed
- Accepted decisions preserved
- Forbidden scope avoided
- Tests added/updated
- Docs updated or intentionally unchanged
- Boundaries verified
- Required commands passed
- Safe-to-commit output provided

Final output:
- Files changed
- Tests/commands run
- Docs updated or intentionally unchanged
- Risks/assumptions
- Whether it is safe to commit
- Exact git commands to commit

Stop condition:
Stop after this milestone. Do not continue into adjacent features.
```

## Verification / Checkpoint Prompt Template

```text
Recommended intelligence level: Medium. Use High for broad release checks.

Before continuing, inspect the repository from disk and treat it as the source of truth.

Read:
- AGENTS.md
- README.md
- docs/decisions.md
- docs/architecture-summary.md
- [checkpoint-specific docs/files]

Goal:
Verify [release readiness / documentation alignment / architecture boundary / test coverage].

Scope:
- Review only.
- Documentation edits allowed: [yes/no].
- Code edits allowed: [yes/no].

Non-goals:
- Do not add product behavior.
- Do not change backend source unless explicitly allowed.
- Do not add new dependencies, endpoints, providers, OCR, LLM, persistence, auth, frontend, Docker/cloud, or runtime agents.

Commands:
- [git diff --check, dotnet test, targeted command, or no build/test condition]

Self-review:
- Are findings grounded in files?
- Are stale docs distinguished from current source of truth?
- Are risks and assumptions explicit?

Final output:
- Findings
- Commands run
- Files changed, if any
- Safe-to-commit status

Stop condition:
Stop after the checkpoint.
```

## Micro-Change Prompt Template

```text
Recommended intelligence level: Low or Medium.

Task:
[Exact small change.]

Source of truth:
- [Specific file or doc, if needed.]

Scope:
- Change only [file/section].

Non-goals:
- No backend code changes.
- No behavior changes.
- No unrelated documentation rewrite.

Verify:
- Run git diff --check.
- Do not run dotnet build/test unless backend source/project files changed.

Final output:
- Files changed
- Verification result
- Safe-to-commit status
- Exact commit commands

Stop condition:
Stop after this change.
```

## Recommended Intelligence Levels

Use High for:

- New milestones.
- Architecture changes.
- Financial logic.
- Parser/extractor/normalizer work.
- PDF ingestion expansion.
- Privacy-sensitive work.
- AI-boundary work.
- Release gates.

Use Medium for:

- Documentation alignment.
- Test additions that follow existing patterns.
- Checkpoint reviews.
- Moderate refactors inside known boundaries.

Use Low for:

- Typos.
- One-file wording updates.
- Tag/Git checks.
- Mechanical edits with no design choice.

Do not lower the intelligence level just to make prompts shorter. Use the level that matches the risk of the task.

## Accepted Decisions vs Suggestions

Accepted decisions are binding. They come from:

- `docs/decisions.md`
- Current README status
- Current architecture summary
- Current feature scope docs
- Current AGENTS.md phase rules

Historical docs may contain useful reasoning but should not override current accepted decisions. If a prompt references a historical planning doc, state that clearly.

Suggestions, risks, and old audit findings should guide review but should not silently become new scope.

## Decision Log Update Rules

Update `docs/decisions.md` only for durable decisions about:

- Product scope.
- Architecture or module boundaries.
- Privacy, retention, or real-data handling.
- Testing or release gates.
- Libraries, providers, or external dependencies.
- Public endpoint shape.
- Deterministic processing boundaries.
- AI/OCR/provider policy.

Do not update `docs/decisions.md` for:

- Temporary implementation details.
- Test names.
- Private helper names.
- Formatting-only changes.
- Wording-only documentation edits.
- Mechanical refactors that do not change accepted behavior or boundaries.

## Final Output and Commit Guidance

Every prompt that allows edits should ask Codex to report:

- Files changed.
- Whether README was changed or intentionally left unchanged.
- Commands run and results.
- Tests not run and why.
- Role-lens self-review summary for non-trivial work.
- Whether it is safe to commit.
- Exact git commands to stage and commit.

Do not ask Codex to commit unless committing is explicitly desired. Most milestone prompts should stop at safe-to-commit guidance.

## Practical Recommendation

Keep full milestone prompts detailed. The detail is helping Codex produce better ExpenseFlow work.

The best upgrade is not shortening the prompts. The best upgrade is structuring them with:

- Decision Authority
- Expected Diff
- Forbidden Diff
- Stop Condition
- Clear separation between accepted decisions and historical suggestions

That gives Codex stronger control surfaces while preserving the scope discipline that has worked so far.
