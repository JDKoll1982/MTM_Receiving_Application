DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_routing_other_reason_get_all_active` $$

CREATE PROCEDURE `sp_routing_other_reason_get_all_active`(
    OUT p_status INT,
    OUT p_error_msg VARCHAR(500)
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1 p_error_msg = MESSAGE_TEXT;
        SET p_status = -1;
    END;

    SELECT
        id,
        reason_code,
        description,
        is_active,
        display_order
    FROM routing_po_alternatives
    WHERE is_active = 1
    ORDER BY display_order, description;

    SET p_status = 1;
    SET p_error_msg = 'Other reasons retrieved successfully';
END $$

DELIMITER ;
