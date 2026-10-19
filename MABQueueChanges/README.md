# Material Availability Board — Change Queue Index

Last Updated: 2026-10-19

Review of how the Associated Parts job run dates and the inbound "next delivery date" are
calculated. Each numbered item has two files: a plain-language reader guide and an
implementation note. Items marked **Applied** are already in the code; items marked
**Awaiting approval** are proposals only.

## Change Queue

| # | Change | Status | Reader guide | Implementation note |
| - | ------ | ------ | ------------ | ------------------- |
| 01 | Closed jobs no longer appear as the next run | Applied | [01](01-Closed-Jobs-Shown-As-Upcoming-User-Guide.md) | [01](01-Closed-Jobs-Shown-As-Upcoming-Implementation.md) |
| 02 | Jobs without a schedule now show a real date | Applied | [02](02-Missing-Job-Run-Date-Fallback-User-Guide.md) | [02](02-Missing-Job-Run-Date-Fallback-Implementation.md) |
| 03 | Master shells and quotes no longer count as demand | Applied | [03](03-Master-And-Quote-Orders-In-Demand-User-Guide.md) | [03](03-Master-And-Quote-Orders-In-Demand-Implementation.md) |
| 04 | "Overdue" and "Due Today" labels added | Applied | [04](04-Overdue-And-Due-Today-Labels-User-Guide.md) | [04](04-Overdue-And-Due-Today-Labels-Implementation.md) |
| 05 | Next delivery date tells you what it really means | Applied | [05](05-Next-Delivery-Date-Accuracy-User-Guide.md) | [05](05-Next-Delivery-Date-Accuracy-Implementation.md) |
| 06 | Look Ahead becomes a highlight window | Applied (Option B) | [06](06-LookAhead-Window-For-Inbound-Material-User-Guide.md) | [06](06-LookAhead-Window-For-Inbound-Material-Implementation.md) |
| 07 | One best row per parent part | Applied | [07](07-One-Row-Per-Parent-Part-User-Guide.md) | [07](07-One-Row-Per-Parent-Part-Implementation.md) |
| 08 | Inbound POs with no warehouse now show | Applied | [08](08-Inbound-Without-Warehouse-User-Guide.md) | [08](08-Inbound-Without-Warehouse-Implementation.md) |

## Root Cause in One Paragraph

The board treated "the job's date has passed" as "the job is history", and treated every
requirement row in Infor Visual as live demand. In MTMFG, 232,567 of roughly 247,000
requirement rows are closed history, and 8,025 of the 8,866 genuinely open rows belong to
unreleased master work orders that are mostly years old. Those two effects pushed real,
released work orders down the ranking, so the board most often showed an old finished job —
or no date at all — instead of the job that is about to consume the material.

## See Also

- `Database/InforVisualScripts/Queries/20_GetMaterialAvailabilityAssociatedPartRuns.sql`
- `Database/InforVisualScripts/Queries/19_GetMaterialAvailabilityIncomingSupply.sql`
- `Module_ShipRec_Tools/Services/Service_Tool_MaterialAvailabilityBoard.cs`
