# ExpenseFlow API Examples

## CSV Endpoint

```http
POST /api/expense-reports/process
Content-Type: application/json
```

The CSV API accepts raw CSV text in JSON. It does not accept multipart uploads in the implemented release.

## CSV Request Shape

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

## CSV Successful Report

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

## PDF Endpoint

```http
POST /api/expense-reports/process-pdf
Content-Type: application/json
```

The PDF endpoint accepts a base64-encoded, text-selectable PDF and feeds it through the deterministic PDF ingestion pipeline. It currently supports only the committed synthetic ICBC-like fixtures:

- `icbc-visa-like-v1`
- `icbc-mastercard-like-v1`

It does not support OCR, LLM extraction, arbitrary bank/card PDFs, persistence, authentication, frontend workflows, or exchange-rate conversion.

## PDF Request Shape

```json
{
  "sourceName": "icbc-visa-like-v1.synthetic.pdf",
  "expectedTotal": 65521.95,
  "pdfBase64": "JVBERi0xLjcK...shortened...",
  "statementShapeHint": "icbc-visa-like-v1"
}
```

Fields:

| Field | Required | Notes |
| --- | --- | --- |
| `sourceName` | Yes | Used for report metadata and audit context. |
| `expectedTotal` | No | Caller-provided total for deterministic validation. Extracted statement totals are not trusted validation input. |
| `pdfBase64` | Yes | Base64-encoded PDF content. Decoded content must be 5 MB or smaller. |
| `statementShapeHint` | No | Optional supported shape hint: `icbc-visa-like-v1` or `icbc-mastercard-like-v1`. |

PowerShell example using the synthetic Visa-like fixture:

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

## PDF Successful Report

Supported synthetic PDFs return `200 OK` when a deterministic report can be produced, even when some extracted rows are invalid, unprocessable, excluded, or require review.

Representative response sections:

```json
{
  "extractionMetadata": {
    "sourceName": "icbc-visa-like-v1.synthetic.pdf",
    "statementShapeId": "icbc-visa-like-v1",
    "extractionStatus": "Partial",
    "normalizedRowCount": 8,
    "invalidExtractedRowCount": 1,
    "unprocessableNormalizedRowCount": 1,
    "sourceRowCount": 9,
    "aiUsed": false
  },
  "extractionWarnings": [
    {
      "code": "missing_date_and_amount",
      "message": "Transaction-like candidate is missing date and amount.",
      "sourcePage": 1,
      "extractionOrder": 9
    }
  ],
  "report": {
    "reportMetadata": {},
    "processingCounts": {},
    "totals": {},
    "totalValidation": {},
    "categorySummary": [],
    "transactionDetails": [],
    "reviewItems": [],
    "invalidRows": [],
    "excludedRows": [],
    "auditSummary": {
      "aiUsed": false
    }
  }
}
```

Arrays and nested report sections are shortened here. The real response keeps every visible transaction, invalid row, review item, excluded row, and audit entry.

## PDF Missing Required Fields With 400

Request:

```json
{
  "sourceName": "",
  "expectedTotal": null,
  "pdfBase64": null,
  "statementShapeHint": "icbc-visa-like-v1"
}
```

Expected response:

```json
{
  "message": "PDF expense report request is invalid.",
  "fileErrors": [
    {
      "code": "missing_source_name",
      "message": "sourceName is required.",
      "details": []
    },
    {
      "code": "missing_pdf_base64",
      "message": "pdfBase64 is required.",
      "details": []
    }
  ]
}
```

## PDF Invalid Base64 With 400

Request:

```json
{
  "sourceName": "statement.pdf",
  "pdfBase64": "not base64",
  "statementShapeHint": "icbc-visa-like-v1"
}
```

Expected response:

```json
{
  "message": "PDF expense report request is invalid.",
  "fileErrors": [
    {
      "code": "invalid_pdf_base64",
      "message": "pdfBase64 must be valid base64-encoded PDF content.",
      "details": []
    }
  ]
}
```

## PDF File Too Large With 400

Decoded PDF content larger than 5 MB returns:

```json
{
  "message": "PDF expense report request is invalid.",
  "fileErrors": [
    {
      "code": "pdf_too_large",
      "message": "Decoded PDF content exceeds the 5 MB limit.",
      "details": ["maximumBytes=5242880"]
    }
  ]
}
```

## Unsupported Or Malformed PDF With 400

Unsupported shapes, empty content, encrypted PDFs, scanned/image-only PDFs, and malformed PDFs return structured file-level errors. The endpoint does not try OCR, LLM extraction, arbitrary bank/card parsing, or external APIs.

Unsupported shape hint:

```json
{
  "sourceName": "statement.pdf",
  "pdfBase64": "JVBERi0xLjcK...shortened...",
  "statementShapeHint": "unsupported-shape"
}
```

Expected response:

```json
{
  "message": "PDF expense report request is invalid.",
  "fileErrors": [
    {
      "code": "unsupported_statement_shape_hint",
      "message": "statementShapeHint must be one of the supported synthetic statement shapes.",
      "details": ["icbc-visa-like-v1", "icbc-mastercard-like-v1"]
    }
  ]
}
```

Example malformed response:

```json
{
  "message": "PDF input could not be processed.",
  "fileErrors": [
    {
      "code": "pdf_extraction_failed",
      "message": "PDF text extraction failed safely: PdfDocumentFormatException.",
      "details": []
    }
  ]
}
```

The endpoint does not expose raw PDF content or full extracted text by default.
