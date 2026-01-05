DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_routing_label_insert` $$

CREATE PROCEDURE `sp_routing_label_insert`(
    IN p_po_number VARCHAR(20),
    IN p_line_number VARCHAR(10),
    IN p_description VARCHAR(200),
    IN p_recipient_id INT,
    IN p_quantity INT,
    IN p_created_by INT,
    IN p_other_reason_id INT,
    OUT p_new_label_id INT,
    OUT p_status INT,
    OUT p_error_msg VARCHAR(500)
)
sp_routing_label_insert: BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1 p_error_msg = MESSAGE_TEXT;
        SET p_status = -1;
        ROLLBACK;
    END;

    START TRANSACTION;

    -- Validation
    IF p_po_number IS NULL OR p_po_number = '' THEN
        SET p_status = -1;
        SET p_error_msg = 'PO number is required';
        ROLLBACK;
        LEAVE sp_routing_label_insert;
    END IF;

    IF p_quantity <= 0 THEN
        SET p_status = -1;
        SET p_error_msg = 'Quantity must be greater than zero';
        ROLLBACK;
        LEAVE sp_routing_label_insert;
    END IF;

    IF p_recipient_id IS NULL THEN
        SET p_status = -1;
        SET p_error_msg = 'Recipient is required';
        ROLLBACK;
        LEAVE sp_routing_label_insert;
    END IF;

    -- Check recipient exists and is active
    IF NOT EXISTS (SELECT 1 FROM routing_recipients WHERE id = p_recipient_id AND is_active = 1) THEN
        SET p_status = -1;
        SET p_error_msg = 'Recipient not found or inactive';
        ROLLBACK;
        LEAVE sp_routing_label_insert;
    END IF;

    -- If OTHER PO, require other_reason_id
    IF p_po_number = 'OTHER' AND p_other_reason_id IS NULL THEN
        SET p_status = -1;
        SET p_error_msg = 'Other reason is required for OTHER PO number';
        ROLLBACK;
        LEAVE sp_routing_label_insert;
    END IF;

    -- Insert label
    INSERT INTO routing_labels (
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
        1,
        0
    );

    SET p_new_label_id = LAST_INSERT_ID();
    SET p_status = 1;
    SET p_error_msg = 'Label created successfully';

    COMMIT;
END $$

DELIMITER ;
