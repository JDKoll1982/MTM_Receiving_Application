# Implementation — Next Delivery Date Accuracy

Last Updated: 2026-10-19

Status: **Applied** (approved answers: separate confirmed/requested lines; order-date fallback
removed). Files listed under "Files Changed".

## Previous Behaviour

`Service_Tool_MaterialAvailabilityBoard.SelectBestIncomingDate` returns the first available date
from this hierarchy:

```text
LinePromiseDate -> LinePromiseShipDate -> HeaderPromiseDate -> HeaderPromiseShipDate
-> LineDesiredReceiveDate -> HeaderDesiredReceiveDate -> OrderDate
```

`BuildIncomingPresentation` then labels it `Line promise delivery`, `Line promise ship`,
`PO promise delivery`, `PO promise ship`, `Line desired receive`, `PO desired receive`, or
`PO order date`. `card.NextDateSummary` receives either

```csharp
$"Next material date: {firstDisplayLine.Label} {firstDisplayLine.Date:MM/dd/yyyy}"
```

or, for blanket lines, `"{label}: {date}"` with the `Last received on` label.

Card header selection in `BuildCards`:

```csharp
var headerSummary =
    associatedPartPresentation.AssociatedPartRuns.Count > 0
        ? associatedPartPresentation.SummaryText
        : incomingPresentation.NextDateSummary;
```

So the incoming next date was only rendered in the card header when the part had no associated
runs. The `Incoming Material` section of the card rendered quantities and a progress bar only, and
`UpcomingDates` was consumed by the print output alone.

`card.NextDateSummary` still uses the same label-based wording; the card now also carries the
separate `ConfirmedDeliveryDisplay` / `RequestedDeliveryDisplay` captions, so the header summary is
no longer the only place a delivery date can appear.

## Measured Data (MTMFG, 2026-10-19)

```sql
SELECT COUNT(*) AS OpenLines,
       SUM(CASE WHEN pol.PROMISE_DATE IS NULL THEN 1 ELSE 0 END) AS NoLinePromise,
       SUM(CASE WHEN pol.PROMISE_DATE IS NULL AND pol.PROMISE_SHIP_DATE IS NULL
                 AND po.PROMISE_DATE IS NULL AND po.PROMISE_SHIP_DATE IS NULL
                THEN 1 ELSE 0 END) AS NoPromiseAnywhere
FROM   PURC_ORDER_LINE pol
JOIN   PURCHASE_ORDER po ON po.ID = pol.PURC_ORDER_ID
WHERE  ISNULL(po.STATUS,'') NOT IN ('C','X')
  AND  ISNULL(pol.LINE_STATUS,'') NOT IN ('C','X')
  AND  (pol.ORDER_QTY - pol.TOTAL_RECEIVED_QTY) > 0;
```

| Measure | Value |
| ------- | ----- |
| Open PO lines | 949 |
| No line-level promise date | 947 |
| No promise date anywhere | 388 |
| Lines that would fall back to `OrderDate` | 0 today, latent |

## Implemented Changes

1. `SelectBestIncomingDate` no longer falls back to `OrderDate` and no longer returns `null`:

```csharp
// The purchase-order order date is when the PO was written, not when material is due, so
// it is deliberately not used as a delivery date. The line is still reported, undated.
return new IncomingLinePresentation(
    row,
    Date: null,
    Label: "No delivery date",
    IsHistorical: false,
    IsConfirmed: false
);
```

`IncomingLinePresentation.Date` is now `DateTime?`, so an undated purchase-order line still appears
in the incoming detail list (previously it was dropped from `IncomingLines` and only counted in
the rollup).

Confidence is carried on the record:

```csharp
private sealed record IncomingLinePresentation(
    Model_InforVisualIncomingSupplyRow Row,
    DateTime? Date,
    string Label,
    bool IsHistorical,
    bool IsConfirmed);
```

Promise dates (`LinePromiseDate`, `LinePromiseShipDate`, `HeaderPromiseDate`,
`HeaderPromiseShipDate`) set `IsConfirmed: true`; desired-receive dates
(`LineDesiredReceiveDate`, `HeaderDesiredReceiveDate`) set `IsConfirmed: false`.

2. `IncomingPresentation` now also returns `EarliestConfirmedDate` and `EarliestRequestedDate`, and
   `BuildCards` maps them onto the card:

```csharp
HasConfirmedDelivery = incomingPresentation.EarliestConfirmedDate.HasValue,
ConfirmedDeliveryDisplay = FormatDeliveryDisplay(
    "Confirmed", incomingPresentation.EarliestConfirmedDate, incomingWindowDays),
HasRequestedDelivery = incomingPresentation.EarliestRequestedDate.HasValue,
RequestedDeliveryDisplay = FormatDeliveryDisplay(
    "Requested", incomingPresentation.EarliestRequestedDate, incomingWindowDays),
```

`FormatDeliveryDisplay` appends `(overdue N days)` or `(beyond N-day highlight)` where relevant.

3. `View_Tool_MaterialAvailabilityBoard.xaml` renders an `Expected delivery` block in the
   Incoming Material section with one line per confidence plus the
   `No delivery date on the PO` fallback, so the date is visible without opening the dialog.

4. `UpcomingDates` is de-duplicated per calendar date instead of per `(Label, Date)`.

5. `Model_Tool_MaterialAvailabilityIncomingLine` gained `DeliveryConfidence`,
   `IsOutsideHighlightWindow`, `IsOverdue`, `OverdueDays`, `StatusNoteDisplay`, and
   `HasStatusNote`. `DateDisplay` now returns `No delivery date on the PO` when the date is null.
   `Dialog_MaterialAvailabilityIncomingDetails.xaml` shows `StatusNoteDisplay` under `DateLabel`.

## Files Changed

- `Module_ShipRec_Tools/Services/Service_Tool_MaterialAvailabilityBoard.cs`
- `Module_ShipRec_Tools/Models/Model_Tool_MaterialAvailabilityIncomingLine.cs`
- `Module_ShipRec_Tools/Models/Model_Tool_MaterialAvailabilityCard.cs`
- `Module_ShipRec_Tools/Views/View_Tool_MaterialAvailabilityBoard.xaml`
- `Module_ShipRec_Tools/Dialogs/Dialog_MaterialAvailabilityIncomingDetails.xaml`
- `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/Services/Service_Tool_MaterialAvailabilityBoardTests.cs`

## Test Changes

- `GetBoardByPartAsync_ShouldKeepIncomingRollupWhenDueDatesAreMissing` renamed to
  `..._ShouldKeepIncomingRollupWithoutUsingOrderDateAsDelivery`; it now asserts no upcoming date,
  a single undated line, and `No delivery date on the PO`.
- `..._ShouldKeepThreeOpenPosVisibleForMmc000744` — the order-date line is now undated, so
  `UpcomingDates` has two entries.
- New `..._ShouldShowConfirmedAndRequestedDeliverySeparately` — confirmed and requested captions,
  plus the beyond-highlight marker.
- New `..._ShouldMarkPastRequestedDeliveryAsOverdue` — overdue caption and status note.

## Verification

- `dotnet test ... --filter "FullyQualifiedName~MaterialAvailability"` — 39 passed.
- Full suite — 838 passed, 0 failed.
- Data check on 2026-10-19: 0 of 949 open PO lines currently reach the removed `OrderDate`
  fallback, so no live line changes its displayed date.

## Risk

Low. Read-only display change; the purchase-order order date is still read and mapped, it is simply
no longer presented as an arrival date.

## See Also

- [05 — Reader guide](05-Next-Delivery-Date-Accuracy-User-Guide.md)
