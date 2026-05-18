# MVP Scope

## 1. MVP objective
Define the first useful version of ExpenseFlow as a backend-focused product that transforms one structured expense transaction file into a categorized, validated, auditable expense report.

The MVP should prove that ExpenseFlow can:
- Account for every transaction in an input file.
- Categorize known patterns with deterministic rules.
- Flag uncertain or invalid transactions for manual review.
- Calculate totals deterministically.
- Validate processed totals against an expected total when available.
- Produce a useful report that demonstrates backend value without becoming a generic CRUD app.

## 2. Primary user
The primary user is Santiago, a backend developer from Argentina who wants:
- A practical personal tool for understanding expenses.
- A portfolio-grade backend project.
- A responsible agent-assisted product where AI supports interpretation later, but does not replace deterministic financial logic.

## 3. User problem
Santiago can export expense transactions, but the resulting files are messy or inconvenient to analyze manually. He needs to clean rows, interpret merchant descriptions, assign categories, verify totals, and summarize spending. This work is repetitive, easy to do inconsistently, and difficult to audit.

The product problem is not simply storing expenses. The product problem is turning messy transaction exports into structured, trustworthy reports.

## 4. First valuable workflow
1. The user provides one structured transaction export using synthetic demo data for public examples or private local data outside version control.
2. The user optionally provides an expected total from the source statement or export.
3. ExpenseFlow processes every row and reports whether each row is valid, categorized, invalid, or requires review.
4. Known merchant or description patterns are categorized deterministically.
5. Unknown, conflicting, or ambiguous transactions are marked for manual review.
6. Totals are calculated deterministically from processed transactions.
7. Processed totals are compared against the expected total when one is available.
8. The user receives a structured report with processing counts, category totals, validation status, and review items.
9. The report preserves enough detail to explain how each transaction was handled.

## 5. User stories
- As Santiago, I want to provide a transaction export so I can avoid manually rebuilding the same expense summary every month.
- As Santiago, I want known merchants to be categorized consistently so repeated expenses are handled the same way each time.
- As Santiago, I want uncertain transactions to be flagged instead of guessed so I can review them before trusting the report.
- As Santiago, I want totals to be calculated by the system deterministically so I can trust that AI did not invent financial numbers.
- As Santiago, I want to compare processed totals against an expected total so I can detect missing, duplicated, or invalid transactions.
- As Santiago, I want a clear summary by category so I can understand where money went.
- As Santiago, I want processing counts and review items so I can verify that every transaction was accounted for.
- As Santiago, I want public demo data to be synthetic so the portfolio project does not expose real financial information.
- As Santiago, I want the product scope to remain small enough to finish so it becomes a real portfolio artifact, not an abandoned architecture exercise.

## 6. Acceptance criteria
- Given a supported structured transaction file, the system accounts for every row in the processing result.
- Given a transaction with required fields present and valid, the transaction can be included in deterministic totals.
- Given a transaction that matches a known merchant or description pattern, the system assigns the expected category using deterministic rules.
- Given a transaction that does not match a known pattern, the system marks it as requiring manual review.
- Given a transaction with missing or invalid required data, the system marks it as invalid and includes it in the report instead of silently dropping it.
- Given an expected total, the system calculates the processed total deterministically and reports whether the totals match.
- Given a total mismatch, the system reports the mismatch clearly and keeps enough detail for investigation.
- Given a file with duplicate-looking transactions, the system surfaces them as potential duplicates or review items.
- Given refunds or negative amounts, the system handles them consistently according to the documented MVP rule once that rule is decided.
- Given the MVP report, the user can see processing counts, category totals, validation status, and transactions requiring review.
- Given public repository examples, all data is synthetic and contains no real financial data, full card numbers, bank account numbers, addresses, tax IDs, or personal identifiers.
- Given any AI capability proposed during MVP implementation, it is rejected unless it is explicitly deferred as a later feature.

