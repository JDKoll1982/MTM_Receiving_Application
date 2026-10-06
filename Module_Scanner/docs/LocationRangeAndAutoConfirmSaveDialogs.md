# Scanner Location Range and Automatic Save-Dialog Confirmation

Last Updated: 2026-10-06

## What this adds

Two changes to the Scanner Workbench:

- A **location range** search mode that loads parts from every location between a start and a
  stop location in one pass.
- An **automatic answer** for the two Infor Visual dialogs that appear when a part has never
  been inventoried at the destination location.

## Auto-formatting of typed locations

Every location box now uses one shared rule, so the same text always resolves to the same
location no matter where it was typed.

| Typed value                   | Resolved value |
| ----------------------------- | -------------- |
| `va001`, `VA001`, `Va001`     | `V-A0-01`      |
| `V-A001`, `VA0-01`, `VA-A0-1` | `V-A0-01`      |
| `VA-A001`, `VAA001`           | `VA-A0-01`     |
| `r04`, `R04`, `R-04`, `R4`    | `R-04`         |
| `s00`, `S00`, `S0`            | `S-00`         |
| `w04`, `W04`, `W-4`           | `W-04`         |
| `T-00`                        | `T-00`         |
| `RECV`                        | `RECV`         |

Locations with no middle block (`R04`, `S00`, `W04`) are supported alongside the longer
`V-A0-01` style, and any value that matches no known pattern is left exactly as typed.

## Using the location range

1. Flip **Search By** to **Location**.
2. Turn on the **Location range** toggle. The single location box is replaced by **Start
   location** and **Stop location**.
3. Type the range, then press **Enter** or click **Load Range**.
4. Pick the parts to move. Each location is shown in its own compact card, and a **Select All**
   button covers every card.

Behavior notes:

- Both bounds are auto-formatted with the table above.
- If the start location sorts after the stop location the two are swapped automatically.
- Only locations that actually exist in Infor Visual are returned, so gaps in the warehouse map
  are skipped instead of inventing empty locations.
- A range is capped at 60 locations. Wider ranges load the first 60 and say so in the status
  bar.
- Each row added keeps the source location of the part it came from, so one range can feed rows
  from several locations.
- Switching **Search By** back to **Part** turns the range off and restores the single
  part-number box.

## Automatic save-dialog confirmation

When Infor Visual saves a transaction for a part that has never been at the destination
location, it can raise two modal dialogs:

1. **Inventory Transaction Entry** - "Part has not been assigned to Location ID ... Assign?"
2. **Add Part Location** - the form that creates the location assignment.

While the Workbench shows **"Was the transaction saved in Infor Visual?"**, the app watches for
these two dialogs and answers them with **Enter** (Yes, then OK). The watch:

- polls every 300 ms,
- runs for at most 30 seconds,
- only touches windows owned by the Infor Visual process, so a same-titled window from another
  application is ignored,
- stops as soon as the operator answers **Yes** or **No**, and when the Workbench page is left.

## Related code

- `Module_Shared/Helpers/Helper_SharedLocationFormat.cs` - the shared location formatter.
- `Module_Shared/Services/Lookup/Service_SharedLocationRange.cs` - range expansion.
- `Database/InforVisualScripts/Queries/34_GetLocationsInRange.sql` - read-only range query.
- `Module_Scanner/Services/Service_ScannerExecution.cs` - the dialog watcher.
- `Module_Scanner/ViewModels/ViewModel_Scanner_Workbench.cs` - range workflow and watcher wiring.

## Support notes

- If a dialog is not answered, check that VMINVENT is running under the same user account as the
  app; the watcher compares the window's process name against the scanner profile.
- A range that finds no parts reports the number of locations searched in the header error.
