-- ============================================================================
-- Migration: 42_Migration_dunnage_part_skid_counters
-- Module: Dunnage
-- Purpose:
--   Add per-part skid counter columns to the active dunnage queue and archive,
--   mirroring the receiving_label_data "N of M" skid logic so Dunnage labels can
--   preserve each row's position within the saved batch for the same part.
-- ============================================================================

DROP PROCEDURE IF EXISTS sp_mig42_add_column_if_missing;
DROP PROCEDURE IF EXISTS sp_mig42_apply;

DELIMITER $$

CREATE PROCEDURE sp_mig42_add_column_if_missing(
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
      AND table_name    = p_table_name
      AND column_name   = p_column_name;

    IF v_exists = 0 THEN
        SET @ddl_sql = p_ddl;
        PREPARE stmt FROM @ddl_sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
END $$

CREATE PROCEDURE sp_mig42_apply()
BEGIN
    CALL sp_mig42_add_column_if_missing('dunnage_label_data', 'part_skid_sequence',
        'ALTER TABLE dunnage_label_data ADD COLUMN part_skid_sequence INT NULL COMMENT ''Position of this skid among all skids for the same part in the saved batch'' AFTER label_number');

    CALL sp_mig42_add_column_if_missing('dunnage_label_data', 'part_skid_total',
        'ALTER TABLE dunnage_label_data ADD COLUMN part_skid_total INT NULL COMMENT ''Total skids for the same part in the saved batch'' AFTER part_skid_sequence');

    CALL sp_mig42_add_column_if_missing('dunnage_history', 'part_skid_sequence',
        'ALTER TABLE dunnage_history ADD COLUMN part_skid_sequence INT NULL COMMENT ''Position of this skid among all skids for the same part in the saved batch'' AFTER label_number');

    CALL sp_mig42_add_column_if_missing('dunnage_history', 'part_skid_total',
        'ALTER TABLE dunnage_history ADD COLUMN part_skid_total INT NULL COMMENT ''Total skids for the same part in the saved batch'' AFTER part_skid_sequence');
END $$

DELIMITER ;

CALL sp_mig42_apply();

DROP PROCEDURE IF EXISTS sp_mig42_apply;
DROP PROCEDURE IF EXISTS sp_mig42_add_column_if_missing;