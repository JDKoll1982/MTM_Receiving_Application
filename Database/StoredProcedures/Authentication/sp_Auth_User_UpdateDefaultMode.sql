DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Auth_User_UpdateDefaultMode`$$

CREATE PROCEDURE `sp_Auth_User_UpdateDefaultMode`(
    IN p_user_id INT,
    IN p_default_mode VARCHAR(20)
)
BEGIN
    -- Update user's default receiving mode
    -- p_default_mode can be 'guided', 'manual', or NULL

    UPDATE auth_users
    SET default_receiving_mode = p_default_mode
    WHERE employee_number = p_user_id;

    SELECT ROW_COUNT() AS affected_rows;
END$$

DELIMITER ;
