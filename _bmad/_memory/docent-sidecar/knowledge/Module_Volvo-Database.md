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
| Dao_VolvoShipment | sp_Volvo_Shipment_GetPending | Database/StoredProcedures/Volvo/sp_Volvo_Shipment_GetPending.sql |
| Dao_VolvoShipment | sp_Volvo_Shipment_GetHistory | Database/StoredProcedures/Volvo/sp_Volvo_Shipment_GetHistory.sql |

### Volvo Shipment Lines

| DAO | Stored Procedure | Script Path |
|-----|------------------|-------------|
| Dao_VolvoShipmentLine | sp_Volvo_ShipmentLine_Insert | Database/StoredProcedures/Volvo/sp_Volvo_ShipmentLine_Insert.sql |
| Dao_VolvoShipmentLine | sp_Volvo_ShipmentLine_GetByShipment | Database/StoredProcedures/Volvo/sp_Volvo_ShipmentLine_GetByShipment.sql |
| Dao_VolvoShipmentLine | sp_Volvo_ShipmentLine_Update | Database/StoredProcedures/Volvo/sp_Volvo_ShipmentLine_Update.sql |
| Dao_VolvoShipmentLine | sp_Volvo_ShipmentLine_Delete | Database/StoredProcedures/Volvo/sp_Volvo_ShipmentLine_Delete.sql |

### Volvo Parts Master

| DAO | Stored Procedure | Script Path |
|-----|------------------|-------------|
| Dao_VolvoPart | sp_Volvo_PartMaster_GetAll | Database/StoredProcedures/Volvo/sp_Volvo_PartMaster_GetAll.sql |
| Dao_VolvoPart | sp_Volvo_PartMaster_GetById | Database/StoredProcedures/Volvo/sp_Volvo_PartMaster_GetById.sql |
| Dao_VolvoPart | sp_Volvo_PartMaster_Insert | Database/StoredProcedures/Volvo/sp_Volvo_PartMaster_Insert.sql |
| Dao_VolvoPart | sp_Volvo_PartMaster_Update | Database/StoredProcedures/Volvo/sp_Volvo_PartMaster_Update.sql |
| Dao_VolvoPart | sp_Volvo_PartMaster_SetActive | Database/StoredProcedures/Volvo/sp_Volvo_PartMaster_SetActive.sql |
| Dao_VolvoPart | sp_volvo_part_check_references | Database/StoredProcedures/Module_Volvo/sp_volvo_part_check_references.sql |

### Volvo Part Components

| DAO | Stored Procedure | Script Path |
|-----|------------------|-------------|
| Dao_VolvoPartComponent | sp_Volvo_PartComponent_Get | Database/StoredProcedures/Volvo/sp_Volvo_PartComponent_Get.sql |
| Dao_VolvoPartComponent | sp_Volvo_PartComponent_Insert | Database/StoredProcedures/Volvo/sp_Volvo_PartComponent_Insert.sql |
| Dao_VolvoPartComponent | sp_Volvo_PartComponent_DeleteByParent | Database/StoredProcedures/Volvo/sp_Volvo_PartComponent_DeleteByParent.sql |

### Volvo Settings

| DAO | Stored Procedure | Script Path |
|-----|------------------|-------------|
| Dao_VolvoSettings | sp_Volvo_Settings_Get | Database/StoredProcedures/Volvo/sp_Volvo_Settings_Get.sql |
| Dao_VolvoSettings | sp_Volvo_Settings_GetAll | Database/StoredProcedures/Volvo/sp_Volvo_Settings_GetAll.sql |
| Dao_VolvoSettings | sp_Volvo_Settings_Upsert | Database/StoredProcedures/Volvo/sp_Volvo_Settings_Upsert.sql |
| Dao_VolvoSettings | sp_Volvo_Settings_Reset | Database/StoredProcedures/Volvo/sp_Volvo_Settings_Reset.sql |

## Notes / Gaps

- Some DAO operations still use direct `MySqlCommand` and/or raw SQL text (documented as an architectural deviation; AM generation does not modify code).
- Module_Volvo appears MySQL-focused; no Infor Visual (SQL Server) read-only queries were observed during this scan.
