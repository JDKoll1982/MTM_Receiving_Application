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
    IN p_label_number INT, -- Ignored (ID is Auto-Increment)
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
proc: BEGIN
    DECLARE v_recipient_id INT;

    -- Handler must be declared first
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        -- Get error details (Compatible with MySQL 5.6+)
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
    -- p_label_number check removed as we use Auto-Increment ID
    
    IF p_deliver_to IS NULL OR TRIM(p_deliver_to) = '' THEN
        SET p_error_message = 'Deliver To recipient is required';
        ROLLBACK;
        LEAVE proc;
    END IF;
    
    -- Resolve Recipient (Find or Create)
    SELECT id INTO v_recipient_id FROM routing_recipients WHERE name = p_deliver_to LIMIT 1;
    
    IF v_recipient_id IS NULL THEN
        INSERT INTO routing_recipients (name, location, department, is_active) 
        VALUES (p_deliver_to, 'Unknown', p_department, 1);
        SET v_recipient_id = LAST_INSERT_ID();
    END IF;
    
    -- Insert routing label
    INSERT INTO routing_labels (
        po_number,
        line_number,      -- Mapping work_order to line_number
        description,      -- Mapping package_description to description
        recipient_id,
        quantity,         -- Defaulting to 1 as not provided in params
        created_by,       -- Casting employee_number string to int
        created_date,
        is_active,
        csv_exported
    ) VALUES (
        p_po_number,
        p_work_order,
        p_package_description,
        v_recipient_id,
        1,
        CAST(p_employee_number AS UNSIGNED),
        p_created_date,
        1,  -- Active
        0   -- Not exported
    );
    
    -- Get the newly created ID
    SET p_new_label_id = LAST_INSERT_ID();
    
    -- Commit transaction
    COMMIT;
END proc$$

DELIMITER ;
