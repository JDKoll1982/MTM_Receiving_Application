# Implementation — Closed Jobs No Longer Show as the Next Run

Last Updated: 2026-10-19

Status: **Applied**. Files: `20_GetMaterialAvailabilityAssociatedPartRuns.sql`,
`Service_Tool_MaterialAvailabilityBoard.cs`, `Model_Tool_MaterialAvailabilityAssociatedPartRun.cs`.

## Defect

`IsPastJob` was derived from the calendar, not from the ERP status:

```sql
CASE
    WHEN ISNULL(r.STATUS,'') IN ('C','X')
      OR ISNULL(wo.STATUS,'') IN ('C','X')
      OR COALESCE(op.SCHED_FINISH_DATE, wo.SCHED_FINISH_DATE, r.CLOSE_DATE, wo.CLOSE_DATE) < CAST(GETDATE() AS date)
        THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS IsPastJob
```

The service then ranked `IsFutureOrTodayRun = 1` before `IsPastJob = 1` before "anything else".
An open requirement with no schedule date satisfied none of those flags, so it landed in the last
bucket. Because `BuildAssociatedPartPresentation` keeps only the first row per
`AssociatedPartNumber`, the closed history row won and the open requirement was dropped from the
card.

Measured on MTMFG, 2026-10-19: 232,567 of the requirement rows are `STATUS = 'C'` (closed), so the
history set dominates the result set.

## Change

`IsPastJob` now means "closed or cancelled", never "the date has passed":

```sql
CASE
    WHEN ISNULL(r.STATUS,'') IN ('C','X')
      OR ISNULL(wo.STATUS,'') IN ('C','X')
        THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS IsPastJob
```

`IsOpenDemand` is emitted alongside it:

```sql
CASE
    WHEN ISNULL(r.STATUS,'') NOT IN ('C','X')
     AND ISNULL(wo.STATUS,'') NOT IN ('C','X')
        THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS IsOpenDemand
```

The service ranking in `GetAssociatedPartPriority` is now date and status driven:

```csharp
if (row.IsCurrentlyRunning) return 0;
if (row.IsPastJob) return 5;
if (!row.NextDueToRunDate.HasValue) return 4;

var runDate = row.NextDueToRunDate.Value.Date;
if (runDate < DateTime.Today) return 1;

return incomingWindowDays.HasValue && runDate > horizon ? 3 : 2;
```

`GetAssociatedPartSortBucket` mirrors the same order so the card header and the card sort agree.
`GetAssociatedPartFutureSortDate` no longer depends on `IsFutureOrTodayRun`; it uses
`IsPastJob == false && NextDueToRunDate.HasValue`, so a date sourced from `REQUIREMENT.REQUIRED_DATE`
still sorts correctly. The SQL `ORDER BY` was rewritten to the same buckets.

`IsOpenDemand` was added to `Model_InforVisualAssociatedPartRunRow`,
`Model_Tool_MaterialAvailabilityAssociatedPartRun`, the DAO reader, and
`Service_InforVisualMockDataCatalog.NormalizeAssociatedPartRun`.

## Verification

- `sqlcmd -S VISUAL -d MTMFG -U <user> -C -i 20_GetMaterialAvailabilityAssociatedPartRuns.sql`
  with `@LocationId = 'RECV'` returns `WO-074826` first for `MMC0000990` and `WO-075089` first
  for `MMC0000386`. Before the change the first rows were closed 2025 work orders.
- `dotnet test ... --filter "FullyQualifiedName~MaterialAvailability"` — 37 passed.
- Full suite — 836 passed, 0 failed.

Two regression tests were added to
`MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/Services/Service_Tool_MaterialAvailabilityBoardTests.cs`:

- `GetBoardByPartAsync_ShouldPreferOpenDemandWithoutScheduleOverClosedHistory`
- `GetBoardByPartAsync_ShouldLabelOpenPastDueDemandAsOverdue`

## Rollback

Revert the `IsPastJob` case block and restore the previous `GetAssociatedPartPriority` ordering.
Navigation and print output do not depend on the new ordering contract.

## See Also

- [01 — Reader guide](01-Closed-Jobs-Shown-As-Upcoming-User-Guide.md)
