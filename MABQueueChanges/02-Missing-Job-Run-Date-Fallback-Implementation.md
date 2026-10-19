# Implementation — Jobs Without a Schedule Now Show a Real Date

Last Updated: 2026-10-19

Status: **Applied**. Files: `20_GetMaterialAvailabilityAssociatedPartRuns.sql`,
`Model_InforVisualAssociatedPartRunRow.cs` (documentation only).

## Defect

`NextDueToRunDate` only considered schedule columns:

```sql
COALESCE(op.SCHED_START_DATE, wo.SCHED_START_DATE) AS NextDueToRunDate
```

Evidence from MTMFG on 2026-10-19, counting open requirements (`r.STATUS IN ('U','R')` and
`wo.STATUS IN ('U','R')`):

| Measure | Rows |
| ------- | ---- |
| Open requirement rows | 8,866 |
| Missing operation and work-order start date | 8,209 (93%) |
| Missing `REQUIREMENT.REQUIRED_DATE` | 790 |
| Missing `WORK_ORDER.DESIRED_WANT_DATE` | small |

So 93% of live demand rendered as "No scheduled date", and — before change 01 — sorted into the
discard bucket.

## Change

```sql
COALESCE(
    op.SCHED_START_DATE,
    wo.SCHED_START_DATE,
    r.REQUIRED_DATE,
    wo.DESIRED_WANT_DATE
)                                   AS NextDueToRunDate,
CASE
    WHEN op.SCHED_START_DATE IS NOT NULL THEN 'OPERATION.SCHED_START_DATE'
    WHEN wo.SCHED_START_DATE IS NOT NULL THEN 'WORK_ORDER.SCHED_START_DATE'
    WHEN r.REQUIRED_DATE IS NOT NULL THEN 'REQUIREMENT.REQUIRED_DATE'
    WHEN wo.DESIRED_WANT_DATE IS NOT NULL THEN 'WORK_ORDER.DESIRED_WANT_DATE'
    ELSE 'NO SCHEDULED START DATE'
END                                 AS NextDueDateSource,
```

Column names were confirmed against `INFORMATION_SCHEMA.COLUMNS`: `REQUIREMENT.REQUIRED_DATE`,
`REQUIREMENT.DUE_DATE`, `WORK_ORDER.DESIRED_WANT_DATE`, `WORK_ORDER.SCHED_START_DATE`,
`WORK_ORDER.SCHED_FINISH_DATE`.

`IsFutureOrTodayRun` and `IsCurrentlyRunning` deliberately still use only the two schedule
columns. A required date is a need-by date, not a start-of-run date, so it must not make a job
look like it is currently running.

`ScheduledStartDate` and `ScheduledFinishDate` are unchanged, so `NextRunDateDisplay`
(`ScheduledStartDate ?? NextDueToRunDate`) still shows the true scheduled start when one exists.

## Consequences to Watch

- Because the existing filter keeps closed requirements for the history fallback, closed rows with
  no schedule now also carry a fallback date. That is harmless: they are bucket 5 and are drawn
  from `ScheduledFinishDate ?? NextDueToRunDate` for the "most recent history" ordering.
- Unreleased master orders would now be dated in 2019-2021 and read as `Overdue`, which is why
  change 03 is part of the same slice.

## Verification

```shell
sqlcmd -S VISUAL -d MTMFG -U JKOLL -P *** -C -i "$env:TEMP\mab_runs2.sql"
```

`WO-075089` returns `NextDueToRunDate = 2026-10-19` with
`NextDueDateSource = REQUIREMENT.REQUIRED_DATE`.

## See Also

- [02 — Reader guide](02-Missing-Job-Run-Date-Fallback-User-Guide.md)