## 7. Non-goals
The MVP will not include:
- PDF parsing.
- Real financial data in public files.
- Bank integrations.
- Automatic account syncing.
- User accounts, authentication, or multi-user collaboration.
- Budgets, alerts, forecasting, or financial advice.
- Dashboards or polished frontend screens.
- Payment provider integrations.
- Full automation for ambiguous transactions.
- AI-generated totals.
- AI-based deterministic validation.
- AI categorization in the first workflow.
- Complex tax or accounting compliance.
- Framework, database, endpoint, entity, or library decisions in this document.

## 8. Risks
- Scope creep: The MVP may expand into dashboards, account management, PDF parsing, or AI features before the core workflow works.
- Generic CRUD drift: The product may become an expense record manager instead of a file-to-report processing system.
- False confidence: Incorrect deterministic rules may categorize transactions in a way that looks trustworthy but is wrong.
- Weak trust: If totals, invalid rows, and review items are unclear, users may not trust the report.
- Real data exposure: Personal financial data could accidentally enter public examples.
- Edge case overload: Refunds, installments, duplicate rows, and inconsistent formats could make the MVP too large.
- Portfolio weakness: If the demo does not clearly show processing, validation, auditability, and testing potential, it may not demonstrate backend skill.
- AI misuse: Adding AI too early could make the project look like a wrapper instead of a serious backend system.

## 9. Edge cases
The MVP should explicitly consider these cases, even if some are handled as review items rather than fully resolved:
- Missing required fields.
- Empty files.
- Invalid dates.
- Invalid amounts.
- Amounts with negative values.
- Refunds or reversals.
- Duplicate-looking transactions.
- Installment descriptions.
- Fees or taxes mixed into regular spending.
- Unknown merchants.
- Merchant descriptions with abbreviations or noisy text.
- Multiple transactions with the same amount and date.
- Category conflicts where more than one deterministic rule could match.
- Expected total missing.
- Expected total mismatch.
- Rows that cannot be safely processed.
- Files using synthetic but realistic demo transactions.

## 10. Manual review expectations
Manual review is part of the MVP's trust model, not a failure state.

Transactions should require manual review when:
- No deterministic category rule matches.
- Multiple category rules conflict.
- Required fields are present but suspicious.
- The transaction resembles a duplicate.
- The transaction appears to be a refund, reversal, installment, fee, or adjustment and the MVP rule is not yet confident enough.
- A row is valid enough to inspect but not safe enough to include silently in category conclusions.

The report should make review items visible with a reason. The MVP does not need to implement correction history yet, but it should define review as a first-class product concept so Phase 2 can add correction and audit history.

## 11. What must be deterministic
The MVP must use deterministic behavior for:
- Financial total calculation.
- Expected total vs processed total validation.
- Required field validation.
- Invalid row detection.
- Processing counts.
- Known merchant or description categorization rules.
- Category total calculation.
- Detection and reporting of unmatched transactions.
- Detection and reporting of category rule conflicts.
- Audit information that explains how a transaction was processed.

AI must not calculate totals, validate totals, decide whether rows were processed correctly, or silently finalize ambiguous classifications.

## 12. What may use AI later
AI may be considered after the deterministic workflow is useful and validated.

Later AI assistance may include:
- Suggesting categories for ambiguous transactions.
- Explaining why a transaction might belong to a category.
- Summarizing spending patterns in plain language.
- Recommending new deterministic rules based on repeated manual review decisions.
- Highlighting unusual merchant descriptions for review.
- Drafting report summaries for human inspection.

Any future AI output must be structured, traceable, confidence-aware, and safe to reject. Low-confidence or invalid AI output must require review.

## 13. Success criteria for the MVP
The MVP is successful when:
- Santiago can run one end-to-end file-to-report workflow using synthetic demo data.
- Every transaction row is accounted for as categorized, invalid, or requiring review.
- Known merchant patterns are categorized consistently.
- Ambiguous transactions are visible and not silently guessed.
- Totals and category totals are calculated deterministically.
- Expected total validation makes missing or mismatched data obvious.
- The report is useful enough for Santiago to understand expense distribution without rebuilding the summary manually.
- The workflow is small enough to build, test, document, and demo.
- The product clearly demonstrates backend skill: data processing, validation, deterministic rules, auditability, edge case handling, and testable behavior.
- The MVP creates a solid foundation for later review correction, reporting, AI assistance, and portfolio presentation.
