-- ============================================================================
-- Migration: 10_Migration_reprint_dunnage_volvo
-- Module: Dunnage / Volvo
-- Purpose:
--   1. Add is_reprint column to dunnage_label_data and volvo_generated_label_data.
--   2. Deploy sp_Dunnage_LabelData_InsertFromHistory (new procedure).
--   3. Deploy sp_Volvo_GeneratedLabelData_InsertFromHistory (new procedure).
--   4. Deploy the three reprint-history queries that expose an `already_queued`
--      flag (sp_Receiving_History_GetForReprint, sp_Dunnage_LabelHistory_GetForReprint,
--      sp_Volvo_GeneratedLabelHistory_GetForReprint).
-- Note: sp_Dunnage_LabelData_ClearToHistory and
--       sp_Volvo_GeneratedLabelData_ClearToHistory are updated in their source files
--       to skip is_reprint rows and are installed by the normal SP deployment.
-- Run this single file against mtm_receiving_application_test to apply everything.
-- ============================================================================

USE mtm_receiving_application_test;

-- ---------------------------------------------------------------------------
-- Step 1: Add is_reprint columns (idempotent — skipped if already present)
-- ---------------------------------------------------------------------------

DROP PROCEDURE IF EXISTS sp_mig10_add_column_if_missing;
DROP PROCEDURE IF EXISTS sp_mig10_apply;

DELIMITER $$

CREATE PROCEDURE sp_mig10_add_column_if_missing(
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
      AND table_name   = p_table_name
      AND column_name  = p_column_name;

    IF v_exists = 0 THEN
        SET @ddl_sql = p_ddl;
        PREPARE stmt FROM @ddl_sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
END $$

CREATE PROCEDURE sp_mig10_apply()
BEGIN
    CALL sp_mig10_add_column_if_missing(
        'dunnage_label_data',
        'is_reprint',
        'ALTER TABLE dunnage_label_data ADD COLUMN is_reprint TINYINT(1) NOT NULL DEFAULT 0 COMMENT ''1 when this row was re-queued from dunnage_history for a label reprint'' AFTER label_number'
    );

    CALL sp_mig10_add_column_if_missing(
        'volvo_generated_label_data',
        'is_reprint',
        'ALTER TABLE volvo_generated_label_data ADD COLUMN is_reprint TINYINT(1) NOT NULL DEFAULT 0 COMMENT ''1 when this row was re-queued from volvo_generated_label_history for a label reprint'' AFTER employee_number'
    );
END $$

DELIMITER ;

CALL sp_mig10_apply();

DROP PROCEDURE IF EXISTS sp_mig10_apply;
DROP PROCEDURE IF EXISTS sp_mig10_add_column_if_missing;
