# AI Agent Design

## Purpose
This document defines how AI may eventually be used inside ExpenseFlow. It is a product and behavior design document, not application code, architecture, database design, endpoint design, framework selection, or provider selection.

ExpenseFlow uses AI as an assistant for ambiguous interpretation. Deterministic processing remains responsible for totals, validation, rule execution, auditability, and report correctness.

## 1. AI principles
- AI assists with ambiguity; it does not replace deterministic business logic.
- AI must not calculate financial totals.
- AI must not validate financial correctness.
- AI must not decide whether every row was processed correctly.
- AI must not silently finalize ambiguous transactions.
- AI output must be structured.
- AI output must be auditable.
- AI output must support confidence and review.
- Known deterministic merchant rules take priority over AI suggestions.
- Low-confidence AI results require manual review.
- Invalid AI output must fail safely.
- AI failure must not block deterministic processing.
- Public examples must use synthetic data only.

## 2. Tasks that must never use AI
AI must never be used for:
- Calculating processed totals.
- Calculating category totals.
- Comparing expected totals to processed totals.
- Validating financial correctness.
- Deciding whether a source row was accounted for.
- Required field validation.
- Invalid row detection.
- File completeness validation.
- Applying known deterministic merchant rules.
- Resolving category rule conflicts as final truth without review.
- Silently excluding transactions from a report.
- Creating or modifying real financial data.
- Inventing facts about the user's financial behavior.
- Making financial advice, tax advice, or accounting compliance decisions.

## 3. Tasks that may use AI
AI may be considered later for:
- Suggesting categories for ambiguous transactions.
- Explaining why a transaction might fit a suggested category.
- Suggesting merchant normalization for noisy descriptions.
- Highlighting unusual or confusing descriptions for review.
- Drafting plain-language summaries of already-calculated report results.
- Recommending new deterministic rule candidates based on repeated manual corrections.
- Explaining validation warnings in user-friendly language without changing the validation result.

AI assistance must remain secondary to deterministic processing and human review.

## 4. First AI-assisted use case
The first AI-assisted use case should be ambiguous transaction categorization after the deterministic MVP workflow is working.

Use case:
- Input: Transactions already marked as review required because deterministic rules could not safely categorize them.
- Output: Structured category suggestions with confidence, rationale, and review requirements.
- Human role: Review, accept, reject, or correct the suggestion.
- System role: Preserve the suggestion and final human decision for auditability and future rule improvement.

This use case is appropriate because deterministic rules are not always enough for noisy merchant descriptions, but a human still needs control before uncertain suggestions affect trusted reporting.

## 5. Categorization agent responsibilities
The future categorization agent may:
- Read only the transaction fields provided to it.
- Suggest one category from the allowed taxonomy when appropriate.
- Mark a transaction as `needs_review` when confidence is low or context is insufficient.
- Explain the reason for the suggestion in concise language.
- Identify whether the suggested category is based on merchant text, description text, transaction type, or contextual clue.
- Return structured output that can be validated before use.
- Refuse to decide when the input is too ambiguous.
- Surface candidate rule ideas for repeated patterns without applying them automatically.

The categorization agent must not:
- Calculate totals.
- Change deterministic validation results.
- Override known merchant rules.
- Create categories outside the allowed taxonomy unless explicitly marking them as a suggested future category.
- Hide uncertainty.
- Decide that a transaction can skip manual review when confidence or policy requires review.

## 6. Inputs to the AI agent
The AI agent should receive the minimum information required for ambiguous classification.

Allowed inputs:
- Transaction identifier or source row reference.
- Transaction date if needed for context.
- Transaction description.
- Normalized merchant candidate, if available.
- Amount sign or amount band when helpful, but not for calculation.
- Source-provided transaction type, if available.
- Allowed category taxonomy.
- Existing deterministic rule result, especially `review_required`.
- Review reason from deterministic processing.
- User-approved category hints or prior manual correction patterns when available.

Restricted inputs:
- Full card numbers.
- Bank account numbers.
- Tax IDs.
- Addresses.
- Personal identifiers not required for categorization.
- Raw files when only a few fields are needed.
- Expected totals, processed totals, or category totals.

## 7. Required structured output
AI output must be structured and validate against the expected shape before it can be used.

