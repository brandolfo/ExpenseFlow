# ExpenseFlow Demo Script

## Demo Objective

Show that ExpenseFlow is a backend processing workflow, not a CRUD app and not an AI wrapper. The demo proves that a synthetic CSV expense export can become a categorized, validated, auditable report while every source row remains visible.

The main demo uses:

- fixture: `backend/testdata/demo-main.csv`
- expected total: `258248.00`
- endpoint: `POST /api/expense-reports/process`

## Setup Commands

From the repository root:

```powershell
cd backend
dotnet restore
dotnet build
dotnet test
```

Run the API:

```powershell
cd backend
dotnet run --project src/ExpenseFlow.Api
```

The default local URL is:

```text
http://localhost:5000
```

Health check:

```http
GET http://localhost:5000/health
```

## Send The Demo Request

From a second terminal at the repository root:

```powershell
$csv = Get-Content -Raw .\backend\testdata\demo-main.csv
$body = @{
  sourceName = "demo-main.csv"
  expectedTotal = 258248.00
  csvText = $csv
} | ConvertTo-Json -Depth 5

Invoke-RestMethod `
  -Uri "http://localhost:5000/api/expense-reports/process" `
  -Method Post `
  -ContentType "application/json" `
  -Body $body
```

## What To Point Out

Start with the summary values:

| Result | Expected value |
| --- | ---: |
| Source rows | 22 |
| Valid rows | 19 |
| Review-required rows | 6 |
| Invalid rows | 3 |
| Excluded-from-totals rows | 2 |
| Potential duplicate rows | 1 |
| Processed total | 258248.00 |
| Trusted category total | 189149.00 |
| Expected total validation | Match |

Then point out these sections in the JSON response:

- `processingCounts`: proves row accounting.
- `totals`: shows deterministic processed and category totals.
- `totalValidation`: compares the provided expected total against the processed total.
- `categorySummary`: includes only trusted categorized rows.
- `reviewItems`: includes unknown, ambiguous, conflicting, refund-like, transfer-like, and duplicate-looking rows.
- `invalidRows`: keeps invalid source rows visible with raw values and errors.
- `excludedRows`: keeps refund/transfer rows visible while excluding them from totals.
- `auditSummary`: explains deterministic rules, review reasons, validation, and confirms `aiUsed` is `false`.

## Explain The Workflow

Parsing:

ExpenseFlow accepts raw CSV text in a JSON request. The CSV parser validates required headers, preserves source row numbers, and keeps raw values for audit output. Missing optional columns are allowed.

Categorization:

Known merchants and stable description patterns are categorized with deterministic seed rules. For example, `FRESHVALE MARKET DEMO` maps to `Groceries` with rule `R001`.

Review and exclusions:

Unknown merchants, ambiguous payment services, category conflicts, potential duplicates, refunds, and transfers are not guessed away. They remain visible as review or excluded rows. Refund-like and transfer-like rows are excluded from totals in the MVP because treating them as normal spending would be misleading.

Totals:

Processed totals include only eligible rows. Category totals include only trusted categorized rows. Invalid rows, refund-like rows, transfer-like rows, and uncategorized review rows do not inflate trusted category totals.

Expected total validation:

The expected total is provided outside the CSV. ExpenseFlow compares it deterministically with the processed total. A mismatch does not hide the report; it makes the investigation visible.

Auditability:

Every row can be traced by source row number. The response exposes applied rules, review reasons, invalid-row errors, exclusion reasons, counts, and `aiUsed: false`.

## Why AI Is Not Used In The MVP

Totals and validation are financial correctness work, so they must be deterministic. The MVP also avoids AI categorization because the first version should prove that known rules, review flags, and auditability are trustworthy before adding suggestions.

Future AI can help with review-required rows, summaries, or rule recommendations, but it must be structured, traceable, confidence-aware, and safe to reject. AI must not calculate totals or validate financial correctness.

## Two-Minute Demo Flow

1. Show the README one-liner and say: "This is a backend file-to-report workflow."
2. Run or show `dotnet test` passing.
3. Start the API with `dotnet run --project src/ExpenseFlow.Api`.
4. Send the `demo-main.csv` request.
5. Point at counts: 22 source rows, 19 valid, 6 review, 3 invalid.
6. Point at totals: processed total `258248.00`, validation `Match`.
7. Point at review/invalid/excluded rows and `aiUsed: false`.
8. Close with: "The value is trustworthy processing, not storage screens or AI guesses."

## Five-Minute Demo Flow

1. Explain the problem: exported transactions are structured, but still messy and hard to audit manually.
2. Show the architecture briefly: API calls Application; Domain has statuses/categories/report concepts; Infrastructure owns CsvHelper and PdfPig adapters.
3. Run the test suite and call out unit plus integration coverage.
4. Start the API and send the main fixture.
5. Walk through parsing: required headers, optional fields, raw values, source row numbers.
6. Walk through categorization: known merchant rules, unknown marketplace, payment ambiguity, category conflict.
7. Walk through edge cases: refund row 14, transfer row 15, installments rows 16-17, duplicate-looking row 19, invalid rows 20-22.
8. Walk through totals: processed total, trusted category total, expected total validation.
9. Walk through audit: rule IDs, review reasons, invalid row errors, no AI used.
10. Explain future direction: manual corrections, persistence, exports, and later AI suggestions only for already review-required rows.

## PDF Demo Flow

The PDF demo uses only committed synthetic fixtures:

- `backend/testdata/pdf/icbc-visa-like-v1.pdf`
- `backend/testdata/pdf/icbc-mastercard-like-v1.pdf`

Run the API:

```powershell
cd backend
dotnet run --project src/ExpenseFlow.Api
```

Send a synthetic PDF request from a second terminal at the repository root:

```powershell
$pdfBytes = [System.IO.File]::ReadAllBytes(".\backend\testdata\pdf\icbc-visa-like-v1.pdf")
$body = @{
  sourceName = "icbc-visa-like-v1.synthetic.pdf"
  expectedTotal = 65521.95
  pdfBase64 = [Convert]::ToBase64String($pdfBytes)
  statementShapeHint = "icbc-visa-like-v1"
} | ConvertTo-Json -Depth 5

Invoke-RestMethod `
  -Uri "http://localhost:5000/api/expense-reports/process-pdf" `
  -Method Post `
  -ContentType "application/json" `
  -Body $body
```

What to point out:

- The API accepts base64 JSON, not a persisted upload.
- PdfPig extracts text in Infrastructure.
- The PDF normalizer maps supported statement rows into the same conceptual transaction fields used by the CSV pipeline.
- The existing categorization, review, totals, expected-total validation, and reporting logic is reused.
- Non-ARS rows stay visible as unsupported rows and do not affect ARS totals.
- Malformed transaction-like candidates stay visible as invalid rows.
- `aiUsed` remains `false`; OCR and LLM extraction are deliberately not used.
- Real PDFs must remain local/private and must not be committed.

Honest limitations:

- Only the two synthetic text-selectable ICBC-like variants are supported.
- Arbitrary bank/card PDFs, scanned statements, OCR, exchange-rate conversion, trusted statement-total extraction, persistence, auth, frontend, Docker, and cloud deployment are out of scope.
