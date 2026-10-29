# Receiving Analytics: ERP Data and App Data

Last Updated: 2026-10-29

## Overview

Receiving Analytics is now two tabs. **ERP Data** is the page as it was, reading Infor Visual.
**App Data** is new: it reads the receiving application's own records and answers what the dock
actually did, when, how, and how well.

The two tabs answer different questions with different data, so the App Data tab carries its own
date range and its own filters. Switching tabs never moves the other tab's dates or charts.

## How to Use

1. Open **Ship/Rec Tools** and select **Receiving Analytics**. ERP Data opens first, unchanged.
2. Select the **App Data** tab.
3. Set the range (defaults to the last thirty days), the period (Day, Week or Month), the unit each
   quantity total is named in, and the supplier ranking measure (Labels, Quantity or Loads).
4. Click **Load**. **Clear** resets the range and filters.

The unit list holds the units the loaded range actually contains. The unit the range's work happened
in most often is chosen for you, and every quantity total then names that unit. When two units tie
there is no single right answer, so the quantity panel draws one line per unit instead of adding them,
because pounds, pieces and coils cannot be added.

## The Four Panels

Each panel is its own page behind a navbar, in the order below. The filter bar sits above the navbar,
so one range, period and unit choice drives every page.

| Page | Header | Question it answers | Reads | Catalog items |
| --- | --- | --- | --- | --- |
| 1 | Activity | How many loads and labels, and what kind of material, per day, week or month | Loads, labels and quantity from our own completed receipts, counted on the day they were received | 1, 2, 3 |
| 2 | On Time | Did the material arrive before the PO due date | Our completed receipts measured against the date each purchase order was due | 11 |
| 3 | Suppliers | Which suppliers account for the work, and how much the leading few add up to | Our completed receipts grouped by the supplier named on each one | 20 |
| 4 | Coverage | How much of the real work the app captures, and whether it classifies it the way the ERP does | Our own completed receipts beside what Infor Visual recorded for the same days | 8, 9 |

Because one panel owns the screen at a time, each one gets the full width and height below the
navbar. Loading again or clearing the filters leaves you on the page you were reading.

Saturday and Sunday are not read. Weekend days never appear on a day axis and weekend receipts
count towards no total, including the supplier ranking and the missing-load-number line. The Infor
Visual side of the coverage chart drops the same weekend dates, so the comparison stays honest.

Each page says in one plain sentence what its figures are read from, rather than warning about them.
Every one of those sentences names completed receipts, because a receipt still in progress is not
filed into `receiving_history` yet and would make today look lower than it is.

## How The Two Sides Line Up

The comparison panel is the only place two systems meet, so it states its rules in the panel itself.

**Only part numbers the app also received.** Infor Visual records material that never flows through
the receiving app, so counting all of it would report a shortfall that says nothing about what the
app captured. Both the coverage chart and the category mix are limited to the part numbers our own
receipts carry for the selected dates. On live data that is 214 of the 524 distinct parts a
thirty-day window holds. Material Infor Visual receives with no part number at all, meaning Outside
Service and Uninventoried, is outside the comparison by definition because the app does not carry it
by part.

- **Coverage.** Two lines over the same dates, one counting app labels per day and one counting Infor
  Visual receipt lines per day, with a day only one side recorded drawn at zero on the other line.
- **Category mix.** The app's split of receipts beside Infor Visual's split of receipt lines, over the
  same five ERP category names, each side summing to 100 per cent. Both halves count receipts, so a
  reader is comparing classification and not two different measures. The app's three part types
  (Standard, Coil, Sheet) reach the five ERP names through one translation,
  `Translation_ReceivingAnalyticsCategory`, which both tabs read.

**Known limits, inherited from the unchanged ERP query.** The ERP half counts one PO line anchored on
its last received date, so a line received on two days counts once, on the later day. It also excludes
consignment and internal orders. Correcting either would change the ERP Data tab, which this work must
not do, so the comparison reports the ERP as the ERP counts itself instead of adjusting it silently.

## On-Time Target

The on-time percentage is early plus on time over the receipts that carry a due date. Receipts with no
due date are reported separately and are never counted as on time.

The target is administrator configuration, not a literal in the code:

| Setting | Scope | Permission | Default |
| --- | --- | --- | --- |
| `ReceivingAnalytics.OnTimeTarget` | System | Admin | 95 |

The percentage is green at or above the target and red below it, and the readout names the target in
force. With no target configured the percentage is shown plainly with no colour, and the readout says
"no target configured". With no receipt carrying a due date the figure is absent rather than zero.

## The Four Procedures

All four take `p_start_date` inclusive and `p_end_date` exclusive, and all four bucket on
`COALESCE(received_date, transaction_date)`.

| Procedure | Returns |
| --- | --- |
| `sp_Reporting_ReceivingAnalyticsApp_GetActivityByDay` | Per day, part type and unit: quantity, receipt count, and the day's distinct load and label totals |
| `sp_Reporting_ReceivingAnalyticsApp_GetOnTimeByDay` | Per day and status: early, on time, late or no due date, with a receipt count |
| `sp_Reporting_ReceivingAnalyticsApp_GetSupplierTotals` | Per supplier and unit: labels, loads and quantity, with a blank supplier grouped as `Unknown` |
| `sp_Reporting_ReceivingAnalyticsApp_GetMissingLoadNumberCount` | One number: receipts in range with no load number |

The app database is read only, Infor Visual is still opened with read-only intent, no existing table
is altered, and no ad-hoc SQL is issued from application code: every app read goes through one of
these procedures.

## What Is Not In This Slice

Catalog items 5, 6, 7, 12, 21, 24, 25, 26, 28, 29, 30, 31, 32, 33, 39, 40, 41, 42, 46, 48, 49 and 50
are out of scope and stay unbuilt. In particular:

- Item 39 marking coverage is deferred with its target, so no marking-coverage figure and no marking
  target exist here.
- Items 7, 33 and 46 report on named individuals and stay out; nothing on App Data names a person.
- Item 25 packaging density is the one place a weight comparison could have crept back in, and it is
  excluded.

## Files

- `Module_ShipRec_Tools/Views/View_Tool_ReceivingAnalytics.xaml` — the two tabs; the ERP markup is
  wrapped in place and not moved.
- `Module_ShipRec_Tools/Views/View_Tool_ReceivingAnalyticsApp.xaml(.cs)`
- `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_ReceivingAnalyticsApp.cs`
- `Module_ShipRec_Tools/Contracts/IService_Tool_ReceivingAnalyticsApp.cs`
- `Module_ShipRec_Tools/Services/Service_Tool_ReceivingAnalyticsApp.cs`
- `Module_ShipRec_Tools/Data/Dao_Tool_ReceivingAnalyticsApp.cs`
- `Module_ShipRec_Tools/Models/Model_Tool_ReceivingAnalyticsApp*.cs`
- `Module_ShipRec_Tools/Helpers/Translation_ReceivingAnalyticsCategory.cs`
- `Database/Database_Deployment/Sql_Files/StoredProcedures/Reporting/sp_Reporting_ReceivingAnalyticsApp_*.sql`