Required fields:
- `transaction_ref`: Reference to the transaction or source row.
- `suggested_category`: One allowed category or `null`.
- `suggested_category_is_new`: Boolean indicating whether the model proposed a category outside the allowed taxonomy.
- `new_category_suggestion`: Proposed new category name, only when explicitly suggesting taxonomy improvement.
- `confidence`: Numeric or enumerated confidence value.
- `requires_manual_review`: Boolean.
- `rationale`: Short explanation grounded in the provided transaction data.
- `signals_used`: List of signals such as merchant text, description keyword, transaction type, or prior correction hint.
- `warnings`: List of concerns such as ambiguous merchant, possible duplicate, refund, transfer, installment, or insufficient context.
- `invalid_output_reason`: Present when the agent cannot produce a valid suggestion.

Rules:
- If `suggested_category_is_new` is true, `requires_manual_review` must also be true.
- If confidence is low, `requires_manual_review` must be true.
- If the suggestion conflicts with a deterministic rule, deterministic logic wins and the AI result must be review-only.
- If output is malformed, incomplete, or unsupported, it must be rejected safely.

## 8. Confidence rules
Confidence is a review signal, not proof of correctness.

Suggested confidence bands:
- High: The model sees a strong category signal in the provided description or merchant text. Human review may still be required depending on policy.
- Medium: The model sees a plausible category signal but ambiguity remains. Manual review is required.
- Low: The model lacks enough context, sees conflicting signals, or is making a weak guess. Manual review is required.
- None: The model cannot make a supported suggestion. Manual review is required.

Minimum behavior:
- Low confidence requires manual review.
- Medium confidence requires manual review for the first AI-assisted version.
- High confidence may be allowed as a suggestion, but should still be auditable and reversible.
- Confidence must never allow AI to calculate totals or validate correctness.

## 9. Manual review rules
Manual review is required when:
- AI confidence is medium, low, or none.
- AI suggests a new category outside the allowed taxonomy.
- AI conflicts with a deterministic rule.
- AI conflicts with a prior manual correction.
- The transaction is a possible refund, reversal, transfer, installment, fee, duplicate, or adjustment.
- The AI rationale is vague or not grounded in the provided fields.
- The AI output is malformed or missing required fields.
- The AI output includes unsupported claims.

Manual review should allow the user to:
- Accept the suggested category.
- Reject the suggestion.
- Select another existing category.
- Mark the transaction as not an expense, transfer, refund, duplicate, or other special treatment once those domain rules are defined.
- Add a note explaining the correction.

Manual correction is part of the domain and should be preserved for auditability and future deterministic rule improvement.

## 10. Guardrails
Guardrails must enforce:
- Deterministic processing completes independently of AI.
- AI receives only transactions already selected for AI assistance.
- AI receives the allowed taxonomy.
- AI output is validated before use.
- AI cannot override deterministic rules.
- AI cannot create final categories outside the allowed taxonomy.
- AI cannot calculate or reconcile totals.
- AI cannot suppress invalid rows or review items.
- AI cannot mark a transaction as processed if deterministic processing did not account for it.
- AI cannot access real financial data in public demos.
- AI suggestions must be traceable to inputs.
- Prompt and output versions should be auditable once implemented.

## 11. Failure modes
Expected AI failure modes include:
- Malformed structured output.
- Missing required output fields.
- Category outside allowed taxonomy.
- Overconfident wrong suggestion.
- Vague rationale.
- Suggestion unsupported by provided data.
- Conflict with deterministic rule.
- Conflict with prior correction.
- Low confidence for many transactions.
- Timeout or unavailable AI provider.
- Cost spike due to too many transactions sent to AI.
- Sensitive data accidentally included in AI input.
- Prompt injection text inside transaction descriptions.

QA expectation: every failure mode must fail safely and preserve deterministic report generation.

## 12. Fallback behavior
If AI is unavailable, invalid, too expensive, or unsafe:
- Deterministic processing continues.
- Transactions remain in review-required state.
- The report is still generated with deterministic totals and validation.
- The user sees that AI suggestions were unavailable or rejected.
- No transaction is silently categorized by AI.
- No totals, validation results, or processing counts change because of AI failure.
- Audit output records that AI was skipped, unavailable, rejected, or failed validation.

The product must remain useful without AI.

## 13. Audit requirements
Every AI-assisted decision must be auditable.

Audit records should include:
- Transaction reference.
- AI use case.
- Input fields sent to AI.
- Allowed taxonomy version or category list used.
- Prompt or instruction version once implementation exists.
- Model/provider metadata once implementation exists, without committing secrets.
- Raw structured output or safely stored equivalent.
- Parsed suggestion.
- Confidence.
- Review requirement.
- Rationale.
- Validation status of the AI output.
- Final human decision when review occurs.
- Whether the AI suggestion influenced the final category.

