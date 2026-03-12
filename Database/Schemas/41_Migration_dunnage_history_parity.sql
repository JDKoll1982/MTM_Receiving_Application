-- ============================================================================
-- Migration: 41_Migration_dunnage_history_parity
-- Module: Dunnage
-- Purpose:
--   Align dunnage_history (archive table) with the full label-context payload
--   held by dunnage_label_data (active queue), so no information is lost when
--   sp_Dunnage_LabelData_ClearToHistory moves rows from queue to archive.
-- Lifecycle:
--   Workflow Complete -> write to dunnage_label_data
--   Clear Label Data -> move rows to dunnage_history, delete queue rows
-- Prerequisite: 40_Table_dunnage_label_data.sql must be applied first.
-- ============================================================================

DROP PROCEDURE IF EXISTS sp_mig41_add_column_if_missing;
DROP PROCEDURE IF EXISTS sp_mig41_add_index_if_missing;
DROP PROCEDURE IF EXISTS sp_mig41_apply;

DELIMITER $$

CREATE PROCEDURE sp_mig41_add_column_if_missing(
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
END$$

CREATE PROCEDURE sp_mig41_add_index_if_missing(
    IN p_table_name  VARCHAR(64),
    IN p_index_name  VARCHAR(64),
    IN p_ddl         TEXT
)
BEGIN
    DECLARE v_exists INT DEFAULT 0;

    SELECT COUNT(*)
    INTO v_exists
    FROM information_schema.statistics
    WHERE table_schema = DATABASE()
      AND table_name   = p_table_name
      AND index_name   = p_index_name;

    IF v_exists = 0 THEN
        SET @ddl_sql = p_ddl;
        PREPARE stmt FROM @ddl_sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
END$$

CREATE PROCEDURE sp_mig41_apply()
BEGIN
    -- ==============================
    -- dunnage_history parity columns
    -- ==============================

    -- PO number (NULL for non-PO items)
    CALL sp_mig41_add_column_if_missing('dunnage_history', 'po_number',
        'ALTER TABLE dunnage_history ADD COLUMN po_number VARCHAR(50) NULL COMMENT ''PO number snapshot; NULL for non-PO items''');

    -- Dunnage type snapshot
    CALL sp_mig41_add_column_if_missing('dunnage_history', 'type_id',
        'ALTER TABLE dunnage_history ADD COLUMN type_id INT NULL COMMENT ''FK to dunnage_types.id; snapshot at time of archive''');
    CALL sp_mig41_add_column_if_missing('dunnage_history', 'type_name',
        'ALTER TABLE dunnage_history ADD COLUMN type_name VARCHAR(100) NULL COMMENT ''Type name snapshot (e.g. Corrugated Cardboard)''');
    CALL sp_mig41_add_column_if_missing('dunnage_history', 'type_icon',
        'ALTER TABLE dunnage_history ADD COLUMN type_icon VARCHAR(100) NULL COMMENT ''MaterialIconKind string snapshot (e.g. PackageVariantClosed)''');

    -- Location and label tracking
    CALL sp_mig41_add_column_if_missing('dunnage_history', 'location',
        'ALTER TABLE dunnage_history ADD COLUMN location VARCHAR(100) NULL COMMENT ''Warehouse location for received dunnage''');
    CALL sp_mig41_add_column_if_missing('dunnage_history', 'label_number',
        'ALTER TABLE dunnage_history ADD COLUMN label_number VARCHAR(50) NULL COMMENT ''Label number for this row (supports multi-label splits)''');

    -- Dynamic spec snapshot
    CALL sp_mig41_add_column_if_missing('dunnage_history', 'specs_json',
        'ALTER TABLE dunnage_history ADD COLUMN specs_json JSON NULL COMMENT ''Snapshot of all per-line dynamic spec key/value pairs as JSON object''');

    -- Archive metadata (stamped by sp_Dunnage_LabelData_ClearToHistory)
    CALL sp_mig41_add_column_if_missing('dunnage_history', 'archived_at',
        'ALTER TABLE dunnage_history ADD COLUMN archived_at DATETIME NULL COMMENT ''Timestamp when the row was moved from the active queue to history''');
    CALL sp_mig41_add_column_if_missing('dunnage_history', 'archived_by',
        'ALTER TABLE dunnage_history ADD COLUMN archived_by VARCHAR(100) NULL COMMENT ''Username of the user who triggered Clear Label Data''');
    CALL sp_mig41_add_column_if_missing('dunnage_history', 'archive_batch_id',
        'ALTER TABLE dunnage_history ADD COLUMN archive_batch_id CHAR(36) NULL COMMENT ''UUID shared by all rows archived in the same Clear Label Data operation''');

    -- Indexes
    CALL sp_mig41_add_index_if_missing('dunnage_history', 'IDX_HISTORY_ARCHIVE_BATCH',
        'CREATE INDEX IDX_HISTORY_ARCHIVE_BATCH ON dunnage_history (archive_batch_id)');
    CALL sp_mig41_add_index_if_missing('dunnage_history', 'IDX_HISTORY_PO_NUMBER',
        'CREATE INDEX IDX_HISTORY_PO_NUMBER ON dunnage_history (po_number)');
END$$

DELIMITER ;

CALL sp_mig41_apply();

DROP PROCEDURE IF EXISTS sp_mig41_apply;
DROP PROCEDURE IF EXISTS sp_mig41_add_index_if_missing;
DROP PROCEDURE IF EXISTS sp_mig41_add_column_if_missing;

-- ============================================================================
