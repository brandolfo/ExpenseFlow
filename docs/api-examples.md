# ExpenseFlow API Examples

## Endpoint

```http
POST /api/expense-reports/process
Content-Type: application/json
```

The first API accepts raw CSV text in JSON. It does not accept multipart uploads in the MVP.

## Request Shape

```json
{
  "sourceName": "demo-main.csv",
  "expectedTotal": 258248.00,
  "csvText": "date,code,description,amount,installment,source_type,notes\n..."
}
```

Fields:

| Field | Required | Notes |
| --- | --- | --- |
| `sourceName` | Yes | Used for report metadata and audit context. |
| `expectedTotal` | No | Provided externally, not inside the CSV. |
| `csvText` | Yes | Raw CSV text with required `date`, `description`, and `amount` columns. |

Full fixtures live in `backend/testdata/`.

## Successful Report

Representative request:

```json
{
  "sourceName": "demo-main.csv",
  "expectedTotal": 258248.00,
  "csvText": "date,code,description,amount,installment,source_type,notes\n2026-04-01,DMO-0001,FRESHVALE MARKET DEMO,34500.00,,purchase,Synthetic grocery row for R001\n2026-04-11,DMO-0011,MARKETBOX DEMO ORDER 8842,42999.00,,purchase,Synthetic unknown marketplace review row\n2026-04-14,DMO-0014,REFUND DEMO STORE,-12000.00,,refund,Synthetic refund-like row excluded from totals"
}
```

The full `demo-main.csv` fixture returns `200 OK` with these sections:

```json
{
  "reportMetadata": {
    "product": "ExpenseFlow",
    "reportType": "mvp_expense_report",
    "inputFormat": "csv",
    "sourceName": "demo-main.csv",
    "usesRealFinancialData": false
  },
  "processingCounts": {
    "sourceRows": 22,
    "validRows": 19,
    "categorizedRows": 14,
    "reviewRequiredRows": 6,
    "invalidRows": 3,
    "excludedFromTotalsRows": 2,
    "potentialDuplicateRows": 1
  },
  "totals": {
    "expectedTotal": 258248.00,
    "processedTotal": 258248.00,
    "categoryTotal": 189149.00
  },
  "totalValidation": {
    "status": "Match",
    "difference": 0
  },
  "categorySummary": [],
  "transactionDetails": [],
  "reviewItems": [],
  "invalidRows": [],
  "excludedRows": [],
  "auditSummary": {
    "aiUsed": false
  }
}
```

Arrays are shortened here for readability. The real response includes every transaction detail, review item, invalid row, excluded row, and audit entry.

## Invalid Rows Visible With 200

Use fixture `backend/testdata/demo-invalid-rows.csv` with expected total `34500.00`.

Expected behavior:

- returns `200 OK`
- `processingCounts.sourceRows` is `4`
- `processingCounts.invalidRows` is `3`
- `totals.processedTotal` is `34500.00`
- `totalValidation.status` is `Match`
- `invalidRows` includes raw values and errors
- invalid rows are not included in processed or category totals

Representative invalid row:

```json
{
  "sourceRow": 4,
  "rawValues": {
    "date": "2026-04-21",
    "code": "DMO-0022",
    "description": "RIDEHILL TAXI DEMO",
    "amount": "abc",
    "sourceType": "purchase"
  },
  "errors": [
    "amount must be a signed decimal number using . as the decimal separator"
  ],
  "includedInProcessedTotal": false,
  "includedInCategoryTotals": false
}
```

Row-level invalid data is reportable when the CSV file shape is valid.

## Missing Required Header With 400

Request:

```json
{
  "sourceName": "missing-required.csv",
  "expectedTotal": 0,
  "csvText": "date,description,code\n2026-04-01,FRESHVALE MARKET DEMO,DMO-0001"
}
```

Expected response:

```json
{
  "message": "CSV input could not be processed.",
  "fileErrors": [
    {
      "code": "MissingRequiredColumns",
      "message": "CSV input is missing required columns.",
      "details": ["amount"]
    }
  ]
}
```

File-level failures return `400 Bad Request` because a trustworthy report cannot be produced.

## Missing Expected Total

Request:

```json
{
  "sourceName": "demo-main.csv",
  "expectedTotal": null,
  "csvText": "date,code,description,amount,installment,source_type,notes\n..."
}
```

Expected behavior:

- returns `200 OK`
- totals are still calculated deterministically
- `totalValidation.status` is `NotProvided`
- missing expected total is not an input error

Representative response snippet:

```json
{
  "totals": {
    "expectedTotal": null,
    "processedTotal": 258248.00,
    "categoryTotal": 189149.00
  },
  "totalValidation": {
    "status": "NotProvided",
    "expectedTotal": null,
    "processedTotal": 258248.00,
    "difference": null
  }
}
```

## Total Mismatch

Use fixture `backend/testdata/demo-total-mismatch.csv` with expected total `260000.00`.

Expected behavior:

- returns `200 OK`
- `totals.processedTotal` remains `258248.00`
- `totalValidation.status` is `Mismatch`
- `totalValidation.difference` is `1752.00`
- review items, invalid rows, excluded rows, and audit summary remain visible

Representative response snippet:

```json
{
  "totals": {
    "expectedTotal": 260000.00,
    "processedTotal": 258248.00,
    "categoryTotal": 189149.00
  },
  "totalValidation": {
    "status": "Mismatch",
    "expectedTotal": 260000.00,
    "processedTotal": 258248.00,
    "difference": 1752.00
  }
}
```

AI is not used to reconcile the mismatch. The report exposes the rows and audit details needed for human review.
