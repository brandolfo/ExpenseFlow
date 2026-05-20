# Synthetic Fixture Spec: icbc-visa-like-v1

## Purpose

This spec defines the future public synthetic Visa-like PDF fixture for PDF extraction and normalization tests. It is a layout and data contract only; PDF-2 does not create the actual PDF binary.

## Fixture Identity

- Variant id: `icbc-visa-like-v1`
- Future source name: `icbc-visa-like-v1.synthetic.pdf`
- Fixture type: text-selectable synthetic credit card statement PDF
- Page scope: single page for the first extraction implementation
- Data policy: synthetic public data only

## Synthetic Statement Metadata

Use synthetic statement metadata only:

- Synthetic issuer label: `Synthetic ICBC-Like Visa Statement`
- Synthetic account label: `SYNTHETIC ACCOUNT 0000`
- Synthetic statement period: `2035-01-01 to 2035-01-31`
- Synthetic closing date: `31.01.35`

Do not use real names, real account/card numbers, real statement numbers, real merchants, real transaction dates, real amounts, real addresses, real IDs, or metadata copied from real PDFs.

## Layout Requirements

The future PDF should contain a transaction table on page 1 with conceptual columns:

- `FECHA`
- `COMPROBANTE`
- `DETALLE DE TRANSACCION`
- `PESOS`
- `DOLARES`

The transaction section starts around the header line containing:

```text
FECHA  COMPROBANTE  DETALLE DE TRANSACCION  PESOS  DOLARES
```

The transaction section stops before any of these summary/footer markers:

- `Total Consumos`
- `Impuesto de Sellos`
- `Saldo Actual`
- `Pago Minimo`

Lines after the stop markers are summary/footer content and must not become normalized transaction rows.

## Date And Amount Rules

- Source date format in the PDF: `dd.MM.yy`
- Expected normalized date format in CSV expectations: `yyyy-MM-dd`
- Positive pesos values represent ordinary spending or fees.
- Trailing-minus pesos values represent negative-like payment/refund/credit rows.
- Dolares values are foreign-currency evidence/metadata for the first phase unless a later decision accepts full multi-currency processing.

## Required Synthetic Scenarios

The future PDF should include rows that exercise:

- ordinary purchases
- payment with trailing-minus amount
- refund or credit with trailing-minus amount
- fee/tax row
- foreign-currency evidence in the `DOLARES` column
- one malformed transaction-like candidate that becomes an extraction warning or invalid extracted row
- summary/footer markers that must be ignored

The expected normalized rows are defined in `icbc-visa-like-v1.expected-normalized-rows.csv`.

## Non-Goals

- No real or anonymized statement content.
- No OCR fixture.
- No LLM fixture.
- No category expectations.
- No processed total or expected-total validation expectations.
