# Location Reconciliation Algorithm

Last Updated: 2026-11-13

This document replaced the Receiving reconciliation process note when the algorithm moved into its own module. The rules below are the algorithm; the day scope, the settings and the outcomes are the ones the Receiving Validator uses.

## What the algorithm does

- It loads one calendar day of receiving rows from the label queue and the history archive, and excludes reprint rows from the candidate set while counting them.
- It groups candidate rows by purchase order, part and receipt day. Two receipts of one purchase order and part on one day are one group; the same pair on two days is two groups; two different purchase orders of one part on one day are two groups.
- Rows without a purchase order are grouped by part, receipt day and vendor instead, and are never skipped for lacking a purchase order.
- It asks Infor Visual for the transfer movements of the group's part around the receipt day.
- Only the movements booked at or after the receipt time count. The receipt time is the earliest receipt stamp on the group's rows, so a transfer another user posted into the same location earlier in the day is not treated as transport of this material. A movement whose stamp carries no time of day is kept, because that is the date-only record Infor Visual writes when it has no create date.
- It follows chained moves from either end of a saved location and from the purchase order evidence tied to the group's rows. Because a row's saved location is often the destination the material already reached, and Infor Visual transfer rows usually carry no purchase order, the walk traces backwards as well as forwards; otherwise the movement that delivered the material would be rejected and the part reported as not found.
- It collapses the movement graph into net destination buckets.
- When a group resolves no transfer at all, it falls back to the day's receipts for that part: material received straight into its stock location never produces a transfer, and the receipt names the location the material reached. A row already saved against a receipt location is unchanged; any other row reports the receipt location instead of claiming Infor Visual holds no evidence.
- It hides a line whose saved location already matches from the grid, and does not offer that outcome in the filter, because such a line needs no review. The day summary still counts it.
- It allocates saved skids or tags by deterministic exact-fit then best-fit allocation: rows are sorted by quantity descending, then load number, then load id. For each row, an exact fit wins; otherwise the allocator chooses the smallest destination bucket that can still fully cover the row. Ties break by latest transfer time, then location id.
- It only proposes an update that the two-part quantity proof supports.
- It shows the transfer user from Infor Visual, never the current application session user.

## The date windows

- Movement discovery reads the receipt day plus and minus two days.
- Strict validation treats the receipt day and the receipt day plus one as inside the receipt.
- Both windows are settings. A sweep uses the same two values for every day it touches and never widens them.

## Chained move rule

- When material moves through staging, reconciliation targets the latest confirmed destination that still has net quantity after later outbound transfers are removed.
- Example: `RECV -> V-STAGE -> V-Z9-01` ends at `V-Z9-01` if the staging quantity is fully transferred onward.
- If a staging location keeps some quantity after onward transfers, that remaining net quantity stays eligible as a destination bucket.
- If the movement graph nets to zero or cannot support a full row quantity, the row stays unresolved for manual review.

## Direction rule

- Direction comes from the paired legs: the outbound leg is the source and the inbound leg is the destination. In this database the outbound leg is always the lower transaction id, and both legs point at each other, so the pair is resolved by transaction id rather than by a sign.
- Quantity is always positive. The sign is never inferred from the stored quantity, and never from transaction id ordering.

## Saved-location rule

- Before the general allocation pass, a row whose saved location is already a destination bucket with enough remaining quantity is bound there and reported unchanged.
- A row already corrected by an earlier run therefore reports as unchanged, and is never moved off a location the material genuinely reached.

## Partial move rule

- Reconciliation does not force every row into a new location.
- If the transfer evidence covers only part of the group, only the supported rows receive proposed destinations.
- Leftover rows stay unresolved and appear in the summary.

## The quantity proof

**Part one, the destination buckets.** Every bucket must be filled exactly by the rows bound to it.

- Less allocated than the bucket received means extra surplus.
- More allocated than the bucket received means not enough.
- A bucket that ran below zero inside the window fails regardless of the arithmetic.

**Part two, the source residual.** What is left in the receiving location is the group total minus what was allocated to destinations.

- It must equal the as-of balance at that location: today's on-hand minus the signed net movement at that location after the strict window.
- A disagreement leaves the rows that keep the receiving location unwritten, and is reported on the page and in the summary.
- A positive residual is legitimate stock, not a failure.

## Outcomes

- `Unchanged`: the saved location already matches where the material went, or the material stayed in the receiving location and the residual agrees.
- `Updated`: the row has a proven destination that differs from the saved location.
- `Extra Surplus`: the bucket received more than its rows account for. The raw difference is stated.
- `Not Enough`: the rows ask for more than the bucket received, or the destination ran negative, or the residual disagrees. The raw difference is stated.
- `NotFound`: no relevant Infor Visual transfer movement was found for the group. It is logged and counted, not surfaced for review.
- `Skipped`: the row was not a candidate, or the operator skipped it for this run.
- `Error`: the Visual call or the write failed.
- `Manually Set`: an operator supplied the destination by hand and the write succeeded.

There is no `Ambiguous` outcome and no `IgnoredBySettings` outcome. The surplus and shortfall pair replaces the first, and the ignored-locations list is gone.

## Worked example

For the five-skid `MMC0000650` example:

- Skids: `4500`, `5000`, `7500`, `4350`, `2000`
- Transfers from `RECV`: `12000 -> V-A0-01`, `5000 -> V-B0-01`, `6350 -> V-C0-01`
- Deterministic allocation produces: `4500 + 7500 -> V-A0-01`, `5000 -> V-B0-01`, `4350 + 2000 -> V-C0-01`

That result comes from sorting rows largest-first and choosing an exact-fit bucket when one exists, otherwise the smallest bucket that can still fully cover the row.

## Reference case

`MMC0001000` is the automated reference: 50,000 received into `RECV`, transferred 5,000 to `SHIP`, 2,500 to `V-C0-01`, 500 to `R-04` and 1,000 to `R-05`. All four destination buckets are consumed to zero, 41,000 stays in `RECV` and reports unchanged, and the residual of 41,000 must equal the as-of balance there.
