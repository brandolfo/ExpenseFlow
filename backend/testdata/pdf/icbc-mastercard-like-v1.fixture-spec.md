# Synthetic Fixture Spec: icbc-mastercard-like-v1

## Purpose

This spec defines the synthetic statement data contract for the public Mastercard-like PDF fixture. It is implemented by the generated multi-page text-selectable PDF `icbc-mastercard-like-v1.pdf`.

The CSV file `icbc-mastercard-like-v1.expected-normalized-rows.csv` is the source of truth for expected extraction results. The generated PDF visually contains the rows described here, and extraction/normalization tests compare extracted rows against that CSV.

## Fixture Identity

- Variant id: `icbc-mastercard-like-v1`
- Source name used in tests/API examples: `icbc-mastercard-like-v1.synthetic.pdf`
- Source type: synthetic text-selectable credit card statement
- Page count: multi-page fixture required for PDF phase completion
- Data policy: synthetic public data only

## Synthetic Statement Metadata

Use synthetic statement metadata only:

- Synthetic issuer label: `Synthetic ICBC-Like Mastercard Statement`
- Synthetic holder: `Persona Demo Mastercard`
- Synthetic card alias: `MASTERCARD DEMO 1111`
- Synthetic account label: `SYNTHETIC ACCOUNT 1111`
- Synthetic statement number: `SYN-MC-2035-02`
- Synthetic statement period: `2035-02-01 to 2035-02-28`
- Synthetic closing date: `28-FEB-35`

No real names, card/account numbers, addresses, statement numbers, tax IDs, emails, transaction dates, transaction amounts, merchants, raw statement text, hidden metadata, or extracted private text may be used.

## Conceptual Structure

The generated PDF includes `RESUMEN CONSOLIDADO` before transaction details. Summary rows in `RESUMEN CONSOLIDADO` are not normalized transactions.

Transaction detail starts at:

```text
DETALLE DEL MES
```

Active transaction subsections:

- `Compras del Mes`
- `Debitos Automaticos`
- `Cuotas del Mes`

Transaction section stop marker:

- `TOTAL TITULAR`

Lines after `TOTAL TITULAR` are summary/legal/informational content and must not become normalized transaction rows.

## Conceptual Columns

- `FECHA`
- description/detail
- `NRO CUPON`
- `PESOS`
- `DOLARES`

`NRO CUPON` maps to normalized `code`.

## Date, Amount, And Installment Rules

- Source date format in the generated PDF: `dd-MMM-yy` with Spanish month abbreviations such as `03-FEB-35`.
- Expected normalized date format in CSV expectations: `yyyy-MM-dd`.
- Positive pesos values represent ordinary spending or fees.
- Negative source values represent refund/payment/credit-like rows and must remain visible.
- Installment values should be extracted from patterns such as `01/06` or `06/06` inside descriptions.
- Pesos amounts map to normalized `amount` with `currency` set to `ARS`.
- Optional dolares amounts map to normalized rows with `currency` set to `USD` when the transaction amount is represented in the `DOLARES` column.
- Dolares values are foreign-currency evidence/metadata for the first phase unless a later decision accepts full multi-currency processing.

## Synthetic Rows To Include

The generated PDF visually contains these invented rows in extraction order. Rows 1 through 4 are on page 1, and rows 5 through 10 are on page 2.

| Page | Subsection | Source date | Detail | NRO CUPON | Pesos | Dolares | Expected purpose |
| ---: | --- | --- | --- | --- | ---: | ---: | --- |
| 1 | `Compras del Mes` | `03-FEB-35` | `ELECTRO TEST` | `SMC-8101` | `22100.00` | | ordinary purchase |
| 1 | `Compras del Mes` | `04-FEB-35` | `DELIVERY DEMO` | `SMC-8102` | `4650.75` | | delivery-like purchase |
| 1 | `Compras del Mes` | `06-FEB-35` | `TRANSPORTE FICTICIO` | `SMC-8103` | `1880.50` | | ride/transport-like purchase |
| 1 | `Debitos Automaticos` | `09-FEB-35` | `SERVICIO DIGITAL TEST` | `SMC-8104` | `7640.00` | | automatic debit |
| 2 | `Cuotas del Mes` | `11-FEB-35` | `ELECTRO TEST 01/06` | `SMC-8105` | `9500.00` | | installment row |
| 2 | `Cuotas del Mes` | `12-FEB-35` | `ELECTRO TEST 06/06` | `SMC-8106` | `9500.00` | | installment row |
| 2 | `Compras del Mes` | `15-FEB-35` | `DEVOLUCION TEST` | `SMC-8107` | `-3200.00` | | refund/credit-like row |
| 2 | `Compras del Mes` | `18-FEB-35` | `IMPUESTO SINTETICO` | `SMC-8108` | `540.75` | | fee/tax-like row |
| 2 | `Compras del Mes` | `20-FEB-35` | `SERVICIO GLOBAL DEMO` | `SMC-8109` | | `18.25` | optional foreign-currency evidence row |
| 2 | `Compras del Mes` | empty | `LINEA INCOMPLETA TEST` | `SMC-8110` | empty | | malformed transaction-like candidate |

The generated PDF also contains at least one `RESUMEN CONSOLIDADO` amount-like line before `DETALLE DEL MES` and one `TOTAL TITULAR` line after the active transaction section. Neither line appears in the expected normalized rows.

## Cases Covered

- `RESUMEN CONSOLIDADO` excluded from normalized transactions.
- `DETALLE DEL MES` transaction-section start detection.
- Subsection-derived `source_type` hints:
  - `Compras del Mes` -> `purchase`
  - `Debitos Automaticos` -> `automatic_debit`
  - `Cuotas del Mes` -> `installment`
- `NRO CUPON` mapped to normalized `code`.
- Spanish `dd-MMM-yy` date normalization to ISO date.
- Installment pattern extraction from descriptions.
- Multi-page continuation with source page and extraction order preserved.
- `TOTAL TITULAR` excluded from normalized transactions.
- Optional USD row preserved as foreign-currency evidence.
- Invalid transaction-like candidate represented in expected rows with an extraction warning.

## Explicit No-Real-Data Statement

This fixture is fully synthetic. It imitates only structural patterns of the target statement family and must not contain data copied or anonymized from a private statement.

## Non-Goals

- No OCR fixture.
- No LLM fixture.
- No category expectations.
- No processed total or expected-total validation expectations.
- No arbitrary bank/card PDF support.
- No trusted statement-total extraction.
