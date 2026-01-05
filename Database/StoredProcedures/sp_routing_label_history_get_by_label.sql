DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_routing_label_history_get_by_label` $$

CREATE PROCEDURE `sp_routing_label_history_get_by_label`(
    IN p_label_id INT,
    OUT p_status INT,
    OUT p_error_msg VARCHAR(500)
)
sp_routing_label_history_get_by_label: BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1 p_error_msg = MESSAGE_TEXT;
        SET p_status = -1;
    END;

    IF p_label_id IS NULL THEN
        SET p_status = -1;
        SET p_error_msg = 'Label ID is required';
        LEAVE sp_routing_label_history_get_by_label;
    END IF;

    SELECT
        h.id,
        h.label_id,
        h.field_changed,
        h.old_value,
        h.new_value,
        h.edited_by,
        h.edit_date
    FROM routing_label_history h
    WHERE h.label_id = p_label_id
    ORDER BY h.edit_date DESC;

    SET p_status = 1;
    SET p_error_msg = 'Label history retrieved successfully';
END $$

DELIMITER ;
