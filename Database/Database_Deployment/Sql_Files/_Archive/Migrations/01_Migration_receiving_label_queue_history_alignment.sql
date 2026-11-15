DROP PROCEDURE IF EXISTS sp_mig38_add_column_if_missing;
DROP PROCEDURE IF EXISTS sp_mig38_add_index_if_missing;
DROP PROCEDURE IF EXISTS sp_mig38_drop_column_if_exists;
DROP PROCEDURE IF EXISTS sp_mig38_drop_index_if_exists;
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
END $$

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
END $$

CREATE PROCEDURE sp_mig38_drop_column_if_exists(
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

    IF v_exists = 1 THEN
        SET @ddl_sql = p_ddl;
        PREPARE stmt FROM @ddl_sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
END $$

CREATE PROCEDURE sp_mig38_drop_index_if_exists(
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

    IF v_exists = 1 THEN
        SET @ddl_sql = p_ddl;
        PREPARE stmt FROM @ddl_sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
END $$

CREATE PROCEDURE sp_mig38_apply()
BEGIN
    DECLARE v_po_number_exists INT DEFAULT 0;

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

    CALL sp_mig38_add_column_if_missing('receiving_history', 'load_number',
        'ALTER TABLE receiving_history ADD COLUMN load_number INT NULL COMMENT ''Sequential load number within the receiving session''');
    CALL sp_mig38_add_column_if_missing('receiving_history', 'load_id',
        'ALTER TABLE receiving_history ADD COLUMN load_id CHAR(36) NULL COMMENT ''Compatibility alias for receiving_label_data.load_id''');
    CALL sp_mig38_add_column_if_missing('receiving_history', 'po_status',
        'ALTER TABLE receiving_history ADD COLUMN po_status VARCHAR(100) NULL COMMENT ''PO status snapshot''');
    CALL sp_mig38_add_column_if_missing('receiving_history', 'po_vendor',
        'ALTER TABLE receiving_history ADD COLUMN po_vendor VARCHAR(255) NULL COMMENT ''Compatibility alias for receiving_label_data.po_vendor''');
    CALL sp_mig38_add_column_if_missing('receiving_history', 'po_due_date',
        'ALTER TABLE receiving_history ADD COLUMN po_due_date DATE NULL COMMENT ''PO due date snapshot''');
    CALL sp_mig38_add_column_if_missing('receiving_history', 'qty_ordered',
        'ALTER TABLE receiving_history ADD COLUMN qty_ordered DECIMAL(18,2) NULL COMMENT ''Ordered quantity snapshot''');
    CALL sp_mig38_add_column_if_missing('receiving_history', 'unit_of_measure',
        'ALTER TABLE receiving_history ADD COLUMN unit_of_measure VARCHAR(20) NULL COMMENT ''UOM snapshot''');
    CALL sp_mig38_add_column_if_missing('receiving_history', 'remaining_quantity',
        'ALTER TABLE receiving_history ADD COLUMN remaining_quantity INT NULL COMMENT ''Remaining PO quantity snapshot''');
    CALL sp_mig38_add_column_if_missing('receiving_history', 'user_id',
        'ALTER TABLE receiving_history ADD COLUMN user_id VARCHAR(100) NULL COMMENT ''Windows user / app user id''');
    CALL sp_mig38_add_column_if_missing('receiving_history', 'received_date',
        'ALTER TABLE receiving_history ADD COLUMN received_date DATETIME NULL COMMENT ''Compatibility alias for receiving_label_data.received_date''');
    CALL sp_mig38_add_column_if_missing('receiving_history', 'packages_per_load',
        'ALTER TABLE receiving_history ADD COLUMN packages_per_load INT NULL COMMENT ''Number of packages per load/skid''');
    CALL sp_mig38_add_column_if_missing('receiving_history', 'package_type_name',
        'ALTER TABLE receiving_history ADD COLUMN package_type_name VARCHAR(50) NULL COMMENT ''Package type description''');
    CALL sp_mig38_add_column_if_missing('receiving_history', 'weight_per_package',
        'ALTER TABLE receiving_history ADD COLUMN weight_per_package DECIMAL(18,2) NULL COMMENT ''Weight of each individual package''');
    CALL sp_mig38_add_column_if_missing('receiving_history', 'is_quality_hold_required',
        'ALTER TABLE receiving_history ADD COLUMN is_quality_hold_required TINYINT(1) NOT NULL DEFAULT 0 COMMENT ''Quality hold required''');
    CALL sp_mig38_add_column_if_missing('receiving_history', 'is_quality_hold_acknowledged',
        'ALTER TABLE receiving_history ADD COLUMN is_quality_hold_acknowledged TINYINT(1) NOT NULL DEFAULT 0 COMMENT ''Quality hold acknowledged''');
    CALL sp_mig38_add_column_if_missing('receiving_history', 'quality_hold_restriction_type',
        'ALTER TABLE receiving_history ADD COLUMN quality_hold_restriction_type VARCHAR(255) NULL COMMENT ''Quality hold restriction type''');

    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = DATABASE()
          AND table_name = 'receiving_history'
          AND column_name = 'PartDescription'
    ) THEN
        SET @ddl_sql = 'UPDATE receiving_history SET part_description = COALESCE(part_description, PartDescription) WHERE part_description IS NULL AND PartDescription IS NOT NULL';
        PREPARE stmt FROM @ddl_sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;

    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = DATABASE()
          AND table_name = 'receiving_history'
          AND column_name = 'POVendor'
    ) THEN
        SET @ddl_sql = 'UPDATE receiving_history SET vendor_name = COALESCE(vendor_name, POVendor) WHERE vendor_name IS NULL AND POVendor IS NOT NULL';
        PREPARE stmt FROM @ddl_sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;

    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = DATABASE()
          AND table_name = 'receiving_history'
          AND column_name = 'POStatus'
    ) THEN
        SET @ddl_sql = 'UPDATE receiving_history SET po_status = COALESCE(po_status, POStatus) WHERE po_status IS NULL AND POStatus IS NOT NULL';
        PREPARE stmt FROM @ddl_sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;

    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = DATABASE()
          AND table_name = 'receiving_history'
          AND column_name = 'PODueDate'
    ) THEN
        SET @ddl_sql = 'UPDATE receiving_history SET po_due_date = COALESCE(po_due_date, PODueDate) WHERE po_due_date IS NULL AND PODueDate IS NOT NULL';
        PREPARE stmt FROM @ddl_sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;

    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = DATABASE()
          AND table_name = 'receiving_history'
          AND column_name = 'QtyOrdered'
    ) THEN
        SET @ddl_sql = 'UPDATE receiving_history SET qty_ordered = COALESCE(qty_ordered, QtyOrdered) WHERE qty_ordered IS NULL AND QtyOrdered IS NOT NULL';
        PREPARE stmt FROM @ddl_sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;

    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = DATABASE()
          AND table_name = 'receiving_history'
          AND column_name = 'UnitOfMeasure'
    ) THEN
        SET @ddl_sql = 'UPDATE receiving_history SET unit_of_measure = COALESCE(unit_of_measure, UnitOfMeasure) WHERE unit_of_measure IS NULL AND UnitOfMeasure IS NOT NULL';
        PREPARE stmt FROM @ddl_sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;

    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = DATABASE()
          AND table_name = 'receiving_history'
          AND column_name = 'RemainingQuantity'
    ) THEN
        SET @ddl_sql = 'UPDATE receiving_history SET remaining_quantity = COALESCE(remaining_quantity, RemainingQuantity) WHERE remaining_quantity IS NULL AND RemainingQuantity IS NOT NULL';
        PREPARE stmt FROM @ddl_sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;

    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = DATABASE()
          AND table_name = 'receiving_history'
          AND column_name = 'UserID'
    ) THEN
        SET @ddl_sql = 'UPDATE receiving_history SET user_id = COALESCE(user_id, UserID) WHERE user_id IS NULL AND UserID IS NOT NULL';
        PREPARE stmt FROM @ddl_sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;

    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = DATABASE()
          AND table_name = 'receiving_history'
          AND column_name = 'EmployeeNumber'
    ) THEN
        SET @ddl_sql = 'UPDATE receiving_history SET employee_number = CASE WHEN (employee_number IS NULL OR employee_number = 0) AND EmployeeNumber IS NOT NULL THEN EmployeeNumber ELSE employee_number END';
        PREPARE stmt FROM @ddl_sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;

    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = DATABASE()
          AND table_name = 'receiving_history'
          AND column_name = 'IsQualityHoldRequired'
    ) THEN
        SET @ddl_sql = 'UPDATE receiving_history SET is_quality_hold_required = CASE WHEN is_quality_hold_required = 0 AND IsQualityHoldRequired IS NOT NULL THEN IsQualityHoldRequired ELSE is_quality_hold_required END';
        PREPARE stmt FROM @ddl_sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;

    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = DATABASE()
          AND table_name = 'receiving_history'
          AND column_name = 'IsQualityHoldAcknowledged'
    ) THEN
        SET @ddl_sql = 'UPDATE receiving_history SET is_quality_hold_acknowledged = CASE WHEN is_quality_hold_acknowledged = 0 AND IsQualityHoldAcknowledged IS NOT NULL THEN IsQualityHoldAcknowledged ELSE is_quality_hold_acknowledged END';
        PREPARE stmt FROM @ddl_sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;

    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = DATABASE()
          AND table_name = 'receiving_history'
          AND column_name = 'QualityHoldRestrictionType'
    ) THEN
        SET @ddl_sql = 'UPDATE receiving_history SET quality_hold_restriction_type = COALESCE(quality_hold_restriction_type, QualityHoldRestrictionType) WHERE quality_hold_restriction_type IS NULL AND QualityHoldRestrictionType IS NOT NULL';
        PREPARE stmt FROM @ddl_sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;

    CALL sp_mig38_drop_index_if_exists('receiving_history', 'idx_po_due_date',
        'ALTER TABLE receiving_history DROP INDEX idx_po_due_date');
    CALL sp_mig38_drop_index_if_exists('receiving_history', 'idx_archived_at',
        'ALTER TABLE receiving_history DROP INDEX idx_archived_at');
    CALL sp_mig38_drop_index_if_exists('receiving_history', 'idx_archive_batch',
        'ALTER TABLE receiving_history DROP INDEX idx_archive_batch');

    CALL sp_mig38_drop_column_if_exists('receiving_history', 'PartDescription',
        'ALTER TABLE receiving_history DROP COLUMN PartDescription');
    CALL sp_mig38_drop_column_if_exists('receiving_history', 'POVendor',
        'ALTER TABLE receiving_history DROP COLUMN POVendor');
    CALL sp_mig38_drop_column_if_exists('receiving_history', 'POStatus',
        'ALTER TABLE receiving_history DROP COLUMN POStatus');
    CALL sp_mig38_drop_column_if_exists('receiving_history', 'PODueDate',
        'ALTER TABLE receiving_history DROP COLUMN PODueDate');
    CALL sp_mig38_drop_column_if_exists('receiving_history', 'QtyOrdered',
        'ALTER TABLE receiving_history DROP COLUMN QtyOrdered');
    CALL sp_mig38_drop_column_if_exists('receiving_history', 'UnitOfMeasure',
        'ALTER TABLE receiving_history DROP COLUMN UnitOfMeasure');
    CALL sp_mig38_drop_column_if_exists('receiving_history', 'RemainingQuantity',
        'ALTER TABLE receiving_history DROP COLUMN RemainingQuantity');
    CALL sp_mig38_drop_column_if_exists('receiving_history', 'UserID',
        'ALTER TABLE receiving_history DROP COLUMN UserID');
    CALL sp_mig38_drop_column_if_exists('receiving_history', 'EmployeeNumber',
        'ALTER TABLE receiving_history DROP COLUMN EmployeeNumber');
    CALL sp_mig38_drop_column_if_exists('receiving_history', 'IsQualityHoldRequired',
        'ALTER TABLE receiving_history DROP COLUMN IsQualityHoldRequired');
    CALL sp_mig38_drop_column_if_exists('receiving_history', 'IsQualityHoldAcknowledged',
        'ALTER TABLE receiving_history DROP COLUMN IsQualityHoldAcknowledged');
    CALL sp_mig38_drop_column_if_exists('receiving_history', 'QualityHoldRestrictionType',
        'ALTER TABLE receiving_history DROP COLUMN QualityHoldRestrictionType');
    CALL sp_mig38_drop_column_if_exists('receiving_history', 'ArchivedAt',
        'ALTER TABLE receiving_history DROP COLUMN ArchivedAt');
    CALL sp_mig38_drop_column_if_exists('receiving_history', 'ArchivedBy',
        'ALTER TABLE receiving_history DROP COLUMN ArchivedBy');
    CALL sp_mig38_drop_column_if_exists('receiving_history', 'ArchiveBatchID',
        'ALTER TABLE receiving_history DROP COLUMN ArchiveBatchID');

    CALL sp_mig38_add_index_if_missing('receiving_history', 'idx_po_due_date',
        'CREATE INDEX idx_po_due_date ON receiving_history (po_due_date)');
    CALL sp_mig38_add_index_if_missing('receiving_history', 'idx_user_id',
        'CREATE INDEX idx_user_id ON receiving_history (user_id)');
END $$

DELIMITER ;

CALL sp_mig38_apply();

DROP PROCEDURE IF EXISTS sp_mig38_apply;
DROP PROCEDURE IF EXISTS sp_mig38_drop_index_if_exists;
DROP PROCEDURE IF EXISTS sp_mig38_drop_column_if_exists;
DROP PROCEDURE IF EXISTS sp_mig38_add_index_if_missing;
DROP PROCEDURE IF EXISTS sp_mig38_add_column_if_missing;