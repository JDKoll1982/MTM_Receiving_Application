-- ============================================================================
-- Test Script: 06-Test-Dunnage-LabelData-InsertAndVerify.sql
-- Module: Dunnage
-- Purpose:
--   Validate that sp_Dunnage_LabelData_Insert and sp_Dunnage_LabelData_InsertBatch
--   correctly persist all fixed fields and specs_json into dunnage_label_data.
-- Safety:
--   Aborts immediately if dunnage_label_data is not empty to avoid touching live work.
--   Cleans up test rows by load_uuid after assertions.
-- ============================================================================

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS sp_Test_Dunnage_LabelData_InsertAndVerify;

DELIMITER $$

CREATE PROCEDURE sp_Test_Dunnage_LabelData_InsertAndVerify()
BEGIN
    DECLARE v_existing_count  INT DEFAULT 0;
    DECLARE v_load_uuid_1     CHAR(36);
    DECLARE v_load_uuid_2     CHAR(36);
    DECLARE v_row_count       INT DEFAULT 0;

    -- Field assertion variables
    DECLARE v_part_id         VARCHAR(50);
    DECLARE v_type_name       VARCHAR(100);
    DECLARE v_quantity        DECIMAL(10,2);
    DECLARE v_po_number       VARCHAR(50);
    DECLARE v_user_id         VARCHAR(100);
    DECLARE v_location        VARCHAR(100);
    DECLARE v_label_number    VARCHAR(50);
    DECLARE v_specs_json      JSON;

    -- 0) Safety gate
    SELECT COUNT(*) INTO v_existing_count FROM dunnage_label_data;
    IF v_existing_count > 0 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Safety stop: dunnage_label_data is not empty. Run this test only when queue is empty.';
    END IF;

    SET v_load_uuid_1 = UUID();
    SET v_load_uuid_2 = UUID();

    -- 1) Single-row insert via sp_Dunnage_LabelData_Insert
    CALL sp_Dunnage_LabelData_Insert(
        v_load_uuid_1,              -- p_load_uuid
        'TEST-PART-001',            -- p_part_id
        1,                          -- p_dunnage_type_id
        'Corrugated Cardboard',     -- p_dunnage_type_name
        'PackageVariantClosed',     -- p_dunnage_type_icon
        25.00,                      -- p_quantity
        'PO-TEST-001',              -- p_po_number
        NOW(),                      -- p_received_date
        'TEST-USER',                -- p_user_id
        'DOCK-A',                   -- p_location
        'LBL-001',                  -- p_label_number
        '{"width": "48", "length": "40", "material": "Cardboard"}' -- p_specs_json
    );

    -- 2) Batch insert via sp_Dunnage_LabelData_InsertBatch (second row)
    CALL sp_Dunnage_LabelData_InsertBatch(
        JSON_ARRAY(
            JSON_OBJECT(
                'load_uuid',         v_load_uuid_2,
                'part_id',           'TEST-PART-002',
                'dunnage_type_id',   2,
                'dunnage_type_name', 'Foam Insert',
                'dunnage_type_icon', 'LayersOutline',
                'quantity',          10.00,
                'po_number',         NULL,
                'received_date',     DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s'),
                'location',          'RACK-B3',
                'label_number',      'LBL-002',
                'specs_json',        JSON_OBJECT('thickness', '2in', 'color', 'grey')
            )
        ),
        'TEST-USER'
    );

    -- 3) Assert both rows were inserted
    SELECT COUNT(*) INTO v_row_count
    FROM dunnage_label_data
    WHERE load_uuid IN (v_load_uuid_1, v_load_uuid_2);

    IF v_row_count <> 2 THEN
        CALL sp_Test_Dunnage_LabelData_Cleanup(v_load_uuid_1, v_load_uuid_2);
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'FAIL: Expected 2 rows in dunnage_label_data after inserts.';
    END IF;

    -- 4) Assert full field coverage on the single-row insert
    SELECT part_id, dunnage_type_name, quantity, po_number, user_id, location, label_number, specs_json
    INTO v_part_id, v_type_name, v_quantity, v_po_number, v_user_id, v_location, v_label_number, v_specs_json
    FROM dunnage_label_data
    WHERE load_uuid = v_load_uuid_1;

    IF v_part_id       <> 'TEST-PART-001'         THEN SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'FAIL: part_id mismatch'; END IF;
    IF v_type_name     <> 'Corrugated Cardboard'   THEN SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'FAIL: dunnage_type_name mismatch'; END IF;
    IF v_quantity      <> 25.00                    THEN SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'FAIL: quantity mismatch'; END IF;
    IF v_po_number     <> 'PO-TEST-001'            THEN SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'FAIL: po_number mismatch'; END IF;
    IF v_user_id       <> 'TEST-USER'              THEN SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'FAIL: user_id mismatch'; END IF;
    IF v_location      <> 'DOCK-A'                 THEN SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'FAIL: location mismatch'; END IF;
    IF v_label_number  <> 'LBL-001'               THEN SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'FAIL: label_number mismatch'; END IF;
    IF v_specs_json    IS NULL                     THEN SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'FAIL: specs_json is NULL'; END IF;

    -- 5) Cleanup test rows
    DELETE FROM dunnage_label_data WHERE load_uuid IN (v_load_uuid_1, v_load_uuid_2);

    SELECT 'PASS: sp_Dunnage_LabelData_Insert and InsertBatch full-field coverage verified.' AS test_result;
END$$

DELIMITER ;

CALL sp_Test_Dunnage_LabelData_InsertAndVerify();
DROP PROCEDURE IF EXISTS sp_Test_Dunnage_LabelData_InsertAndVerify;

-- ============================================================================
