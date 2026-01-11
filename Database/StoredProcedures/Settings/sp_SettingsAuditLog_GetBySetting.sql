-- =============================================
-- Stored Procedure: sp_SettingsAuditLog_GetBySetting
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Get Audit Log Entries for a Specific Setting
-- Spec compatibility wrapper.
-- =============================================
DROP PROCEDURE IF EXISTS sp_SettingsAuditLog_GetBySetting$$
CREATE PROCEDURE sp_SettingsAuditLog_GetBySetting(
    IN p_setting_id INT,
    IN p_limit INT
)
BEGIN
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
    FROM settings_audit_log sal
    INNER JOIN system_settings ss ON sal.setting_id = ss.id
    WHERE sal.setting_id = p_setting_id
    ORDER BY sal.changed_at DESC
    LIMIT p_limit;
END$$



DELIMITER ;
