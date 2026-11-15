-- ============================================================================
-- Migration: 06_Migration_volvo_generated_label_queue
-- Module: Volvo
-- Purpose:
--   Add a dedicated active/history label-row queue for LabelView generation without
--   changing the existing Volvo shipment header and line tables.
-- ============================================================================

SOURCE Database/Database_Deployment/Sql_Files/Schemas/36_Table_volvo_generated_label_data.sql;
SOURCE Database/Database_Deployment/Sql_Files/Schemas/37_Table_volvo_generated_label_history.sql;
SOURCE Database/Database_Deployment/Sql_Files/StoredProcedures/Volvo/sp_Volvo_GeneratedLabelData_Insert.sql;
SOURCE Database/Database_Deployment/Sql_Files/StoredProcedures/Volvo/sp_Volvo_GeneratedLabelData_DeleteByShipment.sql;
SOURCE Database/Database_Deployment/Sql_Files/StoredProcedures/Volvo/sp_Volvo_GeneratedLabelData_GetAll.sql;
SOURCE Database/Database_Deployment/Sql_Files/StoredProcedures/Volvo/sp_Volvo_GeneratedLabelData_ClearToHistory.sql;