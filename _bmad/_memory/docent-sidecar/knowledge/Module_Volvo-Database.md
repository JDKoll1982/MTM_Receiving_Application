---
module_name: Module_Volvo
component: database-mapping
generated_on: 2026-01-09
analyst: Docent v1.0.0
---

# Module_Volvo - Database Schema Details

Companion to:
- [_bmad/_memory/docent-sidecar/knowledge/Module_Volvo.md](../docent-sidecar/knowledge/Module_Volvo.md)

## MySQL Stored Procedures (Observed in Code)

### Volvo Shipments

| DAO | Stored Procedure | Script Path |
|-----|------------------|-------------|
| Dao_VolvoShipment | sp_volvo_shipment_insert | Database/StoredProcedures/Volvo/sp_volvo_shipment_insert.sql |
| Dao_VolvoShipment | sp_volvo_shipment_update | Database/StoredProcedures/Volvo/sp_volvo_shipment_update.sql |
| Dao_VolvoShipment | sp_volvo_shipment_complete | Database/StoredProcedures/Volvo/sp_volvo_shipment_complete.sql |
| Dao_VolvoShipment | sp_volvo_shipment_delete | Database/StoredProcedures/Volvo/sp_volvo_shipment_delete.sql |
| Dao_VolvoShipment | sp_volvo_shipment_get_pending | Database/StoredProcedures/Volvo/sp_volvo_shipment_get_pending.sql |
| Dao_VolvoShipment | sp_volvo_shipment_history_get | Database/StoredProcedures/Volvo/sp_volvo_shipment_history_get.sql |

### Volvo Shipment Lines

| DAO | Stored Procedure | Script Path |
|-----|------------------|-------------|
| Dao_VolvoShipmentLine | sp_volvo_shipment_line_insert | Database/StoredProcedures/Volvo/sp_volvo_shipment_line_insert.sql |
| Dao_VolvoShipmentLine | sp_volvo_shipment_line_get_by_shipment | Database/StoredProcedures/Volvo/sp_volvo_shipment_line_get_by_shipment.sql |
| Dao_VolvoShipmentLine | sp_volvo_shipment_line_update | Database/StoredProcedures/Volvo/sp_volvo_shipment_line_update.sql |
| Dao_VolvoShipmentLine | sp_volvo_shipment_line_delete | Database/StoredProcedures/Volvo/sp_volvo_shipment_line_delete.sql |

### Volvo Parts Master

| DAO | Stored Procedure | Script Path |
|-----|------------------|-------------|
| Dao_VolvoPart | sp_volvo_part_master_get_all | Database/StoredProcedures/Volvo/sp_volvo_part_master_get_all.sql |
| Dao_VolvoPart | sp_volvo_part_master_get_by_id | Database/StoredProcedures/Volvo/sp_volvo_part_master_get_by_id.sql |
| Dao_VolvoPart | sp_volvo_part_master_insert | Database/StoredProcedures/Volvo/sp_volvo_part_master_insert.sql |
| Dao_VolvoPart | sp_volvo_part_master_update | Database/StoredProcedures/Volvo/sp_volvo_part_master_update.sql |
| Dao_VolvoPart | sp_volvo_part_master_set_active | Database/StoredProcedures/Volvo/sp_volvo_part_master_set_active.sql |
| Dao_VolvoPart | sp_volvo_part_check_references | Database/StoredProcedures/Module_Volvo/sp_volvo_part_check_references.sql |

### Volvo Part Components

| DAO | Stored Procedure | Script Path |
|-----|------------------|-------------|
| Dao_VolvoPartComponent | sp_volvo_part_component_get | Database/StoredProcedures/Volvo/sp_volvo_part_component_get.sql |
| Dao_VolvoPartComponent | sp_volvo_part_component_insert | Database/StoredProcedures/Volvo/sp_volvo_part_component_insert.sql |
| Dao_VolvoPartComponent | sp_volvo_part_component_delete_by_parent | Database/StoredProcedures/Volvo/sp_volvo_part_component_delete_by_parent.sql |

### Volvo Settings

| DAO | Stored Procedure | Script Path |
|-----|------------------|-------------|
| Dao_VolvoSettings | sp_volvo_settings_get | Database/StoredProcedures/Volvo/sp_volvo_settings_get.sql |
| Dao_VolvoSettings | sp_volvo_settings_get_all | Database/StoredProcedures/Volvo/sp_volvo_settings_get_all.sql |
| Dao_VolvoSettings | sp_volvo_settings_upsert | Database/StoredProcedures/Volvo/sp_volvo_settings_upsert.sql |
| Dao_VolvoSettings | sp_volvo_settings_reset | Database/StoredProcedures/Volvo/sp_volvo_settings_reset.sql |

## Notes / Gaps

- Some DAO operations still use direct `MySqlCommand` and/or raw SQL text (documented as an architectural deviation; AM generation does not modify code).
- Module_Volvo appears MySQL-focused; no Infor Visual (SQL Server) read-only queries were observed during this scan.
