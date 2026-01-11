-- =============================================
-- Stored Procedure: sp_UserSettings_ResetAll
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Reset All User Settings for User
-- =============================================
DROP PROCEDURE IF EXISTS sp_UserSettings_ResetAll$$
CREATE PROCEDURE sp_UserSettings_ResetAll(
    IN p_user_id INT,
    IN p_changed_by INT
)
BEGIN
    -- Log all resets before deletion
    INSERT INTO settings_audit_log (
        setting_id,
        user_setting_id,
        old_value,
        new_value,
        change_type,
        changed_by,
        changed_at
    )
    SELECT 
        us.setting_id,
        us.id,
        us.setting_value,
        NULL,
        'reset',
        p_changed_by,
        CURRENT_TIMESTAMP
    FROM user_settings us
    WHERE us.user_id = p_user_id;
    
    -- Delete all user overrides
    DELETE FROM user_settings 
    WHERE user_id = p_user_id;
    
    SELECT ROW_COUNT() AS reset_count;
END$$



DELIMITER ;
