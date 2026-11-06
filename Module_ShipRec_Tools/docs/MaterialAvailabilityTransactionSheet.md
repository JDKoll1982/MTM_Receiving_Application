# Material Availability Transaction Sheet

Last Updated: 2026-11-06

## What this sheet is

The transaction sheet is the handwritten move sheet for one warehouse location. Open
**Material Availability Board**, search by **Warehouse Location**, then choose **Print** and pick
**Transaction Sheet**. Each part found in that location gets its own block on the sheet, so the
operator can write where material was physically moved to.

## Sheet layout

The sheet is **grouped by location**: every part is printed under the location it was found in,
and a location's data always ends before the next location starts, so each location begins on a
**new page**. The page subtitle repeats that page's location and the receipt window used for the
skid count.

Each part occupies one row of the sheet:

| Area        | Contents                                                                                                               |
| ----------- | ---------------------------------------------------------------------------------------------------------------------- |
| Part card   | **Part Number**, **Description**, and **Quantity** (quantity on hand in the searched location)                         |
| Skid count  | **Skids / Pkgs** received for that part inside the selected receipt window, on its own small row above the entry table |
| Entry table | **Qty 1** through **Qty 8** across the top, three blank rows, and a smaller **Total** row underneath                   |

- The skid count row replaces the old destination **Location** row, which the page subtitle already
  carries.
- The skid count is the number of receiving labels recorded for the part; one label is one skid or
  package.
- The receipt window comes from the **Date Range** flyout on the board. Leaving both dates empty
  counts the current date only. A single date counts that day. Reversed dates are swapped so the
  range always runs start to finish.
- When the receipt lookup fails, that row prints `n/a` instead of blocking the sheet.
- Twenty-four quantity cells per part (eight columns by three rows) are available for handwriting.
- Three entry rows is the deepest the table can go and still keep five part blocks on one printed
  page. A fourth row pushed the fifth block onto a new sheet and left the rest of the page blank.
- The bottom **Total** row is shorter than the quantity rows; its label spans the first seven
  columns and the last cell collects the summed quantity.
- Five parts are printed per page so a location's blocks keep fitting one printed page.
- The sheet title block repeats the location, and each page shows `Page N of M` across the whole
  sheet (not per location).

## Why it looks like this

- The entry side used to be four separate `Qty / Take To` pairs, which forced the operator to
  hunt across four blocks for a single move. It is now **one table** per part with a total row.
- The part card includes the **Description**, so the operator can confirm the material without
  going back to the screen.
- The **Skids / Pkgs** row tells the operator how many skids or packages of that part arrived in
  the receipt window, so the expected move count is known before any quantity is written.
- Parts with no description on file print `Not available` instead of leaving a blank line.
- Starting a new page per location keeps each location's handwriting together and stops two
  locations sharing one page.

## Where the skid count comes from

- Data source: MySQL `receiving_history`, read through `sp_Receiving_History_Get`
  (`p_PartID = NULL`, `p_StartDate = p_EndDate` for a single day).
- The count is the number of receiving rows (labels) whose `transaction_date` falls inside the
  window, grouped per part.
- Infor Visual remains the source of the part card itself (quantity on hand).

## Related code

- `Module_ShipRec_Tools/Services/Service_Tool_MaterialAvailabilityBoard.cs` -
  `FormatBoardForPrintAsync`, `BuildTransactionSheetDocument`, `AppendTransactionReceivedPackagesRow`,
  `GetTransactionSheetPrintCss`.
- `Module_ShipRec_Tools/Data/Dao_Tool_MaterialAvailabilityReceipts.cs` - the read-only receipt
  count query.
- `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_MaterialAvailabilityBoard.cs` - the receipt date
  window (`ReceiptsFromDate`, `ReceiptsToDate`, `ResolveReceiptDateRange`) and the
  `Transaction Sheet` print choice.

## Notes

- Sheet size is Letter portrait with 0.2in margins, which leaves about 10.6in of usable height.
- A part block never splits across pages; if a block does not fit, it moves whole to the next page.
- Five part blocks measure roughly 9.3in, so a page has headroom for a wrapped description.
- Changing the number of quantity columns, entry rows, or parts per page means editing the
  `Transaction*` constants in the service and the expectations in
  `Service_Tool_MaterialAvailabilityBoardTests`.
- The sheet groups by each card's `SearchLocationId`, falling back to the searched location when a
  card has none (for example after a part-number search).