Audit records must make it possible to answer:
- Why was AI used for this transaction?
- What did AI suggest?
- Was the suggestion accepted, rejected, or ignored?
- Did deterministic logic override the suggestion?
- Did a human review the result?
- Did the final report rely on deterministic totals regardless of AI?

## 14. Cost-control strategy
AI should be used narrowly and deliberately.

Cost controls:
- Send only review-required ambiguous transactions to AI.
- Do not send transactions already categorized by deterministic rules.
- Do not send invalid rows unless there is a specific review-assistance use case.
- Batch only when it does not reduce traceability.
- Limit input fields to what is needed for classification.
- Cap the number of AI-assisted transactions per processing run.
- Cache or reuse suggestions for unchanged synthetic/demo transactions when appropriate.
- Prefer deterministic rules when repeated manual corrections reveal stable patterns.
- Track AI calls, skipped calls, failures, and estimated cost once implementation exists.
- Make AI optional for the product workflow.

Cost principle: AI should reduce ambiguous review effort, not become the default processing engine.

## 15. Security/privacy concerns
AI use introduces privacy risk because transaction descriptions can contain sensitive information.

Security and privacy requirements:
- Public examples must use synthetic data only.
- Real financial data must not be committed.
- Send the minimum necessary transaction fields to AI.
- Mask or omit personal identifiers that are not needed for categorization.
- Do not send full card numbers, bank account numbers, tax IDs, addresses, or credentials.
- Do not send raw files when selected fields are enough.
- Treat transaction descriptions as untrusted input.
- Guard against prompt injection embedded in merchant descriptions or notes.
- Do not include API keys, tokens, secrets, or credentials in prompts, logs, reports, or examples.
- Make retention expectations explicit before using AI with real local data.
- Ensure AI outputs do not reveal hidden system instructions or sensitive configuration.

Security review is required before any real-data AI workflow is enabled.

## 16. Synthetic examples
All examples are synthetic and must not be treated as real financial records.

### Example 1: Ambiguous marketplace transaction
Input:
```json
{
  "transaction_ref": "row-003",
  "date": "2026-04-05",
  "description": "MARKETPLACE DEMO ORDER 8842",
  "amount_sign": "debit",
  "deterministic_status": "review_required",
  "review_reason": "Marketplace description does not reveal purchase type",
  "allowed_categories": [
    "Groceries",
    "Restaurants and cafes",
    "Transport",
    "Housing and utilities",
    "Subscriptions and software",
    "Health and pharmacy",
    "Shopping",
    "Entertainment",
    "Education",
    "Travel",
    "Fees and taxes",
    "Income, refunds, and adjustments",
    "Transfers and payments",
    "Uncategorized review"
  ]
}
```

Structured output:
```json
{
  "transaction_ref": "row-003",
  "suggested_category": "Shopping",
  "suggested_category_is_new": false,
  "new_category_suggestion": null,
  "confidence": "medium",
  "requires_manual_review": true,
  "rationale": "The description suggests a marketplace purchase, but it does not identify the item or merchant category.",
  "signals_used": ["description keyword"],
  "warnings": ["ambiguous merchant", "insufficient context"],
  "invalid_output_reason": null
}
```

Expected handling: keep the transaction in manual review until the user accepts, rejects, or corrects the suggestion.

### Example 2: Known merchant already handled deterministically
Input:
```json
{
  "transaction_ref": "row-001",
  "description": "FRESH MARKET DEMO",
  "deterministic_status": "categorized",
  "deterministic_category": "Groceries"
}
```

Expected handling: do not send this transaction to AI. Known merchant rules take priority.

### Example 3: New category suggestion
Structured output:
```json
{
  "transaction_ref": "row-009",
  "suggested_category": null,
  "suggested_category_is_new": true,
  "new_category_suggestion": "Pet care",
  "confidence": "medium",
  "requires_manual_review": true,
  "rationale": "The synthetic merchant description appears related to veterinary or pet supplies, but the current taxonomy has no specific category.",
  "signals_used": ["merchant text", "description keyword"],
  "warnings": ["category outside allowed taxonomy"],
  "invalid_output_reason": null
}
```

Expected handling: do not add the category automatically. Treat it as a taxonomy improvement suggestion requiring human review and a documented product decision.
