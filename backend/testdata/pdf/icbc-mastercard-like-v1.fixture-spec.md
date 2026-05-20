# Synthetic Fixture Spec: icbc-mastercard-like-v1

## Purpose

This spec defines the future public synthetic Mastercard-like PDF fixture for PDF extraction and normalization tests. It is a layout and data contract only; PDF-2 does not create the actual PDF binary.

## Fixture Identity

- Variant id: `icbc-mastercard-like-v1`
- Future source name: `icbc-mastercard-like-v1.synthetic.pdf`
- Fixture type: text-selectable synthetic credit card statement PDF
- Page scope: multi-page support is part of the PDF phase
- Data policy: synthetic public data only

## Synthetic Statement Metadata

Use synthetic statement metadata only:

- Synthetic issuer label: `Synthetic ICBC-Like Mastercard Statement`
- Synthetic account label: `SYNTHETIC ACCOUNT 1111`
- Synthetic statement period: `2035-02-01 to 2035-02-28`
- Synthetic closing date: `28-FEB-35`

Do not use real names, real account/card numbers, real statement numbers, real merchants, real transaction dates, real amounts, real addresses, real IDs, or metadata copied from real PDFs.

## Layout Requirements

The future PDF should include `RESUMEN CONSOLIDADO` before transaction details. Summary lines in that section are not transaction rows.

Transaction detail starts at:

```text
DETALLE DEL MES
```

Active transaction subsections:

- `Compras del Mes`
- `Debitos Automaticos`
- `Cuotas del Mes`

Conceptual transaction columns:

- `FECHA`
- description/detail
- `NRO CUPON`
- `PESOS`
- `DOLARES`

Transaction rows may continue across pages. Repeated page headers and footers must not become transactions.

The transaction section stops at:

- `TOTAL TITULAR`

Lines after `TOTAL TITULAR` are summary/legal/informational content and must not become normalized transaction rows.

## Date And Amount Rules

- Source date format in the PDF: `dd-MMM-yy` with Spanish month abbreviations.
- Expected normalized date format in CSV expectations: `yyyy-MM-dd`
- Positive pesos values represent ordinary spending or fees.
- Negative values represent payment/refund/credit-like rows.
- Installment values can be extracted from patterns like `01/06` or `06/06` inside descriptions.
- Dolares values are foreign-currency evidence/metadata for the first phase unless a later decision accepts full multi-currency processing.

## Required Synthetic Scenarios

The future PDF should include rows that exercise:

- ordinary purchases
- automatic debit subsection
- installment rows in `Cuotas del Mes`
- rows continuing across pages
- payment or credit-like negative row
- fee/tax row
- foreign-currency evidence in the `DOLARES` column
- one malformed transaction-like candidate that becomes an extraction warning or invalid extracted row
- `RESUMEN CONSOLIDADO`, repeated headers, footers, and `TOTAL TITULAR` ignored as non-transaction content

The expected normalized rows are defined in `icbc-mastercard-like-v1.expected-normalized-rows.csv`.

## Non-Goals

- No real or anonymized statement content.
- No OCR fixture.
- No LLM fixture.
- No category expectations.
- No processed total or expected-total validation expectations.
