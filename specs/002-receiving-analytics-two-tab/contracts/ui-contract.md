# UI Contract: Receiving Analytics App Data Tab

This file is the UI contract the view binds to. It fixes the tab structure, the panel layout, the
filter defaults, and the exact strings and colours a user sees, so a test can assert them without
reopening the spec.

## Tabs

| Order | Label |
| --- | --- |
| 1 | `ERP Data` |
| 2 | `App Data` |

`ERP Data` is first and open by default. The ERP tab keeps its existing markup wrapped in the first
TabViewItem, untouched; the second TabViewItem hosts the App Data control.

## Panels

The four panels are four pages behind a navbar, in this order, with the filter bar above the navbar
so every page is read through the same filters.

| Page | Header | Draws |
| --- | --- | --- |
| 1 | `Activity` | a loads-and-labels chart beside a quantity stack |
| 2 | `On Time` | an early, on-time and late stack with the percentage readout and target |
| 3 | `Suppliers` | a supplier ranking with cumulative share |
| 4 | `Coverage` | a two-line coverage chart over a two-sided category mix |

The four headers are the navbar labels, one or two words each, and the page opens on page 1. Each
page takes the full area below the navbar, so a panel never shares the window with another and never
scrolls. The navbar index survives `Load` and `Clear`, so a reload leaves the user on the page they
were reading.

### What each page reads

Each page states plainly what its figures are read from, in one sentence above the chart.

| Page | Line above the chart |
| --- | --- |
| 1 | Loads, labels and quantity from our own completed receipts, counted on the day they were received. |
| 2 | Our completed receipts measured against the date each purchase order was due. |
| 3 | Our completed receipts grouped by the supplier named on each one. |
| 4 | Our own completed receipts beside what Infor Visual recorded for the same days. |

- `Activity` shows loads per period and labels per period as two series, beside a quantity
  stack of one stacked bar per period with one segment per material category in scope.
- `On Time` shows one chart of `Early`, `OnTime` and `Late` stacked per period, plus a readout
  strip carrying the percentage and its target. Receipts with no PO due date appear as a separate
  remainder and are never counted as on time.
- `Suppliers` shows one ranking chart plus a readout strip with the cumulative share of
  the leading suppliers. Suppliers beyond the ranked few are grouped into one remainder so the shares
  total 100 per cent.
- `Coverage` shows two lines over the same dates, one counting the receipts
  the app recorded and one counting the receipts Infor Visual recorded, over a two-sided category mix
  that names the same five categories on both sides.

## Weekend rule

Saturday and Sunday are not read. No weekend day appears as a bucket on a day axis, and weekend
receipts contribute to no total on any page: they are excluded inside the stored procedures, so the
supplier ranking and the missing-load-number disclosure drop them too. The ERP side of the coverage
chart drops the same weekend dates, so excluding weekend work from the app never reads as lost
coverage against Infor Visual.

## Filter bar

| Control | Default |
| --- | --- |
| Range | the last thirty days ending today |
| Period | `Day`, with `Week` and `Month` available |
| Unit selector | the dominant unit in the range, or a separate line per unit when two units tie |
| Supplier measure | `Labels`, with `Quantity` and `Loads` available |
| Load | the button that runs the query |

The range defaults to the last thirty days ending today, inclusive of today. The service converts
that to an exclusive end for the stored procedures. `Clear` resets every filter to the defaults above
and empties the panels.

## Warnings

A warning is never a line of text in the filter bar. When something is wrong, a red circular button
carrying `!` takes the place of the status text, and pressing it opens a panel with two parts:

- a short heading naming what went wrong, and
- a plain-language explanation of what it means and what to do next.

The status text stays for success and information, so `Filters cleared.` and
`Loaded N activity row(s)` still read as before. The button clears itself when the next load
succeeds, when `Clear` runs, and when a load begins after an error.

| Warning | Heading | What the reader is told |
| --- | --- | --- |
| Dates reversed | The start and end dates are the wrong way round | The `To` date falls before `From`; move it to the same day or later, then press Load |
| App read failed | The app's own records could not be read | Nothing loaded and every page is empty, the underlying message, then press Load and pass the message to IT if it repeats |
| Empty range | Nothing was received in that range | No completed receipts fall between the dates, pick a wider range, and weekend days are never counted |
| ERP unavailable | Infor Visual could not be read | The app's own figures did load, only the Coverage page is empty, the other three pages are unaffected, then press Load to retry |
| Load threw | Something went wrong while loading | The load stopped, the pages still show the previous figures, the message, then press Load |

## Quantity label

Every quantity total is named by its unit, uppercased inside parentheses: `Quantity (LB)`,
`Quantity (EA)`, `Quantity (COILS)`. A blank stored unit is read as `EA`, so it renders
`Quantity (EA)`.

## On-time colour rule

- With a target configured, the on-time percentage is green at or above the target and red below it,
  and the target in force is shown beside the percentage.
- With no target configured, the percentage is plain, with no colour and no implied pass or fail.

## Empty state

When the range holds no completed receipts, every panel shows an explicit empty state instead of a
chart of zeros.

## Completed-work statement

Each page carries one plain sentence saying what its figures are read from, and every one of those
sentences names completed receipts. The line describes the data rather than warning about it, so a
reader learns what the chart counts instead of being told a caveat.

## Exposure rule

No panel shows an individual person's name or employee number.
