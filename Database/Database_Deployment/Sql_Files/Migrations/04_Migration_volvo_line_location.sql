-- ============================================================================
-- Migration: Volvo line location support
-- Module: Volvo / Reporting
-- Purpose: Add persisted location to active/history Volvo line tables.
-- ============================================================================

DROP PROCEDURE IF EXISTS sp_mig42_add_column_if_missing;

DELIMITER $$

CREATE PROCEDURE sp_mig42_add_column_if_missing(
    IN p_table_name VARCHAR(64),
    IN p_column_name VARCHAR(64),
    IN p_alter_sql TEXT
)
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = DATABASE()
          AND table_name = p_table_name
          AND column_name = p_column_name
    ) THEN
        SET @sql = p_alter_sql;
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
END $$

DELIMITER ;

CALL sp_mig42_add_column_if_missing(
    'volvo_line_data',
    'location',
    'ALTER TABLE volvo_line_data ADD COLUMN location VARCHAR(50) NULL COMMENT ''Warehouse location for this shipment line'' AFTER part_number');

CALL sp_mig42_add_column_if_missing(
    'volvo_line_history',
    'location',
    'ALTER TABLE volvo_line_history ADD COLUMN location VARCHAR(50) NULL COMMENT ''Warehouse location for this shipment line'' AFTER part_number');

DROP PROCEDURE IF EXISTS sp_mig42_add_column_if_missing;