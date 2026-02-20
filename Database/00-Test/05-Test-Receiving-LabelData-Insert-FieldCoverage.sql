-- ============================================================================
-- Test Script: 05-Test-Receiving-LabelData-Insert-FieldCoverage.sql
-- Module: Receiving
-- Purpose:
--   Validate queue-write field coverage for sp_Receiving_LabelData_Insert.
-- Safety:
--   Isolated by unique test load_id and deterministic cleanup.
-- ============================================================================

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS sp_Test_Receiving_LabelData_Insert_FieldCoverage;

DELIMITER $$

CREATE PROCEDURE sp_Test_Receiving_LabelData_Insert_FieldCoverage()
BEGIN
    DECLARE v_test_load_id CHAR(36);
    DECLARE v_inserted_count INT DEFAULT 0;

    DECLARE v_part_id VARCHAR(50);
    DECLARE v_part_description VARCHAR(500);
    DECLARE v_part_type VARCHAR(50);
    DECLARE v_po_number VARCHAR(20);
    DECLARE v_po_line_number VARCHAR(10);
    DECLARE v_po_vendor VARCHAR(255);
    DECLARE v_po_status VARCHAR(100);
    DECLARE v_po_due_date DATE;
    DECLARE v_qty_ordered DECIMAL(18,2);
    DECLARE v_unit_of_measure VARCHAR(20);
    DECLARE v_remaining_quantity INT;
    DECLARE v_employee_number INT;
    DECLARE v_user_id VARCHAR(100);
    DECLARE v_heat VARCHAR(100);
    DECLARE v_packages_per_load INT;
    DECLARE v_package_type_name VARCHAR(50);
    DECLARE v_weight_per_package DECIMAL(18,2);
    DECLARE v_weight_quantity DECIMAL(18,2);
    DECLARE v_is_non_po_item TINYINT(1);
    DECLARE v_is_quality_hold_required TINYINT(1);
    DECLARE v_is_quality_hold_acknowledged TINYINT(1);
    DECLARE v_quality_hold_restriction_type VARCHAR(255);

    SET v_test_load_id = UUID();

    -- 1) Execute SP under test with full field payload
    CALL sp_Receiving_LabelData_Insert(
        v_test_load_id,
        7,
        321,
        321.00,
        'TEST-FC-001',
        'Field Coverage Test Part',
        'Standard',
        '54321',
        '70',
        'Field Coverage Vendor',
        'Open',
        CURDATE(),
        555.00,
        'EA',
        234,
        4242,
        'field-coverage-user',
        'HEAT-FC-001',
        NOW(),
        CURDATE(),
        'FC-LOC',
        3,
        'Skid',
        107.00,
        8,
        7,
        'Field Coverage Vendor',
        0,
        1,
        1,
        'FC-Quality-Hold'
    );

    -- 2) Assert row exists
    SELECT COUNT(*) INTO v_inserted_count
    FROM receiving_label_data
    WHERE load_id = v_test_load_id;

    IF v_inserted_count <> 1 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = CONCAT('FAIL: expected 1 inserted queue row, actual=', v_inserted_count);
    END IF;

    -- 3) Read back and assert critical fields
    SELECT
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
        packages_per_load,
        package_type_name,
        weight_per_package,
        weight_quantity,
        is_non_po_item,
        is_quality_hold_required,
        is_quality_hold_acknowledged,
        quality_hold_restriction_type
    INTO
        v_part_id,
        v_part_description,
        v_part_type,
        v_po_number,
        v_po_line_number,
        v_po_vendor,
        v_po_status,
        v_po_due_date,
        v_qty_ordered,
        v_unit_of_measure,
        v_remaining_quantity,
        v_employee_number,
        v_user_id,
        v_heat,
        v_packages_per_load,
        v_package_type_name,
        v_weight_per_package,
        v_weight_quantity,
        v_is_non_po_item,
        v_is_quality_hold_required,
        v_is_quality_hold_acknowledged,
        v_quality_hold_restriction_type
    FROM receiving_label_data
    WHERE load_id = v_test_load_id;

    IF v_part_id <> 'TEST-FC-001' THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'FAIL: part_id mismatch';
    END IF;

    IF v_part_description <> 'Field Coverage Test Part' THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'FAIL: part_description mismatch';
    END IF;

    IF v_po_number <> '54321' OR v_po_line_number <> '70' THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'FAIL: PO fields mismatch';
    END IF;

    IF v_qty_ordered <> 555.00 OR v_remaining_quantity <> 234 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'FAIL: quantity snapshot fields mismatch';
    END IF;

    IF v_weight_quantity <> 321.00 OR v_weight_per_package <> 107.00 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'FAIL: weight fields mismatch';
    END IF;

    IF v_user_id <> 'field-coverage-user' OR v_employee_number <> 4242 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'FAIL: user fields mismatch';
    END IF;

    IF v_is_quality_hold_required <> 1 OR v_is_quality_hold_acknowledged <> 1 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'FAIL: quality hold flags mismatch';
    END IF;

    IF v_quality_hold_restriction_type <> 'FC-Quality-Hold' THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'FAIL: quality hold restriction mismatch';
    END IF;

    -- 4) Cleanup
    DELETE FROM receiving_label_data
    WHERE load_id = v_test_load_id;

    -- 5) Success output
    SELECT
        'PASS' AS result,
        v_test_load_id AS test_load_id,
        1 AS rows_validated;
END$$

DELIMITER ;

-- Execute test
CALL sp_Test_Receiving_LabelData_Insert_FieldCoverage();

-- Cleanup procedure object
DROP PROCEDURE IF EXISTS sp_Test_Receiving_LabelData_Insert_FieldCoverage;

-- ============================================================================
