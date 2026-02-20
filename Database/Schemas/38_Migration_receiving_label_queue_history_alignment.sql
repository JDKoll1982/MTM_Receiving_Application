-- ============================================================================
-- Migration: 38_Migration_receiving_label_queue_history_alignment
-- Module: Receiving
-- Purpose:
--   1) Align receiving_label_data (active label queue) with full workflow payload
--   2) Align receiving_history (archive) with full queue payload + archive metadata
-- Lifecycle:
--   Workflow Complete -> write to receiving_label_data
--   Clear Label Data -> move rows to receiving_history, delete queue rows
-- ============================================================================

DROP PROCEDURE IF EXISTS sp_mig38_add_column_if_missing;
DROP PROCEDURE IF EXISTS sp_mig38_add_index_if_missing;
DROP PROCEDURE IF EXISTS sp_mig38_apply;

DELIMITER $$

CREATE PROCEDURE sp_mig38_add_column_if_missing(
    IN p_table_name VARCHAR(64),
    IN p_column_name VARCHAR(64),
    IN p_ddl TEXT
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
END$$

CREATE PROCEDURE sp_mig38_add_index_if_missing(
    IN p_table_name VARCHAR(64),
    IN p_index_name VARCHAR(64),
    IN p_ddl TEXT
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
END$$

CREATE PROCEDURE sp_mig38_apply()
BEGIN
    DECLARE v_po_number_exists INT DEFAULT 0;

    -- ==============================
    -- Queue table alignment
    -- ==============================
    CALL sp_mig38_add_column_if_missing('receiving_label_data', 'load_id',
        'ALTER TABLE receiving_label_data ADD COLUMN load_id CHAR(36) NULL COMMENT ''Workflow load UUID''');
    CALL sp_mig38_add_column_if_missing('receiving_label_data', 'load_number',
        'ALTER TABLE receiving_label_data ADD COLUMN load_number INT NULL COMMENT ''Sequential workflow load number''');
    CALL sp_mig38_add_column_if_missing('receiving_label_data', 'part_type',
        'ALTER TABLE receiving_label_data ADD COLUMN part_type VARCHAR(50) NULL COMMENT ''Part classification/category''');
    CALL sp_mig38_add_column_if_missing('receiving_label_data', 'po_line_number',
        'ALTER TABLE receiving_label_data ADD COLUMN po_line_number VARCHAR(10) NULL COMMENT ''PO line number''');
    CALL sp_mig38_add_column_if_missing('receiving_label_data', 'po_vendor',
        'ALTER TABLE receiving_label_data ADD COLUMN po_vendor VARCHAR(255) NULL COMMENT ''PO vendor name snapshot''');
    CALL sp_mig38_add_column_if_missing('receiving_label_data', 'po_status',
        'ALTER TABLE receiving_label_data ADD COLUMN po_status VARCHAR(100) NULL COMMENT ''PO status snapshot''');
    CALL sp_mig38_add_column_if_missing('receiving_label_data', 'po_due_date',
        'ALTER TABLE receiving_label_data ADD COLUMN po_due_date DATE NULL COMMENT ''PO due date snapshot''');
    CALL sp_mig38_add_column_if_missing('receiving_label_data', 'qty_ordered',
        'ALTER TABLE receiving_label_data ADD COLUMN qty_ordered DECIMAL(18,2) NULL COMMENT ''Ordered quantity snapshot''');
    CALL sp_mig38_add_column_if_missing('receiving_label_data', 'unit_of_measure',
        'ALTER TABLE receiving_label_data ADD COLUMN unit_of_measure VARCHAR(20) NULL COMMENT ''UOM snapshot''');
    CALL sp_mig38_add_column_if_missing('receiving_label_data', 'remaining_quantity',
        'ALTER TABLE receiving_label_data ADD COLUMN remaining_quantity INT NULL COMMENT ''Remaining PO quantity snapshot''');
    CALL sp_mig38_add_column_if_missing('receiving_label_data', 'packages_per_load',
        'ALTER TABLE receiving_label_data ADD COLUMN packages_per_load INT NULL COMMENT ''Packages per load''');
    CALL sp_mig38_add_column_if_missing('receiving_label_data', 'package_type_name',
        'ALTER TABLE receiving_label_data ADD COLUMN package_type_name VARCHAR(50) NULL COMMENT ''Package type name''');
    CALL sp_mig38_add_column_if_missing('receiving_label_data', 'weight_per_package',
        'ALTER TABLE receiving_label_data ADD COLUMN weight_per_package DECIMAL(18,2) NULL COMMENT ''Calculated weight per package''');
    CALL sp_mig38_add_column_if_missing('receiving_label_data', 'is_non_po_item',
        'ALTER TABLE receiving_label_data ADD COLUMN is_non_po_item TINYINT(1) NOT NULL DEFAULT 0 COMMENT ''Non-PO item flag''');
    CALL sp_mig38_add_column_if_missing('receiving_label_data', 'received_date',
        'ALTER TABLE receiving_label_data ADD COLUMN received_date DATETIME NULL COMMENT ''Workflow received datetime''');
    CALL sp_mig38_add_column_if_missing('receiving_label_data', 'user_id',
        'ALTER TABLE receiving_label_data ADD COLUMN user_id VARCHAR(100) NULL COMMENT ''Windows user / app user id''');
    CALL sp_mig38_add_column_if_missing('receiving_label_data', 'is_quality_hold_required',
        'ALTER TABLE receiving_label_data ADD COLUMN is_quality_hold_required TINYINT(1) NOT NULL DEFAULT 0 COMMENT ''Quality hold required''');
    CALL sp_mig38_add_column_if_missing('receiving_label_data', 'is_quality_hold_acknowledged',
        'ALTER TABLE receiving_label_data ADD COLUMN is_quality_hold_acknowledged TINYINT(1) NOT NULL DEFAULT 0 COMMENT ''Quality hold acknowledged''');
    CALL sp_mig38_add_column_if_missing('receiving_label_data', 'quality_hold_restriction_type',
        'ALTER TABLE receiving_label_data ADD COLUMN quality_hold_restriction_type VARCHAR(255) NULL COMMENT ''Quality hold restriction type''');
    CALL sp_mig38_add_column_if_missing('receiving_label_data', 'weight_quantity',
        'ALTER TABLE receiving_label_data ADD COLUMN weight_quantity DECIMAL(18,2) NULL COMMENT ''Workflow weight/quantity value''');

    SELECT COUNT(*)
    INTO v_po_number_exists
    FROM information_schema.columns
    WHERE table_schema = DATABASE()
      AND table_name = 'receiving_label_data'
      AND column_name = 'po_number';

    IF v_po_number_exists = 1 THEN
        ALTER TABLE receiving_label_data
            MODIFY COLUMN po_number VARCHAR(20) NULL COMMENT 'Purchase order number (string-safe for all PO formats)';
    END IF;

    CALL sp_mig38_add_index_if_missing('receiving_label_data', 'idx_load_id',
        'CREATE INDEX idx_load_id ON receiving_label_data (load_id)');
    CALL sp_mig38_add_index_if_missing('receiving_label_data', 'idx_received_date',
        'CREATE INDEX idx_received_date ON receiving_label_data (received_date)');
    CALL sp_mig38_add_index_if_missing('receiving_label_data', 'idx_user_id',
        'CREATE INDEX idx_user_id ON receiving_label_data (user_id)');

    -- ==============================
    -- History table alignment
    -- ==============================
    CALL sp_mig38_add_column_if_missing('receiving_history', 'PartDescription',
        'ALTER TABLE receiving_history ADD COLUMN PartDescription VARCHAR(500) NULL COMMENT ''Part description snapshot''');
    CALL sp_mig38_add_column_if_missing('receiving_history', 'POVendor',
        'ALTER TABLE receiving_history ADD COLUMN POVendor VARCHAR(255) NULL COMMENT ''PO vendor snapshot''');
    CALL sp_mig38_add_column_if_missing('receiving_history', 'POStatus',
        'ALTER TABLE receiving_history ADD COLUMN POStatus VARCHAR(100) NULL COMMENT ''PO status snapshot''');
    CALL sp_mig38_add_column_if_missing('receiving_history', 'PODueDate',
        'ALTER TABLE receiving_history ADD COLUMN PODueDate DATE NULL COMMENT ''PO due date snapshot''');
    CALL sp_mig38_add_column_if_missing('receiving_history', 'QtyOrdered',
        'ALTER TABLE receiving_history ADD COLUMN QtyOrdered DECIMAL(18,2) NULL COMMENT ''Ordered quantity snapshot''');
    CALL sp_mig38_add_column_if_missing('receiving_history', 'UnitOfMeasure',
        'ALTER TABLE receiving_history ADD COLUMN UnitOfMeasure VARCHAR(20) NULL COMMENT ''UOM snapshot''');
    CALL sp_mig38_add_column_if_missing('receiving_history', 'RemainingQuantity',
        'ALTER TABLE receiving_history ADD COLUMN RemainingQuantity INT NULL COMMENT ''Remaining PO quantity snapshot''');
    CALL sp_mig38_add_column_if_missing('receiving_history', 'UserID',
        'ALTER TABLE receiving_history ADD COLUMN UserID VARCHAR(100) NULL COMMENT ''Windows user / app user id''');
    CALL sp_mig38_add_column_if_missing('receiving_history', 'EmployeeNumber',
        'ALTER TABLE receiving_history ADD COLUMN EmployeeNumber INT NULL COMMENT ''Employee number''');
    CALL sp_mig38_add_column_if_missing('receiving_history', 'IsQualityHoldRequired',
        'ALTER TABLE receiving_history ADD COLUMN IsQualityHoldRequired TINYINT(1) NOT NULL DEFAULT 0 COMMENT ''Quality hold required''');
    CALL sp_mig38_add_column_if_missing('receiving_history', 'IsQualityHoldAcknowledged',
        'ALTER TABLE receiving_history ADD COLUMN IsQualityHoldAcknowledged TINYINT(1) NOT NULL DEFAULT 0 COMMENT ''Quality hold acknowledged''');
    CALL sp_mig38_add_column_if_missing('receiving_history', 'QualityHoldRestrictionType',
        'ALTER TABLE receiving_history ADD COLUMN QualityHoldRestrictionType VARCHAR(255) NULL COMMENT ''Quality hold restriction type''');
    CALL sp_mig38_add_column_if_missing('receiving_history', 'ArchivedAt',
        'ALTER TABLE receiving_history ADD COLUMN ArchivedAt DATETIME NULL COMMENT ''Archive transfer timestamp''');
    CALL sp_mig38_add_column_if_missing('receiving_history', 'ArchivedBy',
        'ALTER TABLE receiving_history ADD COLUMN ArchivedBy VARCHAR(100) NULL COMMENT ''Archive action user''');
    CALL sp_mig38_add_column_if_missing('receiving_history', 'ArchiveBatchID',
        'ALTER TABLE receiving_history ADD COLUMN ArchiveBatchID CHAR(36) NULL COMMENT ''Archive operation batch id''');

    CALL sp_mig38_add_index_if_missing('receiving_history', 'idx_employee_number',
        'CREATE INDEX idx_employee_number ON receiving_history (EmployeeNumber)');
    CALL sp_mig38_add_index_if_missing('receiving_history', 'idx_po_due_date',
        'CREATE INDEX idx_po_due_date ON receiving_history (PODueDate)');
    CALL sp_mig38_add_index_if_missing('receiving_history', 'idx_archived_at',
        'CREATE INDEX idx_archived_at ON receiving_history (ArchivedAt)');
    CALL sp_mig38_add_index_if_missing('receiving_history', 'idx_archive_batch',
        'CREATE INDEX idx_archive_batch ON receiving_history (ArchiveBatchID)');
END$$

DELIMITER ;

CALL sp_mig38_apply();

DROP PROCEDURE IF EXISTS sp_mig38_apply;
DROP PROCEDURE IF EXISTS sp_mig38_add_index_if_missing;
DROP PROCEDURE IF EXISTS sp_mig38_add_column_if_missing;

-- ============================================================================
