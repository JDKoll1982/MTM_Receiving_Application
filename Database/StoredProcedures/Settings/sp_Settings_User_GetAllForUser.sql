-- =============================================
-- Stored Procedure: sp_Settings_User_GetAllForUser
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Get All User Settings for User
-- MySQL 5.7 compatible: use IFNULL, cast JSON to CHAR, and explicit numeric flag
-- =============================================
DROP PROCEDURE IF EXISTS `sp_Settings_User_GetAllForUser`$$
CREATE PROCEDURE `sp_Settings_User_GetAllForUser`(
    IN p_user_id INT
)
BEGIN
    SELECT
        ss.id AS setting_id,
        ss.category,
        ss.setting_key,
        ss.setting_name,
        ss.description,
        IFNULL(us.setting_value, ss.setting_value) AS effective_value,
        ss.setting_value AS system_default,
        us.setting_value AS user_override,
        ss.data_type,
        CAST(ss.validation_rules AS CHAR) AS validation_rules,
        ss.ui_control_type,
        ss.ui_order,
        IF(us.id IS NOT NULL, 1, 0) AS has_override
    FROM settings_universal ss
    LEFT JOIN settings_personal us ON ss.id = us.setting_id AND us.user_id = p_user_id
    WHERE ss.scope = 'user'
    ORDER BY ss.category, ss.ui_order, ss.setting_name;
END$$



DELIMITER ;
