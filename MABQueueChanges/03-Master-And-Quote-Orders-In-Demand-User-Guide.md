# Master Shells and Quotes No Longer Count as Demand

Last Updated: 2026-10-19

Infor Visual holds two kinds of work-order records that look like production but are not:
unreleased master (`M`) orders that act as make-to-stock planning shells, and quote (`Q`) orders
that are estimates. Both were being counted as material demand.

## What You Saw Before

The Associated Parts section listed parent part numbers such as `13042-7001`, `GM104683`, and
`SRE-30481` with entries like:

```text
No scheduled job
13042-7001
No scheduled date
```

Those rows came from unreleased master work orders created in 2019-2021. They had no schedule, no
issued quantity, and no scheduling activity, but they counted as open demand and pushed the real
work order off the card.

The scale was large. On 10/19/2026 MTMFG had 8,866 open component requirement rows, and 8,026 of
them (90%) belonged to master (`M`) work orders. 8,025 of those were unreleased shells: only 109
had a required date inside the last 90 days, and none had a future required date.

## What You See Now

Those planning shells and quotes are excluded from the Associated Parts demand list. For
`MMC0000386` the phantom `13042-7001` entry is gone and the listed parent part is the real one:

```text
Due Today
WO-075089
13042-0804
10/19/2026
```

Released master orders (`M` with status `R`) and closed history are still read, so nothing about
the "last time this ran" fallback changes.

## Why It Matters

About nine out of every ten "open" demands on the board were records that will never consume
material unless somebody releases them. Removing them makes the list short enough to trust. A
missing parent part on a card now means "no released work order needs this", not "a planner
forgot something".

## What You Should Know

- If your team does release work from master orders, those released orders (`M` status `R`) still
  appear normally. Only the never-released shells are hidden.
- If you want unreleased master shells back on the board as a planning signal, say so and I will
  add them as a separate, clearly labeled section instead of mixing them into live demand.
- Quote orders never had part-level requirements, so removing them has no visible effect today.

## See Also

- [01 — Closed jobs no longer appear as the next run](01-Closed-Jobs-Shown-As-Upcoming-User-Guide.md)
- [02 — Jobs without a schedule now show a real date](02-Missing-Job-Run-Date-Fallback-User-Guide.md)
