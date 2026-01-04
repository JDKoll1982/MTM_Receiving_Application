-- ============================================================================
-- Stored Procedure: sp_routing_label_insert
-- Description: Insert new routing label record
-- Feature: Routing Module (001-routing-module)
-- Created: January 4, 2026
-- ============================================================================

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS sp_routing_label_insert;

DELIMITER $$

CREATE PROCEDURE sp_routing_label_insert(
    IN p_label_number INT,
    IN p_deliver_to VARCHAR(100),
    IN p_department VARCHAR(100),
    IN p_package_description TEXT,
    IN p_po_number VARCHAR(20),
    IN p_work_order VARCHAR(50),
    IN p_employee_number VARCHAR(20),
    IN p_created_date DATE,
    OUT p_new_label_id INT,
    OUT p_error_message VARCHAR(500)
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        -- Get error details
        GET DIAGNOSTICS CONDITION 1
            p_error_message = MESSAGE_TEXT;
        ROLLBACK;
    END;
    
    -- Initialize output parameters
    SET p_new_label_id = NULL;
    SET p_error_message = NULL;
    
    -- Start transaction
    START TRANSACTION;
    
    -- Validate required fields
    IF p_label_number IS NULL OR p_label_number <= 0 THEN
        SET p_error_message = 'Label number is required and must be positive';
        ROLLBACK;
        LEAVE;
    END IF;
    
    IF p_deliver_to IS NULL OR TRIM(p_deliver_to) = '' THEN
        SET p_error_message = 'Deliver To recipient is required';
        ROLLBACK;
        LEAVE;
    END IF;
    
    IF p_department IS NULL OR TRIM(p_department) = '' THEN
        SET p_error_message = 'Department is required';
        ROLLBACK;
        LEAVE;
    END IF;
    
    IF p_employee_number IS NULL OR TRIM(p_employee_number) = '' THEN
        SET p_error_message = 'Employee number is required';
        ROLLBACK;
        LEAVE;
    END IF;
    
    IF p_created_date IS NULL THEN
        SET p_error_message = 'Created date is required';
        ROLLBACK;
        LEAVE;
    END IF;
    
    -- Insert routing label
    INSERT INTO routing_labels (
        label_number,
        deliver_to,
        department,
        package_description,
        po_number,
        work_order,
        employee_number,
        created_date,
        is_archived
    ) VALUES (
        p_label_number,
        p_deliver_to,
        p_department,
        p_package_description,
        p_po_number,
        p_work_order,
        p_employee_number,
        p_created_date,
        0  -- Default: not archived
    );
    
    -- Get the newly created ID
    SET p_new_label_id = LAST_INSERT_ID();
    
    -- Commit transaction
    COMMIT;
END$$

DELIMITER ;
