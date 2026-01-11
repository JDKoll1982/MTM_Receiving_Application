DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_routing_user_preference_save` $$

CREATE PROCEDURE `sp_routing_user_preference_save`(
    IN p_employee_number INT,
    IN p_default_mode VARCHAR(20),
    IN p_enable_validation TINYINT(1),
    OUT p_status INT,
    OUT p_error_msg VARCHAR(500)
)
sp_routing_user_preference_save: BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1 p_error_msg = MESSAGE_TEXT;
        SET p_status = -1;
        ROLLBACK;
    END;

    START TRANSACTION;

    IF p_employee_number IS NULL THEN
        SET p_status = -1;
        SET p_error_msg = 'Employee number is required';
        ROLLBACK;
        LEAVE sp_routing_user_preference_save;
    END IF;

    IF p_default_mode NOT IN ('WIZARD', 'MANUAL', 'EDIT') THEN
        SET p_status = -1;
        SET p_error_msg = 'Invalid default mode (must be WIZARD, MANUAL, or EDIT)';
        ROLLBACK;
        LEAVE sp_routing_user_preference_save;
    END IF;

    -- Insert or update preference
    INSERT INTO settings_routing_personal (employee_number, default_mode, enable_validation)
    VALUES (p_employee_number, p_default_mode, p_enable_validation)
    ON DUPLICATE KEY UPDATE
        default_mode = p_default_mode,
        enable_validation = p_enable_validation,
        updated_date = NOW();

    SET p_status = 1;
    SET p_error_msg = 'User preferences saved';

    COMMIT;
END $$

DELIMITER ;
