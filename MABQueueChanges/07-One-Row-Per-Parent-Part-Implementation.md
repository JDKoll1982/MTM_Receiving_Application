# Implementation — One Best Row Per Parent Part

Last Updated: 2026-10-19

Status: **Applied**. File: `20_GetMaterialAvailabilityAssociatedPartRuns.sql`.

## Defect

The query returned every matching requirement row and relied on `TOP (@MaxResults)` with
`@MaxResults = 200`. `Service_Tool_MaterialAvailabilityBoard.BuildAssociatedPartPresentation`
immediately reduced that set with:

```csharp
.GroupBy(row => row.AssociatedPartNumber, StringComparer.OrdinalIgnoreCase)
.Select(group => group.OrderBy(priority...).First())
```

so all duplicate rows were wasted bandwidth, and the 200-row budget could be consumed before every
parent part was represented. A part whose only requirement rows were closed history silently
vanished from the card.

Measured on MTMFG, `@LocationId = 'T-00'`, 2026-10-19:

| Measure | Before | After |
| ------- | ------ | ----- |
| Rows leaving SQL with no cap | 9,400 | 418 |
| Distinct component/parent pairs | 418 | 418 |
| Open demand rows | 21 | 21 |

## Change

A ranking CTE now picks one row per `(InputPartNumber, AssociatedPartNumber)` before `TOP` is
applied:

```sql
RankedRequirements AS (
    SELECT
        mr.*,
        ROW_NUMBER() OVER (
            PARTITION BY mr.InputPartNumber, mr.AssociatedPartNumber
            ORDER BY
                CASE
                    WHEN mr.IsCurrentlyRunning = 1 THEN 0
                    WHEN mr.IsPastJob = 1 THEN 5
                    WHEN mr.NextDueToRunDate IS NULL THEN 4
                    WHEN mr.NextDueToRunDate < CAST(GETDATE() AS date) THEN 1
                    ELSE 2
                END,
                CASE WHEN mr.IsPastJob = 0 THEN mr.NextDueToRunDate END,
                CASE WHEN mr.IsPastJob = 1
                     THEN COALESCE(mr.ScheduledFinishDate, mr.NextDueToRunDate)
                END DESC,
                mr.WorkOrderBaseId,
                mr.OperationSeqNo,
                mr.RequirementPieceNo
        ) AS DemandRank
    FROM MatchingRequirements mr
)
...
FROM RankedRequirements
WHERE DemandRank = 1
ORDER BY ...
```

The bucket expression is identical to `GetAssociatedPartPriority` in the service, so SQL and C#
agree on which row is "best". `DemandRank` is not projected, so the result shape is unchanged and
no DAO mapping work was needed.

## Why This Is Safe

- The service already discarded all but one row per parent part; the selected row is the same one
  it would have chosen.
- `@MaxResults` is still applied, but now it bounds distinct parent parts rather than duplicates.
- Unit tests feed multiple rows per parent part straight into the service and still exercise its
  own grouping, so both layers are covered.

## Verification

```shell
sqlcmd -S VISUAL -d MTMFG -U JKOLL -P *** -C -i "$env:TEMP\mab_t00.sql" -W -s "|"
```

- `@LocationId = 'T-00'` returns 200 rows (capped) with 21 open-demand rows retained.
- `@PartId = 'MMC0000386'` returns exactly one row: `WO-075089`, `IsOpenDemand = 1`,
  `NextDueToRunDate = 2026-10-19`, `NextDueDateSource = REQUIREMENT.REQUIRED_DATE`.
- `dotnet test` — 836 passed.
- `Helper_SqlQueryLoaderTests.LoadAndPrepareQuery_ShouldKeepInputPartNumberInAssociatedPartRunsProjection`
  guarded on `FROM MatchingRequirements`; it now guards on `FROM RankedRequirements`. Its intent
  (the projection keeps `InputPartNumber`) is unchanged.

## See Also

- [07 — Reader guide](07-One-Row-Per-Parent-Part-User-Guide.md)
