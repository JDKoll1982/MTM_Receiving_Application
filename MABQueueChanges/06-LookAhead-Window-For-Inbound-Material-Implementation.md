# Implementation — Look Ahead Becomes a Highlight Window

Last Updated: 2026-10-19

Status: **Applied** (Option B — rename and reword, no filtering). The historical "Option A"
analysis is kept below for reference only.

## Applied Changes

1. Control label:

```csharp
/// <summary>
/// The selector does not hide rows; it marks dates that fall beyond the chosen window. The
/// label says "Highlight" so it does not promise a filter.
/// </summary>
public string LookAheadLabel => "Highlight window:";
```

2. Empty-state wording in `View_Tool_MaterialAvailabilityBoard.xaml`:

```text
No inbound material found for this part.
```

3. Inbound line markers — `Model_Tool_MaterialAvailabilityIncomingLine` gained
   `IsOutsideHighlightWindow`, `IsOverdue`, `OverdueDays`, `StatusNoteDisplay`, and
   `HasStatusNote`. `BuildIncomingPresentation` sets them:

```csharp
IsOutsideHighlightWindow =
    line.IsHistorical is false
    && incomingWindowDays.HasValue
    && line.Date is DateTime outsideDate
    && outsideDate.Date > horizon,
IsOverdue =
    line.IsHistorical is false
    && line.Date is DateTime overdueDate
    && overdueDate.Date < today
    && line.Row.RemainingQty > 0,
```

   `Dialog_MaterialAvailabilityIncomingDetails.xaml` renders `StatusNoteDisplay` under `DateLabel`.

4. Associated-part marker — `Model_Tool_MaterialAvailabilityAssociatedPartRun` gained
   `OutsideWindowHintDisplay` and `HasOutsideWindowHint`, set from the same horizon comparison
   (`"beyond {n}-day highlight"`), and bound under the run date on the card.

5. The card's `Expected delivery` captions append `(beyond N-day highlight)` when the earliest
   confirmed or requested date falls outside the window (see change 05).

6. The print output for a card with no inbound detail now reads `No inbound material found for
   this part.`

## Behaviour

Nothing is filtered or hidden. The window only affects ordering (`GetAssociatedPartPriority`
bucket 3) and the marker text. `All` removes the markers.

## Files Changed

- `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_MaterialAvailabilityBoard.cs`
- `Module_ShipRec_Tools/Services/Service_Tool_MaterialAvailabilityBoard.cs`
- `Module_ShipRec_Tools/Models/Model_Tool_MaterialAvailabilityIncomingLine.cs`
- `Module_ShipRec_Tools/Models/Model_Tool_MaterialAvailabilityAssociatedPartRun.cs`
- `Module_ShipRec_Tools/Views/View_Tool_MaterialAvailabilityBoard.xaml`
- `Module_ShipRec_Tools/Dialogs/Dialog_MaterialAvailabilityIncomingDetails.xaml`
- `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/Services/Service_Tool_MaterialAvailabilityBoardTests.cs`

## Tests

- `FormatBoardForPrintAsync_ShouldOmitIncomingSectionWhenNoIncomingDataExists` updated to the new
  empty-state sentence.
- `GetBoardByPartAsync_ShouldKeepAssociatedPartRunsVisibleOutsideSelectedWindow` extended to assert
  `OutsideWindowHintDisplay` on the +45 day run and no hint on the +10 day run.
- `GetBoardByPartAsync_ShouldShowConfirmedAndRequestedDeliverySeparately` asserts the
  beyond-highlight marker on an inbound line.

Full suite: 838 passed, 0 failed.

## Files Touched by Option A (not implemented)

## Previous Behaviour

`BuildIncomingPresentation` computes a horizon and then never filters on it:

```csharp
var today = DateTime.Today;
var horizon = today.AddDays(incomingWindowDays ?? 30);

foreach (var row in partIncomingRows)
{
    // no comparison against horizon anywhere in this loop
}
```

The only use of `incomingWindowDays` inside the method is the wording of `nextDateSummary`:

```csharp
: incomingWindowDays.HasValue
    ? $"No inbound material due in the next {incomingWindowDays.Value} days."
    : "No inbound material found.";
```

`BuildAssociatedPartPresentation` does use the horizon, but only to classify:

```csharp
IsOutsideSelectedLookAheadWindow =
    row.NextDueToRunDate.HasValue
    && row.IsPastJob is false
    && incomingWindowDays.HasValue
    && row.NextDueToRunDate.Value.Date > horizon,
```

which affects bucket 3 ordering, not membership.

The XAML empty-state string is:

```text
No qualifying incoming material was found for the selected look-ahead window.
```

## Evidence That This Was Deliberate

Two tests assert outside-window rows are still displayed:

- `GetBoardByPartAsync_ShouldKeepAssociatedPartRunsVisibleOutsideSelectedWindow` (line ~569).
- `GetBoardByPartAsync_ShouldUseOpenPoDateEvenWhenItFallsOutsideSelectedWindow` (line ~312).

So the behaviour is intentional; only the wording and the user expectation are wrong. Any filter
implementation must update these two tests.

## Option A — Real Filter for Inbound Material (not implemented)

In `BuildIncomingPresentation`, after selecting a date for a non-blanket line, skip it when it is
outside the window:

```csharp
var selectedDate = SelectBestIncomingDate(row);
if (selectedDate is not null
    && incomingWindowDays.HasValue
    && selectedDate.Date > horizon)
{
    continue;   // outside the selected look-ahead window
}
```

Consequences to update in the same change:

- `qualifyingRollupRows` must use the same filter, otherwise `RemainingQty` still includes
  out-of-window balances. Add the row to `qualifyingRollupRows` only when it survives the filter,
  and decide explicitly whether undated lines are included (recommended: yes, they carry quantity
  even without a date, and the card already has wording for that case).
- Blanket lines are handled before this point; keep their `Last received on` history row.
- Add the applied window to the section heading: `Incoming Material (next 30 days)` / `(all)`.
- Associated Parts remains unfiltered (Option B wording applies there).

## See Also

- [06 — Reader guide](06-LookAhead-Window-For-Inbound-Material-User-Guide.md)
