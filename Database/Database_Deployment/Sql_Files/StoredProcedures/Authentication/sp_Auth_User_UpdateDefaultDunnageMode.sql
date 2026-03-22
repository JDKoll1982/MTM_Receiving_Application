-- Stored Procedure: sp_Auth_User_UpdateDefaultDunnageMode
-- Purpose: Update user's default dunnage workflow mode preference
USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Auth_User_UpdateDefaultDunnageMode`;

DELIMITER //

CREATE PROCEDURE `sp_Auth_User_UpdateDefaultDunnageMode`(
    IN p_user_id INT,
    IN p_default_mode VARCHAR(20)
)
BEGIN
    UPDATE auth_users
    SET default_dunnage_mode = p_default_mode,
        modified_date = NOW()
    WHERE employee_number = p_user_id;

    SELECT ROW_COUNT() AS affected_rows;
END //

DELIMITER ;
