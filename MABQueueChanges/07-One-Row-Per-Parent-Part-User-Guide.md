# One Best Row Per Parent Part

Last Updated: 2026-10-19

Status: **Applied**. The Associated Parts list could be filled with dozens of copies of the same
job, and some parts showed no associated part at all even though they had a run history.

## What You Saw Before

A component part like a coil has a requirement row for every piece, every operation that consumes
it, and every year it has been used. For location `T-00` the board pulled 9,400 raw requirement
rows for 53 parts to fill a list that can hold 200. Because the same job appeared many times, the
200-row budget was spent on duplicates, and parts whose only rows were finished jobs came back
with no Associated Parts entry — as if the part had never run.

## What You See Now

Exactly one row per parent part, chosen by the ranking from changes 01, 02, and 04. For `T-00` the
query now returns 418 distinct rows instead of 9,400, and every part with any history gets a row.

## Why It Matters

- A part that has run before no longer shows "No associated parts were found".
- The row you see is always the most relevant one — running job first, then overdue, then soonest
  due, then the most recent finished job.
- The board loads faster because it moves about 22 times fewer rows out of Infor Visual for a busy
  location.

## What You Should Know

- The card still lists one parent part per line. If you need to see every work order that consumes
  a part, that is a different view and not part of this change.
- A parent part that appears in the list is the one that determines the summary text at the top of
  the card.

## See Also

- [01 — Closed jobs no longer appear as the next run](01-Closed-Jobs-Shown-As-Upcoming-User-Guide.md)
- [04 — Overdue and Due Today labels](04-Overdue-And-Due-Today-Labels-User-Guide.md)
