-- ============================================================================
-- Test Script: 04-Test-Receiving-LabelData-ClearToHistory.sql
-- Module: Receiving
-- Purpose:
--   Validate atomic clear behavior:
--     receiving_label_data -> receiving_history + queue delete
-- Safety:
--   Aborts immediately if queue is not empty to avoid touching live work.
-- ============================================================================

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS sp_Test_Receiving_LabelData_ClearToHistory;

DELIMITER $$

CREATE PROCEDURE sp_Test_Receiving_LabelData_ClearToHistory()
BEGIN
    DECLARE v_existing_queue_count INT DEFAULT 0;
    DECLARE v_rows_moved INT DEFAULT 0;
    DECLARE v_status INT DEFAULT 1;
    DECLARE v_error_message VARCHAR(1000) DEFAULT NULL;
    DECLARE v_batch_id CHAR(36) DEFAULT NULL;
    DECLARE v_archived_count INT DEFAULT 0;
    DECLARE v_remaining_queue_count INT DEFAULT 0;

    DECLARE v_load_id_1 CHAR(36);
    DECLARE v_load_id_2 CHAR(36);

    -- 0) Safety gate - do not run against a non-empty queue
    SELECT COUNT(*) INTO v_existing_queue_count
    FROM receiving_label_data;

    IF v_existing_queue_count > 0 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Safety stop: receiving_label_data is not empty. Run this test only when queue is empty.';
    END IF;

    -- 1) Seed two queue rows with controlled test values
    SET v_load_id_1 = UUID();
    SET v_load_id_2 = UUID();

    INSERT INTO receiving_label_data
    (
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
        label_number,
        vendor_name,
        is_non_po_item,
        is_quality_hold_required,
        is_quality_hold_acknowledged,
        quality_hold_restriction_type
    )
    VALUES
    (
        v_load_id_1,
        1,
        100,
        100.00,
        'TEST-PHASE5-001',
        'Phase5 Test Part 001',
        'Standard',
        '12345',
        '1',
        'Test Vendor',
        'Open',
        CURDATE(),
        250.00,
        'EA',
        150,
        9999,
        'phase5-test-user',
        'HL-001',
        NOW(),
        CURDATE(),
        'TEST-LOC',
        2,
        'Skid',
        50.00,
        1,
        'Test Vendor',
        0,
        0,
        0,
        NULL
    ),
    (
        v_load_id_2,
        2,
        80,
        80.00,
        'TEST-PHASE5-002',
        'Phase5 Test Part 002',
        'Standard',
        '12345',
        '2',
        'Test Vendor',
        'Open',
        CURDATE(),
        250.00,
        'EA',
        170,
        9999,
        'phase5-test-user',
        'HL-002',
        NOW(),
        CURDATE(),
        'TEST-LOC',
        4,
        'Skid',
        20.00,
        2,
        'Test Vendor',
        0,
        1,
        1,
        'Quality Hold Test'
    );

    -- 2) Execute clear-to-history stored procedure
    CALL sp_Receiving_LabelData_ClearToHistory(
        'phase5-test-runner',
        v_rows_moved,
        v_batch_id,
        v_status,
        v_error_message
    );

    -- 3) Assertions
    IF v_status <> 0 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = CONCAT('FAIL: sp_Receiving_LabelData_ClearToHistory status=', v_status, ', error=', IFNULL(v_error_message, 'NULL'));
    END IF;

    IF v_rows_moved <> 2 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = CONCAT('FAIL: rows moved mismatch. Expected=2, Actual=', v_rows_moved);
    END IF;

    SELECT COUNT(*) INTO v_remaining_queue_count
    FROM receiving_label_data;

    IF v_remaining_queue_count <> 0 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = CONCAT('FAIL: queue not cleared. Remaining rows=', v_remaining_queue_count);
    END IF;

    SELECT COUNT(*) INTO v_archived_count
    FROM receiving_history
    WHERE ArchiveBatchID = v_batch_id
      AND PartID IN ('TEST-PHASE5-001', 'TEST-PHASE5-002');

    IF v_archived_count <> 2 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = CONCAT('FAIL: archive row count mismatch for batch. Expected=2, Actual=', v_archived_count);
    END IF;

    -- 4) Field parity checks for seeded rows
    IF EXISTS (
        SELECT 1
        FROM receiving_history rh
        WHERE rh.ArchiveBatchID = v_batch_id
          AND rh.PartID = 'TEST-PHASE5-001'
          AND (
            rh.PONumber <> '12345'
            OR rh.POLineNumber <> '1'
            OR rh.LoadNumber <> 1
            OR rh.WeightQuantity <> 100.00
            OR rh.PackagesPerLoad <> 2
            OR rh.PackageTypeName <> 'Skid'
            OR rh.UserID <> 'phase5-test-user'
          )
    ) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'FAIL: field parity mismatch for TEST-PHASE5-001';
    END IF;

    -- 5) Cleanup test artifacts
    DELETE FROM receiving_history
    WHERE ArchiveBatchID = v_batch_id
      AND PartID IN ('TEST-PHASE5-001', 'TEST-PHASE5-002');

    -- 6) Final success output
    SELECT
        'PASS' AS result,
        v_rows_moved AS rows_moved,
        v_batch_id AS archive_batch_id,
        v_archived_count AS archived_count;
END$$

DELIMITER ;

-- Execute test
CALL sp_Test_Receiving_LabelData_ClearToHistory();

-- Cleanup procedure object
DROP PROCEDURE IF EXISTS sp_Test_Receiving_LabelData_ClearToHistory;

-- ============================================================================
