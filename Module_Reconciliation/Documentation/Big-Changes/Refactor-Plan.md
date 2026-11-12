# Refactor Plan

Last Updated: 2026-11-12

## Goal

Move location reconciliation out of Receiving into its own module that works one day at a time, writes only when the material is fully accounted for, and shares no code with Receiving.

## Approach

1. Build the new module until it works end to end.
2. Add its single write procedure and its Visual queries.
3. Register it and give it one navigation entry.
4. Remove the old surface one step at a time, building between steps.

The removal deliberately comes last: nothing is taken out until its replacement is in place and compiling.

## Risks and how each was handled

| Risk | Handling |
| --- | --- |
| Two modules quietly sharing code | The module owns its models, data access, procedures and settings keys. The only contact is one write procedure. Verified by building with each module's folder excluded. |
| A line being written without proof | The quantity proof is the only place a line becomes writable, and it is re-run at accept time rather than trusted from the preview. |
| A historical day failing every residual check | The balance is reconstructed as of the receipt day instead of using today's live figure. |
| A wrong direction on a transfer pair | Direction comes from the transaction type, not from transaction id ordering. |
| A sweep widening the evidence windows | Both windows are read once and never written by the sweep. |
| Removing the old surface breaking the build | The new surface lands first, and the solution builds between each removal wave. |

## Rollback

The change adds folders and deletes files; it adds no tables and no columns, so there is no data migration to reverse.

1. Redeploy the previous build. The application database needs no change, because the three new procedures are additive and nothing else was altered.
2. Optionally drop the three procedures if the previous build is not restored; nothing else references them.
3. Restore the deleted Receiving files from version control, which returns the old reconciliation step and dialog.
4. Restore the two deleted Visual query files if anything still calls them.

## Success criteria

- The module is reachable exactly once, from the Reporting group, with its own accent and title.
- A day's lines carry exactly one outcome each and the seven counts add up to the rows loaded.
- A line is never written without a passing quantity proof.
- The `MMC0001000` reference case passes as an automated test.
- Receiving offers no reconciliation card, step or dialog, while entry-time location suggestion returns the same recommendation as before.
- Neither module names the other in code, settings, or procedure names.
