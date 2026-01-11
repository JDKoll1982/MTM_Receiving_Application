DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_routing_label_get_all` $$

CREATE PROCEDURE `sp_routing_label_get_all`(
    IN p_limit INT,
    IN p_offset INT,
    OUT p_status INT,
    OUT p_error_msg VARCHAR(500)
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1 p_error_msg = MESSAGE_TEXT;
        SET p_status = -1;
    END;

    IF p_limit IS NULL OR p_limit <= 0 THEN
        SET p_limit = 100;
    END IF;

    IF p_offset IS NULL THEN
        SET p_offset = 0;
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
        l.csv_exported,
        l.csv_export_date
    FROM routing_labels l
    INNER JOIN routing_recipients r ON l.recipient_id = r.id
    LEFT JOIN routing_other_reasons o ON l.other_reason_id = o.id
    WHERE l.is_active = 1
    ORDER BY l.created_date DESC
    LIMIT p_limit OFFSET p_offset;

    SET p_status = 1;
    SET p_error_msg = 'Labels retrieved successfully';
END $$

DELIMITER ;
