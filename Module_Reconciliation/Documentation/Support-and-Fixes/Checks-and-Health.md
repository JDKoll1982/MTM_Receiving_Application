# Checks and Health

Last Updated: 2026-11-12

## What to check after a deploy

1. **The three procedures exist.** Run the test block at the bottom of `19_Migration_reconciliation_stored_procedures.sql`; it lists `sp_Reconciliation_ReceivingRows_GetDay`, `sp_Reconciliation_DayActivity_GetRange` and `sp_Reconciliation_ReceivingLocation_Update`.
2. **The module appears once.** The Reporting group holds one entry, **Receiving Validator**. Receiving offers no reconciliation card and no reconciliation step.
3. **Infor Visual is read-only.** The module's Visual connection refuses to open unless the connection string carries `ApplicationIntent=ReadOnly`.
4. **A preview writes nothing.** Load a day and check the summary; no location changes until a line is accepted.

## The reference cases

`MMC0001000` is the quantity-proof reference case: 50,000 received into `RECV`, transferred 5,000 to `SHIP`, 2,500 to `V-C0-01`, 500 to `R-04` and 1,000 to `R-05`. The four destination buckets are driven to zero and the residual of 41,000 must equal the as-of balance at `RECV`. This case is a passing automated test.

`MMC0000650` is the five-row movement example used to author the Visual queries.

### Live Visual validation

`18a`, `18b` and `18c` have not yet been run against the live Infor Visual database with the two reference parts. That check needs a live connection and is outstanding. Run each query with the sample parameters in its header and record the row counts here.

### SC-004 shop floor measurement

`SC-004` has two halves and neither has been measured. Both need the shop floor terminal, so both are outstanding manual checks rather than anything a test can hold.

- **A 500-row day inside 15 seconds.** Load a 500-row day and time it end to end.
- **The grid stays usable at 250 rows.** Scroll a 250-row day and confirm the page keeps responding.

The grid already virtualizes: the findings `ListView` sits in the `*` row of the page grid, so it measures to the available height and recycles item containers instead of realizing every row. This measurement decides whether the day query or the quantity proof needs work; it does not ask for a change to the grid.

## Signs of trouble

- **A day previews but every line is Error.** The Visual connection is down or refusing; check the inline banner.
- **The summary counts do not add up.** That would be a defect. Every loaded row must carry exactly one outcome, and the seven counts must sum to the rows loaded.
- **A write succeeds but the location is unchanged.** The procedure reports the affected row count; a return of zero is treated as a failed write and the line stays unresolved.
- **A historical day fails the residual check every time.** The as-of reconstruction depends on `18c` returning post-window movement. If it returns nothing for a part, the balance is today's live figure and a historical day will disagree. Confirm the part has movement rows after the window.
