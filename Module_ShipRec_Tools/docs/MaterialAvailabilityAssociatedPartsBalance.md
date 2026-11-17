# Material Availability — Associated Parts Balance Columns

Last Updated: 2026-11-17

## What this is

The Associated Parts section on a Material Availability card used to answer only *when* a parent
job runs. It now also answers *how much material that job takes* and *what is left afterwards*, on
the board and on the print/export sheet, and the rows are ordered by run date.

Both figures are projections. They are not reservations and Infor Visual is still the source of
truth; they exist so the dock can see a shortfall far enough ahead to act on it.

## The two columns

| Column              | What it shows                                                                                       |
| ------------------- | --------------------------------------------------------------------------------------------------- |
| Material required   | The estimated material this job consumes, in the usage unit: the component requirement plus scrap. |
| Remaining after run | The projected on hand left once this job has consumed its material. Red when it goes negative.     |

## How the balance is worked out

1. **Start from usable on hand.** Every positive location for the part counts, **except**:
   - a location named `SHIP` (material already staged to ship), and
   - any location whose id contains `NCM` (non-conforming material).
   Those quantities are excluded because a job cannot be issued from them.
2. **Walk the rows in run-date order.** Rows are sorted by scheduled start, falling back to the due
   date, oldest first. Each live job subtracts its requirement from the running balance, and that
   job's row shows the result.
3. **Past jobs are history.** A job whose requirement or work order is closed shows blank in both
   columns and does **not** reduce the balance; its demand already happened.
4. A load-style row's requirement uses the same estimated-use figure the card already reports
   (component requirement plus the row's scrap percentage), so the two never disagree.

The running balance means the second job on a card is measured against what the first job left
behind, which is what makes "will we run out, and on which job" readable at a glance.

## Where it appears

- **Board UI:** each associated-part row gains a "Material required" and a "Remaining after run"
  figure under the part and date. The balance turns red only when it is negative.
- **Print / export:** the Associated Parts table gains the same two columns; the balance cell is
  bold red when negative. The plain-text export carries the same values.

## Related code

- `Module_ShipRec_Tools/Models/Model_Tool_MaterialAvailabilityAssociatedPartRun.cs` — the two
  values, their display strings, the negative flag, and the run sort date.
- `Module_ShipRec_Tools/Services/Service_Tool_MaterialAvailabilityBoard.cs` —
  `BuildAssociatedPartPresentation` orders the rows, walks the balance, and
  `IsJobConsumableLocation` applies the SHIP/NCM exclusion; `AppendAssociatedPartsSection` prints
  the columns.
- `Module_ShipRec_Tools/Views/View_Tool_MaterialAvailabilityBoard.xaml` — the row layout.
