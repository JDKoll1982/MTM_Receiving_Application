# Unusual Situations

Last Updated: 2026-11-12

## Infor Visual cannot be reached

An inline warning appears above the grid and the affected lines are marked **Error**. The rest of the day is still usable. The module retries each Visual call once before giving up on that group. Reload the day when Visual is back; you are never thrown out of the module.

## The residual does not agree with the balance

The lines that were going to keep the receiving location are marked **Not Enough** with both figures stated. Those lines are not written. This usually means something else moved at that location, or another receipt is still outstanding there.

## A destination ran negative

The movement evidence shows more leaving a location than arriving in it during the window. The module refuses to use that location and marks the affected lines **Not Enough**. It never corrects this automatically.

## Extra surplus

The movement delivered more material to a destination than the rows account for. The leftover quantity is stated. Nothing is written until a person resolves it.

## A line has no purchase order and no vendor

The group cannot be identified from movement evidence. The line is reported **Not Enough** with the missing vendor named, and stays in the day's counts.

## A line was already corrected

It reports as **Unchanged** on the next run. There is no separate "already reconciled" outcome yet; that arrives with the audit tables in a later change.

## A write is refused

The line returns to **Needs Attention** with the reason, and the review cycle does not advance as if the write had succeeded. The proof is re-run at accept time, so a balance that moved between preview and accept is the usual cause.
