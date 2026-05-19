# Demo Data Policy

- Real financial data must not be committed.
- Public examples must use synthetic transactions.
- Synthetic data should resemble realistic merchant names but not expose real user statements.
- Public demos should avoid full card numbers, bank account numbers, addresses, tax IDs, or personal identifiers.
- If real files are used locally, they must be ignored by git.
- Local private PDF inputs, extracted text, generated private reports, and scratch extraction outputs must stay in ignored private work areas.
- Public PDF fixtures must be synthetic, not anonymized real statements.
- Future public synthetic PDF fixtures belong under `backend/testdata/pdf/` and must follow that folder's README policy.
