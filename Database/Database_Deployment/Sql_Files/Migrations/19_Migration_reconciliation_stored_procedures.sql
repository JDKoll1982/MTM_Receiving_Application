-- =====================================================================
-- Migration: 19_Migration_reconciliation_stored_procedures
-- Module:    Reconciliation (Receiving Validator)
-- Purpose:
--   Creates the three procedures the Receiving Validator owns. This change adds
--   no tables and no columns: runs and changes are recorded in logs only
--   (FR-055). The procedures are the module's entire data surface over the
--   receiving tables, and the update procedure is the only write between the
--   validator and Receiving (FR-005, FR-056, R-MOD-08).
--
-- Objects created (all idempotent, DROP IF EXISTS then CREATE):
--   sp_Reconciliation_ReceivingRows_GetDay
--   sp_Reconciliation_DayActivity_GetRange
--   sp_Reconciliation_ReceivingLocation_Update
--
-- Canonical per-object sources, which these bodies mirror verbatim:
--   Database/Database_Deployment/Sql_Files/StoredProcedures/Reconciliation/
--
-- Lifecycle: Run AFTER 18_Migration_lot_markings_reachable_marks. Safe to re-run
--            at any time; every object is dropped and recreated.
--
-- Safety:    This file can modify a database, so it follows R-SQL-06. Do not run
--            it by hand against core. Use the wrapper, which defaults to
--            read-only, supports -WhatIf and -GenerateSql, backs up before
--            writing core, requires a typed confirmation, aborts and restores on
--            the first error, and validates against mtm_receiving_application_test
--            first:
--              Database/Database_Deployment/Deploy-Reconciliation-StoredProcedures.ps1
--
--            To reproduce the validation by hand:
--              1. mysqldump --single-transaction --routines mtm_receiving_application > before_19.sql
--              2. Apply this file to mtm_receiving_application_test.
--              3. Apply it to mtm_receiving_application only after the test schema
--                 accepts all three procedures.
--
-- Notes:     R-SQL-03 asks modifying procedures for SET NOCOUNT ON, which is SQL
--            Server syntax this MySQL server does not accept. The intent is met
--            instead: the update procedure emits no row-count result set and
--            reports its row count through p_affected_rows only.
-- =====================================================================

-- ---------------------------------------------------------------------
-- 1. The day's receiving rows, from both row sources.
-- ---------------------------------------------------------------------

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Reconciliation_ReceivingRows_GetDay` $$

CREATE PROCEDURE `sp_Reconciliation_ReceivingRows_GetDay`(
    IN p_transaction_date DATE
)
BEGIN
    SELECT
        'CurrentLabel' AS row_source,
        ld.id AS record_id,
        ld.load_id AS load_id,
        ld.load_number AS load_number,
        ld.part_id AS part_id,
        COALESCE(ld.part_description, '') AS part_description,
        COALESCE(ld.po_number, '') AS po_number,
        COALESCE(ld.po_line_number, '') AS po_line_number,
        ld.transaction_date AS transaction_date,
        ld.received_date AS received_date,
        ld.quantity AS quantity,
        ld.weight_quantity AS weight_quantity,
        COALESCE(ld.unit_of_measure, '') AS unit_of_measure,
        COALESCE(ld.initial_location, '') AS initial_location,
        COALESCE(NULLIF(ld.vendor_name, ''), ld.po_vendor, '') AS vendor_name,
        ld.is_non_po_item AS is_non_po_item,
        ld.is_reprint AS is_reprint
    FROM receiving_label_data ld
    WHERE ld.transaction_date = p_transaction_date

    UNION ALL

    SELECT
        'History' AS row_source,
        h.id AS record_id,
        h.load_id AS load_id,
        h.load_number AS load_number,
        h.part_id AS part_id,
        COALESCE(h.part_description, '') AS part_description,
        COALESCE(h.po_number, '') AS po_number,
        COALESCE(h.po_line_number, '') AS po_line_number,
        h.transaction_date AS transaction_date,
        h.received_date AS received_date,
        h.quantity AS quantity,
        h.weight_quantity AS weight_quantity,
        COALESCE(h.unit_of_measure, '') AS unit_of_measure,
        COALESCE(h.initial_location, '') AS initial_location,
        COALESCE(NULLIF(h.vendor_name, ''), h.po_vendor, '') AS vendor_name,
        h.is_non_po_item AS is_non_po_item,
        h.is_reprint AS is_reprint
    FROM receiving_history h
    WHERE h.transaction_date = p_transaction_date

    ORDER BY
        part_id ASC,
        transaction_date ASC,
        load_number ASC,
        load_id ASC,
        record_id ASC;
END $$

DELIMITER ;

-- ---------------------------------------------------------------------
-- 2. Which days hold receiving activity, cheaply.
-- ---------------------------------------------------------------------

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Reconciliation_DayActivity_GetRange` $$

CREATE PROCEDURE `sp_Reconciliation_DayActivity_GetRange`(
    IN p_from DATE,
    IN p_to DATE
)
BEGIN
    SELECT activity_date
    FROM (
        SELECT DISTINCT ld.transaction_date AS activity_date
        FROM receiving_label_data ld
        WHERE ld.transaction_date BETWEEN p_from AND p_to

        UNION

        SELECT DISTINCT h.transaction_date AS activity_date
        FROM receiving_history h
        WHERE h.transaction_date BETWEEN p_from AND p_to
    ) AS activity_days
    ORDER BY activity_date ASC;
END $$

DELIMITER ;

-- ---------------------------------------------------------------------
-- 3. The single write procedure.
-- ---------------------------------------------------------------------

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Reconciliation_ReceivingLocation_Update` $$

CREATE PROCEDURE `sp_Reconciliation_ReceivingLocation_Update`(
    IN p_row_source VARCHAR(16),
    IN p_record_id INT,
    IN p_destination_location VARCHAR(50),
    IN p_operator_user_id VARCHAR(100),
    OUT p_affected_rows INT
)
BEGIN
    DECLARE v_normalized_source VARCHAR(16);

    SET p_affected_rows = 0;

    START TRANSACTION;

    IF p_destination_location IS NULL OR TRIM(p_destination_location) = '' THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'A destination location is required.';
    END IF;

    IF p_operator_user_id IS NULL OR TRIM(p_operator_user_id) = '' THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'An operator user id is required.';
    END IF;

    IF p_record_id IS NULL OR p_record_id <= 0 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'A positive record id is required.';
    END IF;

    SET v_normalized_source = TRIM(p_row_source);

    IF v_normalized_source = 'CurrentLabel' THEN
        UPDATE receiving_label_data
        SET initial_location = TRIM(p_destination_location)
        WHERE id = p_record_id;
    ELSEIF v_normalized_source = 'History' THEN
        UPDATE receiving_history
        SET initial_location = TRIM(p_destination_location)
        WHERE id = p_record_id;
    ELSE
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'The row source must be CurrentLabel or History.';
    END IF;

    SET p_affected_rows = ROW_COUNT();

    COMMIT;
END $$

DELIMITER ;

-- TEST BLOCK START: Verify all three objects exist
-- SELECT ROUTINE_NAME, ROUTINE_TYPE
-- FROM information_schema.ROUTINES
-- WHERE ROUTINE_SCHEMA = DATABASE()
--   AND ROUTINE_NAME IN (
--       'sp_Reconciliation_ReceivingRows_GetDay',
--       'sp_Reconciliation_DayActivity_GetRange',
--       'sp_Reconciliation_ReceivingLocation_Update'
--   )
-- ORDER BY ROUTINE_NAME;
-- TEST BLOCK END: Verify all three objects exist
