# Receiving Analytics Two-Tab Analytics Catalog

- **Last Updated:** 2026-10-29
- **Module:** Ship/Rec Tools
- **Status:** Delivery marked per item — **Built** shipped in the two-tab slice, **Deferred** is not built yet

This document lists everything the Receiving Analytics page could show you once it is split into two
tabs. Nothing here is a commitment. It is a menu to pick from.

## What The Page Will Look Like

The page becomes two tabs that answer two different questions.

- **ERP Data** — what you have today. It reads Infor Visual and answers "what did the business
  receive, and what is still coming?" It keeps its current filters and charts unchanged.
- **App Data** — new. It reads our own receiving application and answers "what did we actually do,
  when, how, and how well?" It gets its own filters, because it needs a different date range.

Tab 2 is the reason this work exists. Infor Visual knows what arrived at the plant. Only our app
knows who received it, how long it took, whether it was on time, and what went wrong.

## How To Read This List

- Each line has a number. Use the number when you tell me which ones you want.
- Each line ends with its delivery status: **Built** means it shipped in the two-tab slice,
  **Deferred** means it is not built yet.
- Lines marked with a star are the ones worth building first.
- Every line says what you would see on the page and what question it answers for you.
- Items you already turned down have been removed, so the numbering has gaps. The numbers never
  change, which means a number you wrote down earlier still means the same thing today.

## Start Here: The Seven Worth Building First

These cover four panels and fit on one screen without scrolling.

- **1, 2, 3 — How busy were we.** Loads per day, labels per day, and quantity received by category,
  each with a weekly and monthly view.
- **11 — Were we on time.** Received early, on time, or late against the PO due date.
- **20 — Who is filling our dock.** Top suppliers by labels, quantity, and loads, with the leading
  few suppliers showing how much of the total they account for.
- **8, 9 — How much does our app actually see.** Our app's receipts next to what Infor Visual
  recorded for the same period, overall and by category.

The last two are the only ones that need both tabs. They are what proves the app is capturing the
real work. Without them, Tab 2 is just another dashboard.

## Everything Else

### How Busy We Were

- **1 ★ Loads received** — one bar per day, with weekly and monthly trends. Tells you how many loads — **Built.**
  were worked.
- **2 ★ Labels per day** — one line per day. Tells you how many labels were printed and applied. — **Built.**
- **3 ★ Quantity by category** — stacked bars for Parts, Coils, Flat, Outside Service, and — **Built.**
  Uninventoried. Tells you what kind of material is coming in.
- **5 Load density** — average labels per load and average quantity per label, by week. Tells you — **Deferred.**
  whether loads are getting bigger or smaller.
- **6 Receiving clock** — a grid of hour by weekday showing when the dock is busiest. Tells you when — **Deferred.**
  you need people.
- **7 Touches per employee per day** — a top-ten bar. Tells you who is doing the receiving. — **Deferred.**

### How We Compare To The System Of Record

- **8 ★ Receipt coverage** — two lines on one chart: what our app recorded against what Infor Visual — **Built.**
  recorded for the same dates. Tells you how much of the real work we are capturing.
- **9 ★ Category mix, side by side** — our app's split against Visual's split. Tells you whether we — **Built.**
  classify material the same way the ERP does.

### Whether We Received On Time

- **11 ★ On-time receipt** — stacked bars for early, on time, and late against the PO due date. Tells — **Built.**
  you whether suppliers and the dock are keeping their promises.
- **12 Lateness spread** — how many days early or late, as a distribution. Tells you how late we are — **Deferred.**
  when we are late.

### Suppliers

- **20 ★ Supplier ranking** — a bar chart of the top suppliers plus the running share they add up to. — **Built.**
  Tells you who matters most.
- **21 Supplier scorecard** — a scatter of how much each supplier sends against how often they are on — **Deferred.**
  time. Separates the big reliable suppliers from the big unreliable ones.

### How The Material Was Packaged

- **24 Package mix** — a breakdown of skids, coils, sheets, and other package types. — **Deferred.**
- **25 Package density by supplier** — average quantity per package and average packages per load. — **Deferred.**
  Tells you whether we are over-packaging.
- **26 Coils per skid** — a distribution of how many coils went on each skid. Tells you whether the — **Deferred.**
  coil rules are being followed.

### Where Material Was Put

- **28 Location heat** — the most used put-away locations as a bar chart. Tells you where material is — **Deferred.**
  piling up.
- **29 Location drift** — how the mix of locations changes over time. Tells you whether a corner is — **Deferred.**
  quietly filling up that should not be.

### Dunnage

- **30 Dunnage by type** — quantity of each dunnage type and unit per day. — **Deferred.**
- **31 Dunnage by location** — where dunnage is being recorded. — **Deferred.**
- **32 Dunnage to receiving ratio** — how many dunnage records we create for every receiving label. — **Deferred.**
  Tells you whether the extra steps are earning their keep.
- **33 Dunnage by user** — who is recording dunnage work. — **Deferred.**

### Heat And Lot Markings

- **39 Marking coverage** — the percentage of heat-bearing receipts that have a marking recorded. — **Deferred.**
  Tells you whether every lot is marked.
- **40 Manual overrides** — how often a marking had to be changed by hand, and the leading reasons. — **Deferred.**
  Tells you whether the automatic marking is good enough.
- **41 Distinct heats per week** — how many different heats we handled, optionally by supplier. — **Deferred.**
- **42 Missing heat** — the percentage of inventoried coils and flat stock received with no heat — **Deferred.**
  number. Tells you whether we are losing traceability at the door.

