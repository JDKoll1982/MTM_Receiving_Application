-- =============================================
-- Stored Procedure: sp_Settings_System_GetAll
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Get All System Settings
-- =============================================
DROP PROCEDURE IF EXISTS `sp_Settings_System_GetAll`$$
CREATE PROCEDURE `sp_Settings_System_GetAll`()
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
        CAST(validation_rules AS CHAR) AS validation_rules,
        ui_control_type,
        ui_order,
        created_at,
        updated_at,
        updated_by
    FROM settings_universal
    ORDER BY category, ui_order, setting_name;
END$$

DELIMITER ;
