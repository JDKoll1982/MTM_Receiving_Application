---
module_name: Module_Reporting
component: database-mapping
generated_on: 2026-01-09
analyst: Docent v1.0.0
---

# Module_Reporting - Database Schema Details

Companion to:
- [_bmad/_memory/docent-sidecar/knowledge/Module_Reporting.md](../docent-sidecar/knowledge/Module_Reporting.md)

## MySQL Views (Reporting)

These views are defined by the schema script:

- `Database/Schemas/views_01_create_reporting_views.sql`

Views consumed by `Dao_Reporting`:

| View | Consumed By | Purpose |
|------|------------|---------|
| vw_receiving_history | GetReceivingHistoryAsync | Unified receiving transaction history for EoD reporting |
| vw_dunnage_history | GetDunnageHistoryAsync | Unified dunnage load history including specs concatenation |
| vw_routing_history | GetRoutingHistoryAsync | Unified routing label history |
| vw_volvo_history | GetVolvoHistoryAsync | Placeholder/initial integration surface for Volvo |

## Availability Counts

- `Dao_Reporting.CheckAvailabilityAsync` computes record counts by querying each view between the selected dates.
- Volvo is currently forced to 0 in availability checks (even though `vw_volvo_history` is defined). This matches the module README’s “placeholder view” stance.

## Notes

- Module_Reporting is read-only and queries views with parameterized SQL.
- No MySQL stored procedures are referenced by the Reporting module’s DAO.
