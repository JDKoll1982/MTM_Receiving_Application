-- ============================================================================
-- Test Script: 07-Test-Dunnage-LabelData-ClearToHistory.sql
-- Module: Dunnage
-- Purpose:
--   Validate atomic clear behavior:
--     dunnage_label_data -> dunnage_history + queue delete
-- Safety:
--   Aborts immediately if dunnage_label_data is not empty to avoid touching live work.
-- ============================================================================

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS sp_Test_Dunnage_LabelData_ClearToHistory;

DELIMITER $$

CREATE PROCEDURE sp_Test_Dunnage_LabelData_ClearToHistory()
BEGIN
    DECLARE v_existing_queue_count  INT DEFAULT 0;
    DECLARE v_rows_moved            INT DEFAULT 0;
    DECLARE v_status                INT DEFAULT 1;
    DECLARE v_error_message         VARCHAR(1000) DEFAULT NULL;
    DECLARE v_batch_id              CHAR(36) DEFAULT NULL;
    DECLARE v_archived_count        INT DEFAULT 0;
    DECLARE v_remaining_queue_count INT DEFAULT 0;

    DECLARE v_load_uuid_1  CHAR(36);
    DECLARE v_load_uuid_2  CHAR(36);

    -- 0) Safety gate - do not run against a non-empty queue
    SELECT COUNT(*) INTO v_existing_queue_count FROM dunnage_label_data;
    IF v_existing_queue_count > 0 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Safety stop: dunnage_label_data is not empty. Run this test only when queue is empty.';
    END IF;

    SET v_load_uuid_1 = UUID();
    SET v_load_uuid_2 = UUID();

    -- 1) Seed two test rows into the active queue
    INSERT INTO dunnage_label_data
    (
        load_uuid,
        part_id,
        dunnage_type_id,
        dunnage_type_name,
        dunnage_type_icon,
        quantity,
        po_number,
        received_date,
        user_id,
        location,
        label_number,
        specs_json
    )
    VALUES
    (
        v_load_uuid_1,
        'TEST-PART-A',
        1,
        'Corrugated Cardboard',
        'PackageVariantClosed',
        50.00,
        'PO-TEST-CLR-001',
        NOW(),
        'TEST-CLEAR-USER',
        'DOCK-A',
        'LBL-C001',
        '{"width": "48", "length": "40"}'
    ),
    (
        v_load_uuid_2,
        'TEST-PART-B',
        2,
        'Foam Insert',
        'LayersOutline',
        12.00,
        NULL,
        NOW(),
        'TEST-CLEAR-USER',
        'RACK-B1',
        'LBL-C002',
        '{"thickness": "1in"}'
    );

    -- 2) Execute the clear-to-history SP
    CALL sp_Dunnage_LabelData_ClearToHistory(
        'TEST-CLEAR-USER',
        v_rows_moved,
        v_batch_id,
        v_status,
        v_error_message
    );

    -- 3) Assert SP reported success
    IF v_status <> 0 THEN
        -- Cleanup any partial state
        DELETE FROM dunnage_label_data  WHERE load_uuid IN (v_load_uuid_1, v_load_uuid_2);
        DELETE FROM dunnage_history     WHERE load_uuid IN (v_load_uuid_1, v_load_uuid_2);
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'FAIL: sp_Dunnage_LabelData_ClearToHistory returned non-zero status.';
    END IF;

    IF v_rows_moved <> 2 THEN
        DELETE FROM dunnage_history WHERE load_uuid IN (v_load_uuid_1, v_load_uuid_2);
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'FAIL: Expected p_rows_moved = 2.';
    END IF;

    IF v_batch_id IS NULL THEN
        DELETE FROM dunnage_history WHERE load_uuid IN (v_load_uuid_1, v_load_uuid_2);
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'FAIL: p_archive_batch_id is NULL.';
    END IF;

    -- 4) Assert queue is now empty
    SELECT COUNT(*) INTO v_remaining_queue_count FROM dunnage_label_data;
    IF v_remaining_queue_count > 0 THEN
        DELETE FROM dunnage_history WHERE load_uuid IN (v_load_uuid_1, v_load_uuid_2);
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'FAIL: dunnage_label_data is not empty after clear.';
    END IF;

    -- 5) Assert both rows are in dunnage_history with archive metadata
    SELECT COUNT(*) INTO v_archived_count
    FROM dunnage_history
    WHERE load_uuid IN (v_load_uuid_1, v_load_uuid_2)
      AND archive_batch_id = v_batch_id
      AND archived_by = 'TEST-CLEAR-USER'
      AND archived_at IS NOT NULL;

    IF v_archived_count <> 2 THEN
        DELETE FROM dunnage_history WHERE load_uuid IN (v_load_uuid_1, v_load_uuid_2);
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'FAIL: Expected 2 archived rows in dunnage_history with correct metadata.';
    END IF;

    -- 6) Assert field parity: specs_json and po_number survived the move
    IF NOT EXISTS (
        SELECT 1 FROM dunnage_history
        WHERE load_uuid = v_load_uuid_1
          AND po_number  = 'PO-TEST-CLR-001'
          AND specs_json IS NOT NULL
    ) THEN
        DELETE FROM dunnage_history WHERE load_uuid IN (v_load_uuid_1, v_load_uuid_2);
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'FAIL: Field parity check failed — po_number or specs_json missing from dunnage_history.';
    END IF;

    -- 7) Cleanup test rows from history
    DELETE FROM dunnage_history WHERE load_uuid IN (v_load_uuid_1, v_load_uuid_2);

    SELECT 'PASS: sp_Dunnage_LabelData_ClearToHistory atomic move, metadata stamp, queue empty, and field parity all verified.' AS test_result;
END$$

DELIMITER ;

CALL sp_Test_Dunnage_LabelData_ClearToHistory();
DROP PROCEDURE IF EXISTS sp_Test_Dunnage_LabelData_ClearToHistory;

-- ============================================================================
