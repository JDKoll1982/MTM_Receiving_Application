# Implementation — Overdue and Due Today Labels

Last Updated: 2026-10-19

Status: **Applied**. Files: `20_GetMaterialAvailabilityAssociatedPartRuns.sql`,
`Model_Tool_MaterialAvailabilityAssociatedPartRun.cs`.

## Defect 1 — today was excluded from "future or today"

```sql
COALESCE(op.SCHED_START_DATE, wo.SCHED_START_DATE) > CAST(GETDATE() AS date)
```

The name says "future or today" but the operator excluded today. A job starting today with no
finish date satisfied neither `IsFutureOrTodayRun` nor `IsCurrentlyRunning`
(`... <= today AND finish >= today` fails when `finish` is null), so it fell through to the
discard bucket.

Change:

```sql
COALESCE(op.SCHED_START_DATE, wo.SCHED_START_DATE) >= CAST(GETDATE() AS date)
```

## Defect 2 — an open, past-due job was labeled "Past Job"

`RunTimingLabel` only knew four states and had no concept of overdue:

```csharp
public string RunTimingLabel =>
    NextDueToRunDate.HasValue is false ? "No scheduled job"
    : IsCurrentlyRunning ? "Currently Running"
    : IsPastJob ? "Past Job"
    : "Future Run";
```

Because change 01 redefines `IsPastJob` as "closed or cancelled", an open past-due row would have
fallen through to `Future Run` with a date in the past. The label is now:

```csharp
public string RunTimingLabel =>
    NextDueToRunDate.HasValue is false ? "No scheduled job"
    : IsCurrentlyRunning ? "Currently Running"
    : IsPastJob ? "Past Job"
    : NextDueToRunDate.Value.Date < DateTime.Today ? "Overdue"
    : NextDueToRunDate.Value.Date == DateTime.Today ? "Due Today"
    : "Future Run";
```

## Display Contract

| Flag combination | Label |
| ---------------- | ----- |
| `IsCurrentlyRunning` | `Currently Running` |
| `IsPastJob` | `Past Job` |
| no `NextDueToRunDate` | `No scheduled job` |
| date `< DateTime.Today` | `Overdue` |
| date `== DateTime.Today` | `Due Today` |
| otherwise | `Future Run` |

`NextRunSummaryDisplay` on the card and `AssociatedPartPresentation.summaryText` both consume
`RunTimingLabel`, so the header, the row, and the print output stay in step.

## Tests

- `GetBoardByPartAsync_ShouldUseScheduledStartDateAndCurrentRunLabelWhenJobIsRunning` still
  expects `Currently Running`.
- `GetBoardByPartAsync_ShouldKeepAssociatedPartsVisibleWhenNoScheduledRunDateExists` still expects
  `No scheduled job`.
- `GetBoardByPartAsync_ShouldLabelOpenPastDueDemandAsOverdue` (new) asserts `Overdue` plus the
  summary text.
- `GetBoardByPartAsync_ShouldPreferOpenDemandWithoutScheduleOverClosedHistory` (new) asserts
  `Due Today`.

## See Also

- [04 — Reader guide](04-Overdue-And-Due-Today-Labels-User-Guide.md)
