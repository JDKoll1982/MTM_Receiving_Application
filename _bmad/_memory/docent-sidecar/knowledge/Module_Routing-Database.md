---
module_name: Module_Routing
component: database-mapping
generated_on: 2026-01-09
analyst: Docent v1.0.0
---

# Module_Routing - Database Schema Details

Companion to:
- [_bmad/_memory/docent-sidecar/knowledge/Module_Routing.md](../docent-sidecar/knowledge/Module_Routing.md)

## MySQL Schema (Application Database)

Schema script:
- `Database/Schemas/10_schema_routing.sql`

Tables:

| Table | Purpose |
|-------|---------|
| routing_recipients | Recipient directory for routing labels |
| routing_other_reasons | Enumerated reasons for non-PO packages |
| routing_labels | Primary routing label records |
| routing_usage_tracking | Employee→recipient usage counts (Quick Add personalization) |
| routing_user_preferences | Default routing mode and validation toggle |
| routing_label_history | Audit trail of label edits |

## MySQL Stored Procedures

Module_Routing uses stored procedures via `Helper_Database_StoredProcedure`.

| Stored Procedure | Called By | Script Path |
|-----------------|----------|------------|
| sp_routing_label_insert | Dao_RoutingLabel.InsertLabelAsync | Database/StoredProcedures/sp_routing_label_insert.sql |
| sp_routing_label_update | Dao_RoutingLabel.UpdateLabelAsync | Database/StoredProcedures/sp_routing_label_update.sql |
| sp_routing_label_get_by_id | Dao_RoutingLabel.GetLabelByIdAsync | Database/StoredProcedures/sp_routing_label_get_by_id.sql |
| sp_routing_label_get_all | Dao_RoutingLabel.GetAllLabelsAsync | Database/StoredProcedures/sp_routing_label_get_all.sql |
| sp_routing_label_delete | Dao_RoutingLabel.DeleteLabelAsync | Database/StoredProcedures/sp_routing_label_delete.sql |
| sp_routing_label_mark_exported | Dao_RoutingLabel.MarkLabelExportedAsync | Database/StoredProcedures/sp_routing_label_mark_exported.sql |
| sp_routing_label_check_duplicate | Dao_RoutingLabel.CheckDuplicateLabelAsync | Database/StoredProcedures/sp_routing_label_check_duplicate.sql |
| sp_routing_label_history_insert | Dao_RoutingLabelHistory.InsertHistoryAsync | Database/StoredProcedures/sp_routing_label_history_insert.sql |
| sp_routing_label_history_get_by_label | Dao_RoutingLabelHistory.GetHistoryByLabelAsync | Database/StoredProcedures/sp_routing_label_history_get_by_label.sql |
| sp_routing_recipient_get_all_active | Dao_RoutingRecipient.GetAllActiveRecipientsAsync | Database/StoredProcedures/sp_routing_recipient_get_all_active.sql |
| sp_routing_recipient_get_top_by_usage | Dao_RoutingRecipient.GetTopRecipientsByUsageAsync | Database/StoredProcedures/sp_routing_recipient_get_top_by_usage.sql |
| sp_routing_other_reason_get_all_active | Dao_RoutingOtherReason.GetAllActiveReasonsAsync | Database/StoredProcedures/sp_routing_other_reason_get_all_active.sql |
| sp_routing_usage_increment | Dao_RoutingUsageTracking.IncrementUsageAsync | Database/StoredProcedures/sp_routing_usage_increment.sql |
| sp_routing_user_preference_get | Dao_RoutingUserPreference.GetUserPreferenceAsync | Database/StoredProcedures/sp_routing_user_preference_get.sql |
| sp_routing_user_preference_save | Dao_RoutingUserPreference.SaveUserPreferenceAsync | Database/StoredProcedures/sp_routing_user_preference_save.sql |

Note: Some routing stored procedure scripts also exist under `Database/StoredProcedures/Routing/` as duplicates.

## Reporting View Integration

The reporting view `view_routing_history` (used by Module_Reporting) is defined in:
- `Database/Schemas/views_01_create_reporting_views.sql`

It reads from:
- `routing_labels` joined to `routing_recipients` and `routing_other_reasons`.

## SQL Server (Infor Visual) - READ ONLY

Module_Routing integrates with Infor Visual ERP for PO validation and line retrieval.

- DAO: `Module_Routing/Data/Dao_InforVisualPO.cs`
- Provider: `Microsoft.Data.SqlClient`
- Access pattern: parameterized `SELECT` queries only
- Required warehouse/site filter: `SITE_REF = '002'`

Operational note:
- Service uses a connection check and will “gracefully degrade” by allowing the workflow to proceed even when Infor Visual is unavailable.
