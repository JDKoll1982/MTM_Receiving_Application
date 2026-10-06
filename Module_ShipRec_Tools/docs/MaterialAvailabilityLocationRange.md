# Material Availability Board - Location Range Search

Last Updated: 2026-10-06

## What this adds

The board's **Warehouse Location** mode now accepts a range of locations instead of a single
one, so a whole aisle or rack bank can be reviewed in one search.

## Using the range

1. Open **Material Availability Board** and keep the mode on **Warehouse Location**.
2. Turn on the **Location range** toggle. The single search box is replaced by **Start
   location** and **Stop location**.
3. Type the range, then press **Enter** or click **Load Range**.

The cards are grouped by location into **one collapsible section per location**. Each section
header shows the location ID and how many parts were found there, and the section can be
collapsed or expanded by clicking the header so the locations you are not working can be folded
away while their headers stay in view. Every section starts expanded.

Padding and row spacing are kept tight so more locations fit on screen at once.

## Location formatting

Both bounds use the same shared rule as the rest of the application.

| Typed value                 | Resolved value |
| --------------------------- | -------------- |
| `va001`, `V-A001`           | `V-A0-01`      |
| `r04`, `R04`                | `R-04`         |
| `s00`, `S00`                | `S-00`         |
| `w04`, `W04`                | `W-04`         |
| `RECV`, `FG-A1-01`, `T-00`  | left as typed  |

## Behavior notes

- Only locations that exist in Infor Visual are searched; gaps in the warehouse map are skipped.
- If the start location sorts after the stop location the two are swapped automatically.
- A range is capped at 60 locations. Wider ranges load the first 60, and the status line says so.
- Locations with no positive-quantity parts are skipped and do not produce an empty section.
- **Look Ahead** applies to every location in the range.
- **Print** uses the range as the report's search term, so the printed header reads
  `V-A0-01 to V-A0-05` instead of a single location. Printing a range prints every location; a
  collapsed section is a screen convenience only and does not change the printout.
- Switching the mode to **Part Number** turns the range off, clears the sections, and restores
  the single search box.

## Related code

- `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_MaterialAvailabilityBoard.cs` - range load
  command, the `CardGroups` sections, and the flat `Cards` list kept for printing.
- `Module_ShipRec_Tools/Models/Model_Tool_MaterialAvailabilityLocationGroup.cs` - one collapsible
  location section (location ID, its cards, and the expanded flag).
- `Module_ShipRec_Tools/Views/View_Tool_MaterialAvailabilityBoard.xaml` - range toggle, Start/Stop
  inputs, the shared card template, and the collapsible section list.
- `Module_Shared/Services/Lookup/Service_SharedLocationRange.cs` - shared range expansion.

## Support notes

- A range search issues one board query per location, so a wide range takes longer than a single
  location search. The progress ring stays active for the whole run.
- If a location's board fails to load, the search stops and the reason is shown in the status
  line instead of showing a partial board.