### Who Is Using The App, And How Complete Is The Data

- **46 Active users per day** — how many people used the app each day. — **Deferred.**
- **48 How records were created** — the split between manual entry, edit mode, and the barcode — **Deferred.**
  scanner. Tells you whether the scanner is actually replacing typing.
- **49 How long records sit** — the average time between a label being created and the receipt being — **Deferred.**
  filed away.
- **50 Cross-module activity** — receiving, dunnage, and Volvo work on one chart. Tells you how busy — **Deferred.**
  the whole department was, not just receiving.

## What Each "Quantity" On The Screen Actually Is

Before any total is built, it matters what the number behind each label really counts. Two of these
turn out to be the same number stored twice.

| Where you see it | Stored as | What it actually is |
| --- | --- | --- |
| "Weight/Quantity" entry step | `weight_quantity`, two decimals | The one number the operator types. Raw, unrounded. |
| Same number, elsewhere | `quantity`, whole number | The same operator number rounded off when it is saved. Not a second measurement. |
| "Weight/Qty" grid column | reads `weight_quantity`, falls back to `quantity` | Same operator number again, whichever of the two columns has it. |
| "Qty Ordered" | `qty_ordered`, two decimals | What the purchase order asked for, copied from Infor Visual. Nobody edits it here. |
| "Remaining Qty" | `remaining_quantity`, whole number | What was still open on the PO at the moment of receipt. A snapshot, not a live balance. |
| Per-package value | `weight_per_package`, two decimals | The per-package share of the operator number. |
| Package count | `packages_per_load`, whole number | How many packages the line was split into. |
| Coils on skid | `coils_on_skid`, whole number | A count of coils. Coil material only, blank otherwise. |
| Unit | `unit_of_measure`, text | What the number counts. Blank on older rows, read back as "EA". |

Findings worth acting on:

- **Weight is not a separate measure.** The operator is asked for one number and the app stores it
  twice, unrounded and rounded. Nothing weighs anything. A chart of weight and a chart of quantity
  would show the same shape, so comparing them is meaningless.
- **The label "Weight/Quantity" is ambiguous** and is the origin of this confusion. It should follow
  the unit instead — "Quantity (LB)", "Quantity (EA)", "Quantity (COILS)" — so the number on screen
  states what it counts.
- **The entry prompt says "Enter whole number"** while the field stores two decimals and the
  whole-number copy is rounded afterward. The operator cannot tell which of those three is true.

## Things That Will Limit Some Numbers

- **To implement: different units cannot be added together.** A quantity may be in pounds, pieces, or
  coils. Twelve pieces plus three thousand pounds plus five coils is not a total. Every total needs a
  unit filter or a separate line per unit. This affects items 3, 20, and 25 the most.
- **To implement: prefer the date-and-time field over the date-only field, and fall back to the
  date-only one when the time is missing.** We record the moment material was received and also a
  date-only copy of the transaction day. Charts should follow the received date, which means some
  days will read slightly differently than they do today.
- **Our category names do not match the ERP's.** See the comparison table below. Items 8 and 9 need
  a translation between the two, and that translation must live in one place so the two tabs never
  disagree.
- **Approved: new tables only, no changes to existing tables, and use triggers and functions to
  generate the views.** Some totals cannot be produced from what is stored today. New supporting
  tables and generated views are approved on those terms.
- **Loads and labels can collapse into the same number.** Older history rows may not have a load
  number recorded, and the app then reads the label number as the load. On those rows, item 1 and
  item 2 will report the same count. Worth checking how many rows are affected before trusting the
  loads figure.

## Our Categories Against Infor Visual

The two systems name and derive categories differently, which is what item 9 has to reconcile.

| Our app (part type) | How we decide it | Infor Visual category | How Visual decides it |
| --- | --- | --- | --- |
| Standard | Part number has no "MMC" or "MMF" | Parts | Has a part, not MMF or MMC, not detail-only, no service code |
| Coil | Part number contains "MMC" | MMC Coils | Part number starts with "MMC" |
| Sheet | Part number contains "MMF" | MMF Flat | Part number starts with "MMF" |
| No equivalent | — | Outside Service | The PO line carries a service code |
| No equivalent | — | Uninventoried | No part number, or the part is marked detail-only |

Two mismatches to settle before item 9 is built:

- **"Contains" against "starts with".** Our app looks for MMC or MMF anywhere in the part number;
  Infor Visual only counts them at the start. Any part number carrying those letters in the middle
  gets classified differently by the two tabs.
- **Two Visual categories have no counterpart here.** Outside Service and Uninventoried are tracked
  in our app by other means — a no-purchase-order flag covers one case — so they cannot be compared
  line for line without a rule for each.

## Questions I Need You To Answer

- **Do you want the comparison to Infor Visual at all?** If yes, items 8 and 9 stay. If no, Tab 2
  becomes a simpler operations dashboard and the two tabs no longer have to line up.
- **Should today's unfinished work be included?** Our data holds both completed receipts and work
  still in progress. Including in-progress work means today always looks lower than it really is.
- **Are there targets?** If there are goals for on-time percentage or marking coverage, the page can
  show red and green instead of plain numbers.
- **Is per-person data acceptable on screen?** Items 7, 33, and 46 report on named individuals. That
  may not be something you want visible on a shared terminal.

## What Has Not Been Decided

No scope has been picked and no filters have been designed. The two database decisions are settled:
new tables are approved on a new-tables-only basis, generated views are approved, and existing tables
are not to be altered. Nothing else on this list is scheduled.
