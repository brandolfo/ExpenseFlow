# Synthetic Fixture Spec: icbc-visa-like-v1

## Purpose

This spec defines the synthetic statement data contract for the public Visa-like PDF fixture. It is implemented by the generated text-selectable PDF `icbc-visa-like-v1.pdf`.

The CSV file `icbc-visa-like-v1.expected-normalized-rows.csv` is the source of truth for expected extraction results. The generated PDF visually contains the rows described here, and extraction/normalization tests compare extracted rows against that CSV.

## Fixture Identity

- Variant id: `icbc-visa-like-v1`
- Source name used in tests/API examples: `icbc-visa-like-v1.synthetic.pdf`
- Source type: synthetic text-selectable credit card statement
- Page count: single-page for first extraction
- Data policy: synthetic public data only

## Synthetic Statement Metadata

Use synthetic statement metadata only:

- Synthetic issuer label: `Synthetic ICBC-Like Visa Statement`
- Synthetic holder: `Persona Demo Visa`
- Synthetic card alias: `VISA DEMO 0000`
- Synthetic account label: `SYNTHETIC ACCOUNT 0000`
- Synthetic statement number: `SYN-VISA-2035-01`
- Synthetic statement period: `2035-01-01 to 2035-01-31`
- Synthetic closing date: `31.01.35`

No real names, card/account numbers, addresses, statement numbers, tax IDs, emails, transaction dates, transaction amounts, merchants, raw statement text, hidden metadata, or extracted private text may be used.

## Conceptual Layout

The generated PDF contains one transaction table on page 1 with these conceptual columns:

- `FECHA`
- `COMPROBANTE`
- `DETALLE DE TRANSACCION`
- `PESOS`
- `DOLARES`

Transaction start marker/header:

```text
FECHA  COMPROBANTE  DETALLE DE TRANSACCION  PESOS  DOLARES
```

Transaction stop markers:

- `Total Consumos`
- `Impuesto de Sellos`
- `Saldo Actual`
- `Pago Minimo`

Rows after the first stop marker are summary/footer content and must not become normalized transaction rows.

## Date And Amount Rules

- Source date format in the generated PDF: `dd.MM.yy`
- Expected normalized date format in CSV expectations: `yyyy-MM-dd`
- `COMPROBANTE` maps to normalized `code`.
- Pesos amounts map to normalized `amount` with `currency` set to `ARS`.
- Optional dolares amounts map to normalized rows with `currency` set to `USD` when the transaction amount is represented in the `DOLARES` column.
- Trailing-minus source amounts, such as `1.111,10-`, normalize to negative decimal values.
- Dolares values are foreign-currency evidence/metadata for the first phase unless a later decision accepts full multi-currency processing.

## Synthetic Rows To Include

The generated PDF visually contains these invented rows in table order:

| Source date | Comprobante | Detail | Pesos | Dolares | Expected purpose |
| --- | --- | --- | ---: | ---: | --- |
| `04.01.35` | `SVI-9101` | `MERCADO NORTE TEST` | `18450.25` | | ordinary supermarket-like purchase |
| `05.01.35` | `SVI-9102` | `CAFE RIO FICTICIO` | `2750.00` | | cafe/restaurant-like purchase |
| `07.01.35` | `SVI-9103` | `FARMACIA CENTRAL DEMO` | `6240.80` | | pharmacy-like purchase |
| `09.01.35` | `SVI-9104` | `COMBUSTIBLE DEMO` | `22100.00` | | fuel-like purchase |
| `11.01.35` | `SVI-9105` | `SALUD INTEGRAL TEST` | `15300.50` | | service/health-like purchase |
| `13.01.35` | `SVI-9106` | `IMPUESTO SINTETICO` | `680.40` | | fee/tax-like row |
| `15.01.35` | `SVI-9107` | `DEVOLUCION TEST` | `1200.00-` | | refund/negative-like row using trailing minus |
| `18.01.35` | `SVI-9108` | `SERVICIO DIGITAL TEST` | | `12.50` | optional foreign-currency evidence row |
| empty | `SVI-9109` | `LINEA INCOMPLETA TEST` | empty | | malformed transaction-like candidate |

The malformed candidate is included to prove extraction warnings or invalid extracted rows remain visible instead of being silently dropped.

## Cases Covered

- Header-based transaction start detection.
- Stop-marker exclusion for summary/footer lines.
- Ordinary ARS purchases from multiple merchant-like domains.
- Fee/tax row preserved as an extracted transaction row.
- Trailing-minus source amount normalized to a negative decimal.
- Optional USD row preserved as foreign-currency evidence.
- Invalid transaction-like candidate represented in expected rows with an extraction warning.
- Source traceability through source name, statement shape, source page, and extraction order.

## Explicit No-Real-Data Statement

This fixture is fully synthetic. It imitates only structural patterns of the target statement family and must not contain data copied or anonymized from a private statement.

## Non-Goals

- No OCR fixture.
- No LLM fixture.
- No category expectations.
- No processed total or expected-total validation expectations.
- No arbitrary bank/card PDF support.
- No trusted statement-total extraction.
