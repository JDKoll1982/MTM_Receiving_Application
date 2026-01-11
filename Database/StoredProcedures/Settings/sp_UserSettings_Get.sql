-- =============================================
-- Stored Procedure: sp_UserSettings_Get
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Get User Setting (with fallback to system default)
-- =============================================
DROP PROCEDURE IF EXISTS sp_UserSettings_Get$$
CREATE PROCEDURE sp_UserSettings_Get(
    IN p_user_id INT,
    IN p_category VARCHAR(50),
    IN p_setting_key VARCHAR(100)
)
BEGIN
    SELECT
        ss.id AS setting_id,
        ss.category,
        ss.setting_key,
        ss.setting_name,
        ss.description,
        COALESCE(us.setting_value, ss.setting_value, ss.default_value) AS effective_value,
        ss.default_value AS system_default,
        us.setting_value AS user_override,
        ss.data_type,
        ss.validation_rules,
        ss.ui_control_type,
        (us.id IS NOT NULL) AS has_override
    FROM settings_universal ss
    LEFT JOIN settings_personal us ON ss.id = us.setting_id AND us.user_id = p_user_id
    WHERE ss.category = p_category
      AND ss.setting_key = p_setting_key
      AND ss.scope = 'user';
END$$



DELIMITER ;
