-- ============================================================================
-- Migration: 08_Migration_label_queue_employee_scoping
-- Modules: Dunnage, Receiving, Volvo
-- Purpose:
--   Persist employee_number on active label queues that do not yet store it and
--   update Clear Label Data procedures so a normal clear archives only the active
--   user's rows while Shift+Click admin/developer clears can archive all rows.
-- ============================================================================

DROP PROCEDURE IF EXISTS sp_mig44_add_column_if_missing;
DROP PROCEDURE IF EXISTS sp_mig44_add_index_if_missing;
DROP PROCEDURE IF EXISTS sp_mig44_apply;

DELIMITER $$

CREATE PROCEDURE sp_mig44_add_column_if_missing(
    IN p_table_name  VARCHAR(64),
    IN p_column_name VARCHAR(64),
    IN p_ddl         TEXT
)
BEGIN
    DECLARE v_exists INT DEFAULT 0;

    SELECT COUNT(*)
    INTO v_exists
    FROM information_schema.columns
    WHERE table_schema = DATABASE()
      AND table_name = p_table_name
      AND column_name = p_column_name;

    IF v_exists = 0 THEN
        SET @ddl_sql = p_ddl;
        PREPARE stmt FROM @ddl_sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
END $$

CREATE PROCEDURE sp_mig44_add_index_if_missing(
    IN p_table_name VARCHAR(64),
    IN p_index_name VARCHAR(64),
    IN p_ddl        TEXT
)
BEGIN
    DECLARE v_exists INT DEFAULT 0;

    SELECT COUNT(*)
    INTO v_exists
    FROM information_schema.statistics
    WHERE table_schema = DATABASE()
      AND table_name = p_table_name
      AND index_name = p_index_name;

    IF v_exists = 0 THEN
        SET @ddl_sql = p_ddl;
        PREPARE stmt FROM @ddl_sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
END $$

CREATE PROCEDURE sp_mig44_apply()
BEGIN
    CALL sp_mig44_add_column_if_missing('dunnage_label_data', 'employee_number',
        'ALTER TABLE dunnage_label_data ADD COLUMN employee_number INT NULL COMMENT ''4-digit employee identifier for the user who saved the queue row'' AFTER user_id');

    CALL sp_mig44_add_index_if_missing('dunnage_label_data', 'idx_employee_number',
        'CREATE INDEX idx_employee_number ON dunnage_label_data (employee_number)');

    CALL sp_mig44_add_column_if_missing('dunnage_history', 'employee_number',
        'ALTER TABLE dunnage_history ADD COLUMN employee_number INT NULL COMMENT ''4-digit employee identifier preserved from the active queue row'' AFTER created_by');

    CALL sp_mig44_add_index_if_missing('dunnage_history', 'IDX_LOADS_EMPLOYEE',
        'CREATE INDEX IDX_LOADS_EMPLOYEE ON dunnage_history (employee_number)');

    CALL sp_mig44_add_column_if_missing('volvo_generated_label_data', 'employee_number',
        'ALTER TABLE volvo_generated_label_data ADD COLUMN employee_number INT NULL COMMENT ''4-digit employee identifier for the user whose shipment generated this label row'' AFTER part_description');

    CALL sp_mig44_add_index_if_missing('volvo_generated_label_data', 'idx_volvo_generated_label_employee_number',
        'CREATE INDEX idx_volvo_generated_label_employee_number ON volvo_generated_label_data (employee_number)');

    CALL sp_mig44_add_column_if_missing('volvo_generated_label_history', 'employee_number',
        'ALTER TABLE volvo_generated_label_history ADD COLUMN employee_number INT NULL COMMENT ''4-digit employee identifier preserved from the active generated label row'' AFTER part_description');

    CALL sp_mig44_add_index_if_missing('volvo_generated_label_history', 'idx_volvo_generated_label_history_employee_number',
        'CREATE INDEX idx_volvo_generated_label_history_employee_number ON volvo_generated_label_history (employee_number)');
END $$

DELIMITER ;

CALL sp_mig44_apply();

DROP PROCEDURE IF EXISTS sp_mig44_apply;
DROP PROCEDURE IF EXISTS sp_mig44_add_index_if_missing;
DROP PROCEDURE IF EXISTS sp_mig44_add_column_if_missing;

SOURCE Database/Database_Deployment/Sql_Files/StoredProcedures/Dunnage/sp_Dunnage_LabelData_Insert.sql;
SOURCE Database/Database_Deployment/Sql_Files/StoredProcedures/Dunnage/sp_Dunnage_LabelData_Update.sql;
SOURCE Database/Database_Deployment/Sql_Files/StoredProcedures/Dunnage/sp_Dunnage_LabelData_GetAll.sql;
SOURCE Database/Database_Deployment/Sql_Files/StoredProcedures/Dunnage/sp_Dunnage_LabelData_ClearToHistory.sql;
SOURCE Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/sp_Receiving_LabelData_ClearToHistory.sql;
SOURCE Database/Database_Deployment/Sql_Files/StoredProcedures/Volvo/sp_Volvo_GeneratedLabelData_Insert.sql;
SOURCE Database/Database_Deployment/Sql_Files/StoredProcedures/Volvo/sp_Volvo_GeneratedLabelData_GetAll.sql;
SOURCE Database/Database_Deployment/Sql_Files/StoredProcedures/Volvo/sp_Volvo_GeneratedLabelData_ClearToHistory.sql;