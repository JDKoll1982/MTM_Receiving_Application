# Implementation — Master Shells and Quotes No Longer Count as Demand

Last Updated: 2026-10-19

Status: **Applied** (business rule, reversible with one predicate). File:
`20_GetMaterialAvailabilityAssociatedPartRuns.sql`.

## Defect

Nothing filtered work-order type. The only status exclusions were `'X'` (cancelled):

```sql
WHERE ISNULL(r.STATUS, '') <> 'X'
  AND ISNULL(wo.STATUS, '') <> 'X'
```

`WORK_ORDER.TYPE` values in MTMFG and their open-requirement contribution on 2026-10-19:

| Type | WO status | Open requirement rows | Within last 90 days | Future-dated |
| ---- | --------- | --------------------- | ------------------- | ------------ |
| M | U | 8,025 | 109 | 0 |
| M | R | 1 | 0 | 0 |
| W | R | 829 | 821 | 360 |
| W | U | 7 | 0 | 0 |
| Q | U | 4 | 0 | 0 |

`Q` rows carry no `PART_ID` (base ids look like `CQ-016245-19`), so they are quotes. `M` rows are
master work orders whose `BASE_ID` equals the finished part; the unreleased population is a
make-to-stock planning shell that has never been scheduled.

## Change

```sql
      -- Quote work orders never represent real production demand.
      AND wo.TYPE <> 'Q'
      -- Unreleased master ('M') work orders are make-to-stock planning shells. 8025 of the 8866
      -- open requirement rows in MTMFG are these shells, 98.6% of them dated more than 90 days
      -- ago, and they otherwise outrank the real released ('W') work orders.
      AND NOT (wo.TYPE = 'M' AND ISNULL(wo.STATUS, '') = 'U')
```

Deliberately kept:

- `M` rows with status `R` (released master) — real work.
- `M` rows with status `C` — needed for the latest-historical-run fallback.
- `W` rows with status `U` — rare (7 rows) but genuinely open work orders.

## Decision Owner Question

The exclusion of unreleased `M` shells is a business rule, not a pure defect. If the team wants
those rows visible as forward planning signal, the recommended alternative is a new
`wo.TYPE = 'M' AND wo.STATUS = 'U'` group rendered in its own labeled section, not mixed into the
demand ranking.

## Verification

- Row count for `@LocationId = 'RECV'` dropped from 68 to 66 and the `13042-7001` parent
  disappeared from the output.
- `dotnet test ... --filter "FullyQualifiedName~MaterialAvailability"` — 37 passed.
- Full suite — 836 passed.

## See Also

- [03 — Reader guide](03-Master-And-Quote-Orders-In-Demand-User-Guide.md)
