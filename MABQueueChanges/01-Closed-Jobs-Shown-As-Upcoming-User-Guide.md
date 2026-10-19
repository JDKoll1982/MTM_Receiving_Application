# Closed Jobs No Longer Show as the Next Run

Last Updated: 2026-10-19

A finished job was being picked as the "next run" for a part, even when a brand-new work order
for that same part was already released and waiting for material.

## What You Saw Before

You searched location `RECV` in warehouse `002`. The card for `MMC0000386` (Coil, 13Ga X 6.125)
said:

```text
Next Run Summary
Past Job: WO-063104 / 13042-0804 on 05/30/2025
```

That work order was finished and closed in May of 2025. Meanwhile `WO-075089` had been released
that same morning, needed 13,739.96 units of that coil, and had issued nothing yet. It did not
appear anywhere on the card.

The same thing happened on the other parts in that location: cards reported a job from 2021 or
2019 as the current run while open work orders sat in the system.

## What You See Now

```text
Next Run Summary
Due Today: WO-075089 / 13042-0804 on 10/19/2026 | Required Parts: 105,692 | Estimated Coil Use: 13,740
```

The card shows the job that still needs the material. Finished jobs are kept only as a
fallback for when nothing is open at all, and they are always labeled as past jobs.

Ranking order is now, top to bottom:

1. A job running right now.
2. An open job that is already past its due date.
3. An open job due within your Look Ahead window, soonest first.
4. An open job due beyond your Look Ahead window, soonest first.
5. An open job with no usable date at all.
6. A finished job, most recent first.

## Why It Matters

Receiving and production use this card to decide whether to release, move, or hold a coil. A
year-old closed job on the card tells you nothing about the material that is about to be
consumed today. The board now answers the question you actually ask: "what is the next job that
will need this material, and when".

## What You Might Notice

- Some cards now show an older or a shorter list than before, because closed jobs no longer
  crowd out the open ones.
- When a part has no open demand at all, the card still shows its most recent finished job, and
  the label says `Past Job` so you know it is history rather than a plan.
- Part numbers that only ever appeared through an unreleased master order may disappear from the
  Associated Parts list entirely. That is covered in change 03.

## See Also

- [03 — Master shells and quotes no longer count as demand](03-Master-And-Quote-Orders-In-Demand-User-Guide.md)
- [04 — Overdue and Due Today labels](04-Overdue-And-Due-Today-Labels-User-Guide.md)
