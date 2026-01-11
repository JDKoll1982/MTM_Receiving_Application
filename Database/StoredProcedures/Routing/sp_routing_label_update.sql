-- ============================================================================
-- Stored Procedure: sp_routing_label_update
-- Description: Update existing routing label
-- Feature: Routing Module (001-routing-module)
-- Created: January 4, 2026
-- ============================================================================

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS sp_routing_label_update;

DELIMITER $$

CREATE PROCEDURE sp_routing_label_update(
    IN p_id INT,
    IN p_label_number INT,
    IN p_deliver_to VARCHAR(100),
    IN p_department VARCHAR(100),
    IN p_package_description TEXT,
    IN p_po_number VARCHAR(20),
    IN p_work_order VARCHAR(50),
    OUT p_error_message VARCHAR(500)
)
proc: BEGIN
    DECLARE v_exists INT DEFAULT 0;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1
            p_error_message = MESSAGE_TEXT;
        ROLLBACK;
    END;

    -- Initialize output
    SET p_error_message = NULL;

    -- Start transaction
    START TRANSACTION;

    -- Validate ID exists
    SELECT COUNT(*) INTO v_exists
    FROM routing_label_data
    WHERE id = p_id;

    IF v_exists = 0 THEN
        SET p_error_message = 'Label ID not found';
        ROLLBACK;
        LEAVE proc;
    END IF;

    -- Validate required fields
    IF p_deliver_to IS NULL OR TRIM(p_deliver_to) = '' THEN
        SET p_error_message = 'Deliver To recipient is required';
        ROLLBACK;
        LEAVE proc;
    END IF;

    IF p_department IS NULL OR TRIM(p_department) = '' THEN
        SET p_error_message = 'Department is required';
        ROLLBACK;
        LEAVE proc;
    END IF;

    -- Update label
    UPDATE routing_label_data
    SET
        label_number = p_label_number,
        deliver_to = p_deliver_to,
        department = p_department,
        package_description = p_package_description,
        po_number = p_po_number,
        work_order = p_work_order
    WHERE id = p_id;

    -- Commit transaction
    COMMIT;
END proc$$

DELIMITER ;
