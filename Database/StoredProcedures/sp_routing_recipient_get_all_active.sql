DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_routing_recipient_get_all_active` $$

CREATE PROCEDURE `sp_routing_recipient_get_all_active`(
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
        name,
        location,
        department,
        is_active,
        created_date,
        updated_date
    FROM routing_recipients
    WHERE is_active = 1
    ORDER BY name;

    SET p_status = 1;
    SET p_error_msg = 'Recipients retrieved successfully';
END $$

DELIMITER ;
