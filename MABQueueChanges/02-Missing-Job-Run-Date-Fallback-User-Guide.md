# Jobs Without a Schedule Now Show a Real Date

Last Updated: 2026-10-19

Most open work orders in Infor Visual have no operation schedule and no work-order schedule, so
the card kept saying "No scheduled date" even though Infor Visual already knew the date the
material was needed.

## What You Saw Before

For a released job that had not been scheduled yet, the row could only ever say:

```text
Future Run
WO-075089
No scheduled date
```

and in practice you usually did not see that row at all, because a finished job from an earlier
year took the slot.

`WO-075089` was released on 10/19/2026 and its component requirement was due 10/19/2026, but the
card could only look at the operation start date and the work-order start date. Both were blank,
so the date came out empty.

This was not a rare case. Of the 8,866 open component requirements in MTMFG on 10/19/2026, 8,209
(93%) had neither an operation schedule nor a work-order schedule.

## What You See Now

```text
Due Today
WO-075089
10/19/2026
```

The board now looks for a date in this order and uses the first one that exists:

1. The scheduled start of the operation that consumes the material.
2. The scheduled start of the work order.
3. The requirement's required date.
4. The work order's want date.

The card, the work order detail panel (`Next Due-To-Run Date`), and the printed board all use this
same date.

## Why It Matters

A card that says "No scheduled date" for a job that is 93% of your open demand is not useful for
planning. With the fallback, almost every open job now shows a date, and you can tell at a glance
whether the material is needed today, this week, or later.

## What You Might Notice

- Rows that used to say `No scheduled date` now carry a date and a label such as `Due Today` or
  `Overdue`.
- The difference between a scheduled date and a required date is not shown on screen yet. The board
  records which of the four dates was used, so the wording can be surfaced later if you want it.
- Some of those fallback dates are old. An unreleased requirement from 2021 that was never
  scheduled now reads as `Overdue`. Change 03 removes the planning shells that cause most of that
  noise.

## See Also

- [03 — Master shells and quotes no longer count as demand](03-Master-And-Quote-Orders-In-Demand-User-Guide.md)
- [04 — Overdue and Due Today labels](04-Overdue-And-Due-Today-Labels-User-Guide.md)
