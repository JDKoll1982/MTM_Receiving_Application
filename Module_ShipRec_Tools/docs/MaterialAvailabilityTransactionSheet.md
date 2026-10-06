# Material Availability Transaction Sheet

Last Updated: 2026-10-06

## What this sheet is

The transaction sheet is the handwritten move sheet for one warehouse location. Open
**Material Availability Board**, search by **Warehouse Location**, then choose **Print** and pick
**Transaction Sheet**. Each part found in that location gets its own block on the sheet, so the
operator can write where material was physically moved to.

## Sheet layout

The sheet is **grouped by location**: every part is printed under the location it was found in,
and a location's data always ends before the next location starts, so each location begins on a
**new page**. The page subtitle repeats that page's location.

Each part occupies one row of the sheet:

| Area        | Contents                                                                                     |
| ----------- | -------------------------------------------------------------------------------------------- |
| Part card   | **Part Number**, **Description**, and **Quantity** (quantity on hand in the searched location) |
| Location    | The destination location, auto-filled on its own small row above the entry table               |
| Entry table | **Qty 1** through **Qty 8** across the top, four blank rows, and a smaller **Total** row underneath |

- Sixteen quantity cells per part (eight columns by four rows) are available for handwriting.
- The bottom **Total** row is shorter than the quantity rows; its label spans the first seven
  columns and the last cell collects the summed quantity.
- The location is printed for the operator, so the table needs no location column and all eight
  columns are free for quantities.
- Five parts are printed per page so a location's blocks keep fitting one printed page.
- The sheet title block repeats the location, and each page shows `Page N of M` across the whole
  sheet (not per location).

## Why it looks like this

- The entry side used to be four separate `Qty / Take To` pairs, which forced the operator to
  hunt across four blocks for a single move. It is now **one table** per part with a total row.
- The part card includes the **Description**, so the operator can confirm the material without
  going back to the screen.
- The **Location** row replaced the location column, which freed a column for quantities and made
  eight quantity columns possible.
- Parts with no description on file print `Not available` instead of leaving a blank line.
- Starting a new page per location keeps each location's handwriting together and stops two
  locations sharing one page.

## Related code

- `Module_ShipRec_Tools/Services/Service_Tool_MaterialAvailabilityBoard.cs` -
  `BuildTransactionSheetDocument`, `AppendTransactionEntryGrid`, `GetTransactionSheetPrintCss`.
- `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_MaterialAvailabilityBoard.cs` - the
  `Transaction Sheet` print choice.

## Notes

- Sheet size is Letter portrait with 0.2in margins.
- A part block never splits across pages; if a block does not fit, it moves whole to the next page.
- Changing the number of quantity columns, entry rows, or parts per page means editing the
  `Transaction*` constants in the service and the expectations in
  `Service_Tool_MaterialAvailabilityBoardTests`.
- The sheet groups by each card's `SearchLocationId`, falling back to the searched location when a
  card has none (for example after a part-number search).
