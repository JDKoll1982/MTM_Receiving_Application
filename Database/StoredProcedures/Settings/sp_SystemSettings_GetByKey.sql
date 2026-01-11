-- =============================================
-- Stored Procedure: sp_SystemSettings_GetByKey
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Get Single System Setting
-- =============================================
DROP PROCEDURE IF EXISTS sp_SystemSettings_GetByKey$$
CREATE PROCEDURE sp_SystemSettings_GetByKey(
    IN p_category VARCHAR(50),
    IN p_setting_key VARCHAR(100)
)
BEGIN
    SELECT 
        id,
        category,
        sub_category,
        setting_key,
        setting_name,
        description,
        setting_value,
        default_value,
        data_type,
        scope,
        permission_level,
        is_locked,
        is_sensitive,
        validation_rules,
        ui_control_type,
        ui_order,
        created_at,
        updated_at,
        updated_by
    FROM system_settings
    WHERE category = p_category 
      AND setting_key = p_setting_key;
END$$



DELIMITER ;
