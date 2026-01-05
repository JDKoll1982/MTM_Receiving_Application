DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_routing_label_update` $$

CREATE PROCEDURE `sp_routing_label_update`(
    IN p_label_id INT,
    IN p_po_number VARCHAR(20),
    IN p_line_number VARCHAR(10),
    IN p_description VARCHAR(200),
    IN p_recipient_id INT,
    IN p_quantity INT,
    IN p_other_reason_id INT,
    OUT p_status INT,
    OUT p_error_msg VARCHAR(500)
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1 p_error_msg = MESSAGE_TEXT;
        SET p_status = -1;
        ROLLBACK;
    END;

    START TRANSACTION;

    -- Validation
    IF p_label_id IS NULL THEN
        SET p_status = -1;
        SET p_error_msg = 'Label ID is required';
        ROLLBACK;
        LEAVE sp_routing_label_update;
    END IF;

    IF p_quantity <= 0 THEN
        SET p_status = -1;
        SET p_error_msg = 'Quantity must be greater than zero';
        ROLLBACK;
        LEAVE sp_routing_label_update;
    END IF;

    -- Check label exists
    IF NOT EXISTS (SELECT 1 FROM routing_labels WHERE id = p_label_id AND is_active = 1) THEN
        SET p_status = -1;
        SET p_error_msg = 'Label not found or inactive';
        ROLLBACK;
        LEAVE sp_routing_label_update;
    END IF;

    -- Check recipient exists and is active
    IF NOT EXISTS (SELECT 1 FROM routing_recipients WHERE id = p_recipient_id AND is_active = 1) THEN
        SET p_status = -1;
        SET p_error_msg = 'Recipient not found or inactive';
        ROLLBACK;
        LEAVE sp_routing_label_update;
    END IF;

    -- Update label
    UPDATE routing_labels
    SET po_number = p_po_number,
        line_number = p_line_number,
        description = p_description,
        recipient_id = p_recipient_id,
        quantity = p_quantity,
        other_reason_id = p_other_reason_id,
        csv_exported = 0,  -- Reset export flag when edited
        csv_export_date = NULL
    WHERE id = p_label_id;

    SET p_status = 1;
    SET p_error_msg = 'Label updated successfully';

    COMMIT;
END $$

DELIMITER ;
