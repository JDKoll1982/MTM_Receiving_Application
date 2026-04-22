-- ============================================================================
-- Migration: 43_Migration_dunnage_quantity_types
-- Module: Dunnage
-- Purpose:
--   Add quantity_type to the Dunnage part master and to the active/history label
--   snapshots, and create the reusable dunnage_quantity_types lookup table.
-- ============================================================================

DROP PROCEDURE IF EXISTS sp_mig43_add_column_if_missing;
DROP PROCEDURE IF EXISTS sp_mig43_add_table_if_missing;
DROP PROCEDURE IF EXISTS sp_mig43_apply;

DELIMITER $$

CREATE PROCEDURE sp_mig43_add_column_if_missing(
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

CREATE PROCEDURE sp_mig43_add_table_if_missing(
    IN p_table_name VARCHAR(64),
    IN p_ddl        TEXT
)
BEGIN
    DECLARE v_exists INT DEFAULT 0;

    SELECT COUNT(*)
    INTO v_exists
    FROM information_schema.tables
    WHERE table_schema = DATABASE()
      AND table_name = p_table_name;

    IF v_exists = 0 THEN
        SET @ddl_sql = p_ddl;
        PREPARE stmt FROM @ddl_sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
END $$

CREATE PROCEDURE sp_mig43_apply()
BEGIN
    CALL sp_mig43_add_column_if_missing('dunnage_parts', 'quantity_type',
        'ALTER TABLE dunnage_parts ADD COLUMN quantity_type VARCHAR(100) NOT NULL DEFAULT ''Quantity'' COMMENT ''Label header used for quantity on printed dunnage labels'' AFTER image_path');

    CALL sp_mig43_add_column_if_missing('dunnage_label_data', 'quantity_type',
        'ALTER TABLE dunnage_label_data ADD COLUMN quantity_type VARCHAR(100) NOT NULL DEFAULT ''Quantity'' COMMENT ''Quantity label header snapshot copied from dunnage_parts at save time'' AFTER quantity');

    CALL sp_mig43_add_column_if_missing('dunnage_history', 'quantity_type',
        'ALTER TABLE dunnage_history ADD COLUMN quantity_type VARCHAR(100) NOT NULL DEFAULT ''Quantity'' COMMENT ''Quantity label header snapshot preserved when queue rows move to history'' AFTER quantity');

    CALL sp_mig43_add_table_if_missing('dunnage_quantity_types',
        'CREATE TABLE dunnage_quantity_types (id INT AUTO_INCREMENT PRIMARY KEY COMMENT ''Unique identifier for a reusable dunnage quantity label header'', quantity_type VARCHAR(100) NOT NULL UNIQUE COMMENT ''Display label used for quantity on dunnage labels, for example Weight or Pieces'', created_by VARCHAR(50) NOT NULL DEFAULT ''SYSTEM'' COMMENT ''User who first saved this reusable quantity type'', created_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT ''Timestamp when the quantity type was created'') ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_unicode_ci COMMENT = ''Reusable quantity label headers for Dunnage part setup''');

    INSERT IGNORE INTO dunnage_quantity_types (quantity_type, created_by)
    VALUES
        ('Weight', 'SYSTEM'),
        ('Gallons', 'SYSTEM'),
        ('Boxes', 'SYSTEM'),
        ('Sheets', 'SYSTEM'),
        ('Bags', 'SYSTEM'),
        ('Pieces', 'SYSTEM');
END $$

DELIMITER ;

CALL sp_mig43_apply();

DROP PROCEDURE IF EXISTS sp_mig43_apply;
DROP PROCEDURE IF EXISTS sp_mig43_add_table_if_missing;
DROP PROCEDURE IF EXISTS sp_mig43_add_column_if_missing;