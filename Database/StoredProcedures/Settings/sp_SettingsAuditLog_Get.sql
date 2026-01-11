-- =============================================
-- Stored Procedure: sp_SettingsAuditLog_Get
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Get Settings Audit Log
-- =============================================
DROP PROCEDURE IF EXISTS sp_SettingsAuditLog_Get$$
CREATE PROCEDURE sp_SettingsAuditLog_Get(
    IN p_setting_id INT,
    IN p_limit INT
)
BEGIN
    -- Allow p_setting_id to be optional (NULL = all settings)
    -- and handle NULL/zero p_limit by omitting the LIMIT clause.
    IF p_limit IS NULL OR p_limit <= 0 THEN
        SELECT
            sal.id,
            sal.setting_id,
            ss.category,
            ss.setting_key,
            ss.setting_name,
            sal.user_setting_id,
            sal.old_value,
            sal.new_value,
            sal.change_type,
            sal.changed_by,
            sal.changed_at,
            sal.ip_address,
            sal.workstation_name
        FROM settings_activity sal
        INNER JOIN settings_universal ss ON sal.setting_id = ss.id
        WHERE (p_setting_id IS NULL OR sal.setting_id = p_setting_id)
        ORDER BY sal.changed_at DESC;
    ELSE
        SELECT
            sal.id,
            sal.setting_id,
            ss.category,
            ss.setting_key,
            ss.setting_name,
            sal.user_setting_id,
            sal.old_value,
            sal.new_value,
            sal.change_type,
            sal.changed_by,
            sal.changed_at,
            sal.ip_address,
            sal.workstation_name
        FROM settings_activity sal
        INNER JOIN settings_universal ss ON sal.setting_id = ss.id
        WHERE (p_setting_id IS NULL OR sal.setting_id = p_setting_id)
        ORDER BY sal.changed_at DESC
        LIMIT p_limit;
    END IF;
END$$

-- =============================================
-- PACKAGE TYPES CRUD OPERATIONS
-- =============================================



DELIMITER ;
