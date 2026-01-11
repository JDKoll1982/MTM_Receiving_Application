-- =============================================
-- Stored Procedure: sp_SystemSettings_GetByCategory
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Get System Settings by Category
-- =============================================
DROP PROCEDURE IF EXISTS `sp_SystemSettings_GetByCategory` $$
CREATE PROCEDURE `sp_SystemSettings_GetByCategory`(
    IN p_category VARCHAR(50)
)
BEGIN
    SELECT
        `id`,
        `category`,
        `sub_category`,
        `setting_key`,
        `setting_name`,
        `description`,
        `setting_value`,
        `default_value`,
        `data_type`,
        `scope`,
        `permission_level`,
        `is_locked`,
        `is_sensitive`,
        `validation_rules`,
        `ui_control_type`,
        `ui_order`,
        `created_at`,
        `updated_at`,
        `updated_by`
    FROM `settings_universal`
    WHERE `category` = p_category
    ORDER BY `ui_order`, `setting_name`;
END $$


DELIMITER ;
