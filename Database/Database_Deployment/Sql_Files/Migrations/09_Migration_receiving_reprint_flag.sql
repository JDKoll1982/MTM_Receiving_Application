-- ============================================================================
-- Migration: 09_Migration_receiving_reprint_flag
-- Module: Receiving
-- Purpose:
--   1. Add is_reprint column to receiving_label_data.
--   2. Re-deploy sp_Receiving_LabelData_GetAll        (SELECTs is_reprint).
--   3. Re-deploy sp_Receiving_LabelData_ClearToHistory (skips is_reprint rows).
--   4. Deploy    sp_Receiving_LabelData_InsertFromHistory (new procedure).
-- Run this single file against mtm_receiving_application to apply everything.
-- ============================================================================

USE mtm_receiving_application;

-- ---------------------------------------------------------------------------
-- Step 1: Add is_reprint column (idempotent — skipped if already present)
-- ---------------------------------------------------------------------------

DROP PROCEDURE IF EXISTS sp_mig09_add_column_if_missing;
DROP PROCEDURE IF EXISTS sp_mig09_apply;

DELIMITER $$

CREATE PROCEDURE sp_mig09_add_column_if_missing(
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

CREATE PROCEDURE sp_mig09_apply()
BEGIN
    CALL sp_mig09_add_column_if_missing(
        'receiving_label_data',
        'is_reprint',
        'ALTER TABLE receiving_label_data ADD COLUMN is_reprint TINYINT(1) NOT NULL DEFAULT 0 COMMENT ''1 when this row was re-queued from receiving_history for a label reprint'' AFTER is_quality_hold_acknowledged'
    );
END $$

DELIMITER ;

CALL sp_mig09_apply();

DROP PROCEDURE IF EXISTS sp_mig09_apply;
DROP PROCEDURE IF EXISTS sp_mig09_add_column_if_missing;

-- ---------------------------------------------------------------------------
-- Step 2: sp_Receiving_LabelData_GetAll  (includes is_reprint in SELECT)
-- ---------------------------------------------------------------------------

DROP PROCEDURE IF EXISTS `sp_Receiving_LabelData_GetAll`;

DELIMITER $$

CREATE PROCEDURE `sp_Receiving_LabelData_GetAll`()
BEGIN
    SELECT
        id,
        load_id,
        part_id,
        part_description,
        part_type,
        po_number,
        po_line_number,
        po_vendor,
        po_status,
        po_due_date,
        qty_ordered,
        unit_of_measure,
        remaining_quantity,
        load_number,
        weight_quantity,
        heat,
        initial_location,
        packages_per_load,
        package_type_name,
        weight_per_package,
        is_non_po_item,
        received_date,
        created_at,
        user_id,
        employee_number,
        is_quality_hold_required,
        is_quality_hold_acknowledged,
        is_reprint,
        quality_hold_restriction_type
    FROM receiving_label_data
    ORDER BY load_number ASC;
END $$

DELIMITER ;

-- ---------------------------------------------------------------------------
-- Step 3: sp_Receiving_LabelData_ClearToHistory  (skips is_reprint = 1 rows)
-- ---------------------------------------------------------------------------

DROP PROCEDURE IF EXISTS `sp_Receiving_LabelData_ClearToHistory`;

DELIMITER $$

CREATE PROCEDURE `sp_Receiving_LabelData_ClearToHistory`(
    IN  p_archived_by      VARCHAR(100),
    IN  p_employee_number  INT,
    IN  p_clear_all        TINYINT(1),
    OUT p_rows_moved       INT,
    OUT p_archive_batch_id CHAR(36),
    OUT p_status           INT,
    OUT p_error_message    VARCHAR(1000)
)
BEGIN
    DECLARE v_rows_to_move INT DEFAULT 0;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        SET p_rows_moved      = 0;
        SET p_status          = 1;
        SET p_error_message   = 'Clear Label Data failed. Transaction rolled back.';
    END;

    SET p_rows_moved      = 0;
    SET p_status          = 0;
    SET p_error_message   = NULL;
    SET p_archive_batch_id = UUID();

    IF COALESCE(p_clear_all, 0) = 0 AND COALESCE(p_employee_number, 0) <= 0 THEN
        SET p_status        = 1;
        SET p_error_message = 'A valid employee number is required to clear your own receiving label rows.';
    ELSE
        START TRANSACTION;

        SELECT COUNT(*) INTO v_rows_to_move
        FROM receiving_label_data
        WHERE COALESCE(p_clear_all, 0) = 1
           OR employee_number = p_employee_number;

        IF v_rows_to_move = 0 THEN
            COMMIT;
            SET p_rows_moved    = 0;
            SET p_status        = 0;
            SET p_error_message = NULL;
        ELSE
            -- Rows with is_reprint = 1 are already in receiving_history; skip them.
            INSERT INTO receiving_history
            (
                load_guid,
                quantity,
                part_id,
                part_description,
                po_number,
                po_line_number,
                employee_number,
                heat,
                transaction_date,
                created_at,
                initial_location,
                load_number,
                packages_per_load,
                package_type_name,
                weight_per_package,
                coils_on_skid,
                label_number,
                vendor_name,
                po_status,
                po_due_date,
                qty_ordered,
                unit_of_measure,
                remaining_quantity,
                user_id,
                is_non_po_item,
                is_quality_hold_required,
                is_quality_hold_acknowledged,
                quality_hold_restriction_type,
                part_skid_sequence,
                part_skid_total
            )
            SELECT
                COALESCE(rld.load_id, UUID())                                  AS load_guid,
                rld.quantity,
                rld.part_id,
                rld.part_description,
                rld.po_number,
                rld.po_line_number,
                rld.employee_number,
                rld.heat,
                COALESCE(DATE(rld.received_date), rld.transaction_date)        AS transaction_date,
                COALESCE(rld.received_date, rld.created_at, CURRENT_TIMESTAMP) AS created_at,
                rld.initial_location,
                rld.load_number,
                rld.packages_per_load,
                rld.package_type_name,
                rld.weight_per_package,
                rld.coils_on_skid,
                COALESCE(rld.label_number, 1)                                  AS label_number,
                COALESCE(rld.vendor_name, rld.po_vendor)                       AS vendor_name,
                rld.po_status,
                rld.po_due_date,
                rld.qty_ordered,
                COALESCE(NULLIF(rld.unit_of_measure, ''), 'EA')                AS unit_of_measure,
                rld.remaining_quantity,
                rld.user_id,
                COALESCE(rld.is_non_po_item, 0),
                COALESCE(rld.is_quality_hold_required, 0),
                COALESCE(rld.is_quality_hold_acknowledged, 0),
                rld.quality_hold_restriction_type,
                rld.part_skid_sequence,
                rld.part_skid_total
            FROM receiving_label_data rld
            WHERE rld.is_reprint = 0
              AND (COALESCE(p_clear_all, 0) = 1
                OR rld.employee_number = p_employee_number)
            ON DUPLICATE KEY UPDATE
                quantity                      = VALUES(quantity),
                part_id                       = VALUES(part_id),
                part_description              = VALUES(part_description),
                po_number                     = VALUES(po_number),
                po_line_number                = VALUES(po_line_number),
                employee_number               = VALUES(employee_number),
                heat                          = VALUES(heat),
                transaction_date              = VALUES(transaction_date),
                created_at                    = VALUES(created_at),
                initial_location              = VALUES(initial_location),
                load_number                   = VALUES(load_number),
                packages_per_load             = VALUES(packages_per_load),
                package_type_name             = VALUES(package_type_name),
                weight_per_package            = VALUES(weight_per_package),
                coils_on_skid                 = VALUES(coils_on_skid),
                label_number                  = VALUES(label_number),
                vendor_name                   = VALUES(vendor_name),
                po_status                     = VALUES(po_status),
                po_due_date                   = VALUES(po_due_date),
                qty_ordered                   = VALUES(qty_ordered),
                unit_of_measure               = VALUES(unit_of_measure),
                remaining_quantity            = VALUES(remaining_quantity),
                user_id                       = VALUES(user_id),
                is_non_po_item                = VALUES(is_non_po_item),
                is_quality_hold_required      = VALUES(is_quality_hold_required),
                is_quality_hold_acknowledged  = VALUES(is_quality_hold_acknowledged),
                quality_hold_restriction_type = VALUES(quality_hold_restriction_type),
                part_skid_sequence            = VALUES(part_skid_sequence),
                part_skid_total               = VALUES(part_skid_total);

            -- Delete all matching rows (reprint rows are deleted too; they are already in history).
            DELETE FROM receiving_label_data
            WHERE COALESCE(p_clear_all, 0) = 1
               OR employee_number = p_employee_number;

            COMMIT;

            SET p_rows_moved    = v_rows_to_move;
            SET p_status        = 0;
            SET p_error_message = NULL;
        END IF;
    END IF;
END $$

DELIMITER ;

-- ---------------------------------------------------------------------------
-- Step 4: sp_Receiving_LabelData_InsertFromHistory  (new procedure)
-- ---------------------------------------------------------------------------

DROP PROCEDURE IF EXISTS `sp_Receiving_LabelData_InsertFromHistory`;

DELIMITER $$

CREATE PROCEDURE `sp_Receiving_LabelData_InsertFromHistory`(
    IN p_history_id      INT,
    IN p_queued_by       VARCHAR(100),
    IN p_employee_number INT
)
BEGIN
    DECLARE v_already_queued INT DEFAULT 0;

    -- Guard: block re-queuing if a reprint row for this history GUID already exists.
    SELECT COUNT(*)
    INTO v_already_queued
    FROM receiving_label_data rld
    INNER JOIN receiving_history rh ON rh.load_guid = rld.load_id
    WHERE rh.id        = p_history_id
      AND rld.is_reprint = 1;

    IF v_already_queued > 0 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'This history record is already queued for reprint.';
    ELSE
        INSERT INTO receiving_label_data (
            load_id,
            load_number,
            quantity,
            weight_quantity,
            part_id,
            part_description,
            part_type,
            po_number,
            po_line_number,
            po_vendor,
            po_status,
            po_due_date,
            qty_ordered,
            unit_of_measure,
            remaining_quantity,
            employee_number,
            user_id,
            heat,
            received_date,
            transaction_date,
            initial_location,
            packages_per_load,
            package_type_name,
            weight_per_package,
            coils_on_skid,
            label_number,
            vendor_name,
            is_non_po_item,
            is_quality_hold_required,
            is_quality_hold_acknowledged,
            is_reprint,
            quality_hold_restriction_type
        )
        SELECT
            rh.load_guid,
            rh.load_number,
            rh.quantity,
            CAST(rh.quantity AS DECIMAL(18,2)),
            rh.part_id,
            rh.part_description,
            NULL,
            rh.po_number,
            rh.po_line_number,
            rh.vendor_name,
            rh.po_status,
            rh.po_due_date,
            rh.qty_ordered,
            rh.unit_of_measure,
            rh.remaining_quantity,
            COALESCE(p_employee_number, rh.employee_number),
            COALESCE(p_queued_by, rh.user_id),
            rh.heat,
            rh.created_at,
            rh.transaction_date,
            rh.initial_location,
            rh.packages_per_load,
            rh.package_type_name,
            rh.weight_per_package,
            rh.coils_on_skid,
            rh.label_number,
            rh.vendor_name,
            rh.is_non_po_item,
            rh.is_quality_hold_required,
            rh.is_quality_hold_acknowledged,
            1,
            rh.quality_hold_restriction_type
        FROM receiving_history rh
        WHERE rh.id = p_history_id;

        SELECT ROW_COUNT() AS rows_inserted;
    END IF;
END $$

DELIMITER ;
