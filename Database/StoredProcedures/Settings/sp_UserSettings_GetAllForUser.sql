-- =============================================
-- Stored Procedure: sp_UserSettings_GetAllForUser
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Get All User Settings for User
-- =============================================
DROP PROCEDURE IF EXISTS sp_UserSettings_GetAllForUser$$
CREATE PROCEDURE sp_UserSettings_GetAllForUser(
    IN p_user_id INT
)
BEGIN
    SELECT 
        ss.id AS setting_id,
        ss.category,
        ss.setting_key,
        ss.setting_name,
        ss.description,
        COALESCE(us.setting_value, ss.setting_value) AS effective_value,
        ss.setting_value AS system_default,
        us.setting_value AS user_override,
        ss.data_type,
        ss.validation_rules,
        ss.ui_control_type,
        ss.ui_order,
        (us.id IS NOT NULL) AS has_override
    FROM system_settings ss
    LEFT JOIN user_settings us ON ss.id = us.setting_id AND us.user_id = p_user_id
    WHERE ss.scope = 'user'
    ORDER BY ss.category, ss.ui_order, ss.setting_name;
END$$



DELIMITER ;
