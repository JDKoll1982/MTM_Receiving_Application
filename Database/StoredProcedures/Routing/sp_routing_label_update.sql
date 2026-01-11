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
    IN p_po_number VARCHAR(20),
    IN p_line_number VARCHAR(10),
    IN p_description VARCHAR(200),
    IN p_recipient_id INT,
    IN p_quantity INT,
    IN p_other_reason_id INT,
    OUT p_error_message VARCHAR(500)
)
proc: BEGIN
    DECLARE v_exists INT DEFAULT 0;
    DECLARE v_error_message VARCHAR(500) DEFAULT NULL;

    -- MySQL 5.7 compatible handler
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        GET DIAGNOSTICS CONDITION 1 v_error_message = MESSAGE_TEXT;
        SET p_error_message = v_error_message;
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
        SET p_error_message = 'Quantity must be positive';
        ROLLBACK;
        LEAVE proc;
    END IF;

    -- Update label
    UPDATE routing_label_data
    SET
        po_number = p_po_number,
        line_number = p_line_number,
        description = p_description,
        recipient_id = p_recipient_id,
        quantity = p_quantity,
        other_reason_id = p_other_reason_id
    WHERE id = p_id;

    -- Commit transaction
    COMMIT;
END proc$$

DELIMITER ;
