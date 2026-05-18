# Glossary

## Expense report
A structured output that summarizes processed transactions, categories, totals, validation results, and review items.

## Transaction
One financial movement from an input file, such as a purchase, refund, fee, payment, or adjustment.

## Merchant
The business, service, platform, or counterparty associated with a transaction.

## Category
A user-facing label that groups transactions for analysis, such as groceries, transport, subscriptions, health, or income-related adjustments.

## Categorization rule
A repeatable rule that assigns a transaction to a category based on known patterns such as merchant name, description, amount sign, or source.

## Deterministic rule
A rule implemented in code that produces the same result for the same input and does not rely on AI interpretation.

## AI suggestion
A structured recommendation produced by AI for ambiguous interpretation, such as a proposed category or explanation.

## Confidence
A structured indicator of how reliable a suggestion or categorization is expected to be.

## Manual review
A human step used to confirm, correct, or reject uncertain results.

## Audit trail
A record of how a transaction was processed, including rules applied, suggestions made, corrections performed, and validation outcomes.

## Expected total
The total amount the system expects from a statement, file metadata, user input, or known source.

## Processed total
The total calculated deterministically from transactions actually processed by the system.

## Validation
The deterministic checks used to confirm data quality, totals, required fields, and processing completeness.

## Ambiguous transaction
A transaction that cannot be confidently categorized using deterministic rules.

## Synthetic data
Artificial demo data that resembles realistic transactions without exposing real financial details or personal information.

## Source row
One row from the original transaction file. Every source row must be accounted for as categorized, invalid, requiring review, or another explicit documented state.

## Transaction state
The current domain status of a source row or transaction, such as received, invalid, review required, categorized, or excluded from totals.

## Review item
A transaction or source row that requires human inspection before it should be treated as final.

## Manual correction
A human change to a category, merchant interpretation, review status, or transaction treatment.

## Rule conflict
A case where multiple deterministic rules match the same transaction but point to different categories or outcomes.

## Invalid row
A source row that is missing required data or contains values that cannot be safely processed.

## Potential duplicate
A transaction that looks similar enough to another transaction that it should be surfaced for review instead of silently merged or discarded.

## CSV
A structured text file format used as the first supported input format for the ExpenseFlow MVP.

## Expected total validation
The deterministic comparison between a separately provided expected total and the processed total calculated from eligible rows.

## Processing run
One conceptual execution of the ExpenseFlow workflow against a transaction file and optional expected total.

## Category summary
The report section that groups categorized transactions by category and shows deterministic counts and totals.

## Excluded from totals
A visible row state for transactions that should appear in the report but should not be included in processed or category totals under the MVP treatment rules.
