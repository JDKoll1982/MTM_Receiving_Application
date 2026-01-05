DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_routing_label_get_by_id` $$

CREATE PROCEDURE `sp_routing_label_get_by_id`(
    IN p_label_id INT,
    OUT p_status INT,
    OUT p_error_msg VARCHAR(500)
)
sp_routing_label_get_by_id: BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1 p_error_msg = MESSAGE_TEXT;
        SET p_status = -1;
    END;

    IF p_label_id IS NULL THEN
        SET p_status = -1;
        SET p_error_msg = 'Label ID is required';
        LEAVE sp_routing_label_get_by_id;
    END IF;

    SELECT
        l.id,
        l.po_number,
        l.line_number,
        l.description,
        l.recipient_id,
        r.name AS recipient_name,
        r.location AS recipient_location,
        l.quantity,
        l.created_by,
        l.created_date,
        l.other_reason_id,
        o.description AS other_reason_description,
        l.is_active,
        l.csv_exported,
        l.csv_export_date
    FROM routing_labels l
    INNER JOIN routing_recipients r ON l.recipient_id = r.id
    LEFT JOIN routing_other_reasons o ON l.other_reason_id = o.id
    WHERE l.id = p_label_id AND l.is_active = 1;

    SET p_status = 1;
    SET p_error_msg = 'Label retrieved successfully';
END $$

DELIMITER ;
