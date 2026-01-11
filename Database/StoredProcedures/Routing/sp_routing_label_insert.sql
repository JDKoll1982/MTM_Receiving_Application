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
    IN p_po_number VARCHAR(20),
    IN p_line_number VARCHAR(10),
    IN p_description VARCHAR(200),
    IN p_recipient_id INT,
    IN p_quantity INT,
    IN p_created_by INT,
    IN p_other_reason_id INT,
    OUT p_new_label_id INT,
    OUT p_error_message VARCHAR(500)
)
proc: BEGIN
    -- Local variables must be declared at the top of the block (MySQL requirement)
    DECLARE v_error_message VARCHAR(500);

    -- Handler must be declared after local DECLAREs
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        -- Capture error details into a local variable, then assign to OUT param
        GET DIAGNOSTICS CONDITION 1
            v_error_message = MESSAGE_TEXT;
        SET p_error_message = v_error_message;
        ROLLBACK;
    END;

    -- Initialize output parameters
    SET p_new_label_id = NULL;
    SET p_error_message = NULL;

    -- Start transaction
    START TRANSACTION;

    -- Validate required fields
    IF p_po_number IS NULL OR TRIM(p_po_number) = '' THEN
        SET p_error_message = 'PO number is required';
        ROLLBACK;
        LEAVE proc;
    END IF;

    IF p_line_number IS NULL OR TRIM(p_line_number) = '' THEN
        SET p_error_message = 'Line number is required';
        ROLLBACK;
        LEAVE proc;
    END IF;

    IF p_description IS NULL OR TRIM(p_description) = '' THEN
        SET p_error_message = 'Description is required';
        ROLLBACK;
        LEAVE proc;
    END IF;

    IF p_recipient_id IS NULL THEN
        SET p_error_message = 'Recipient ID is required';
        ROLLBACK;
        LEAVE proc;
    END IF;

    IF p_quantity IS NULL OR p_quantity <= 0 THEN
        SET p_error_message = 'Quantity is required and must be positive';
        ROLLBACK;
        LEAVE proc;
    END IF;

    IF p_created_by IS NULL THEN
        SET p_error_message = 'Created by user is required';
        ROLLBACK;
        LEAVE proc;
    END IF;

    -- Insert routing label
    INSERT INTO routing_label_data (
        po_number,
        line_number,
        description,
        recipient_id,
        quantity,
        created_by,
        other_reason_id,
        is_active,
        csv_exported
    ) VALUES (
        p_po_number,
        p_line_number,
        p_description,
        p_recipient_id,
        p_quantity,
        p_created_by,
        p_other_reason_id,
        1,  -- Default: active
        0   -- Default: not exported
    );

    -- Get the newly created ID
    SET p_new_label_id = LAST_INSERT_ID();

    -- Commit transaction
    COMMIT;
END proc$$

DELIMITER ;
