-- =============================================
-- Stored Procedure: sp_Settings_User_ResetAll
-- =============================================

-- Ensure DROP uses the standard delimiter so clients (MySQL 5.7) parse correctly
DROP PROCEDURE IF EXISTS `sp_Settings_User_ResetAll`;

DELIMITER $$

-- =============================================
-- SP: Reset All User Settings for User
-- =============================================
CREATE PROCEDURE `sp_Settings_User_ResetAll`(
    IN p_user_id INT,
    IN p_changed_by INT
)
BEGIN
    -- Log all resets before deletion
    INSERT INTO settings_activity (
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
    FROM settings_personal us
    WHERE us.user_id = p_user_id;

    -- Delete all user overrides
    DELETE FROM settings_personal
    WHERE user_id = p_user_id;

    SELECT ROW_COUNT() AS reset_count;
END$$

DELIMITER ;
