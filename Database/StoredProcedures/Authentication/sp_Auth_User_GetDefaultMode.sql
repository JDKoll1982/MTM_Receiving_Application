DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Auth_User_GetDefaultMode`$$

CREATE PROCEDURE `sp_Auth_User_GetDefaultMode`(
    IN p_user_id INT
)
BEGIN
    -- Retrieve user's default receiving mode

    SELECT default_receiving_mode
    FROM auth_users
    WHERE employee_number = p_user_id;
END$$

DELIMITER ;
