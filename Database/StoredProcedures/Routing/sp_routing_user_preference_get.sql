DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_routing_user_preference_get` $$

CREATE PROCEDURE `sp_routing_user_preference_get`(
    IN p_employee_number INT,
    OUT p_status INT,
    OUT p_error_msg VARCHAR(500)
)
sp_routing_user_preference_get: BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1 p_error_msg = MESSAGE_TEXT;
        SET p_status = -1;
    END;

    IF p_employee_number IS NULL THEN
        SET p_status = -1;
        SET p_error_msg = 'Employee number is required';
        LEAVE sp_routing_user_preference_get;
    END IF;

    SELECT
        id,
        employee_number,
        default_mode,
        enable_validation,
        updated_date
    FROM settings_routing_personal
    WHERE employee_number = p_employee_number;

    SET p_status = 1;
    SET p_error_msg = 'User preferences retrieved';
END $$

DELIMITER ;
