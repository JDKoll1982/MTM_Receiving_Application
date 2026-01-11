-- =============================================
-- Stored Procedure: sp_Settings_User_Reset
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Reset User Setting to System Default
-- =============================================
DROP PROCEDURE IF EXISTS `sp_Settings_User_Reset`$$
CREATE PROCEDURE `sp_Settings_User_Reset`(
    IN p_user_id INT,
    IN p_setting_id INT
)
BEGIN
    DECLARE v_user_setting_id INT DEFAULT NULL;
    DECLARE v_old_value TEXT DEFAULT NULL;

    -- Get the user override if it exists (limit to one row to avoid multiple-row errors)
    SELECT id, setting_value
    INTO v_user_setting_id, v_old_value
    FROM settings_personal
    WHERE user_id = p_user_id
      AND setting_id = p_setting_id
    LIMIT 1;

    IF v_user_setting_id IS NOT NULL THEN
        -- Delete the override
        DELETE FROM settings_personal
        WHERE id = v_user_setting_id;

        -- Log the reset
        INSERT INTO settings_activity (
            setting_id,
            user_setting_id,
            old_value,
            new_value,
            change_type,
            changed_by,
            changed_at
        ) VALUES (
            p_setting_id,
            v_user_setting_id,
            v_old_value,
            NULL,
            'reset',
            p_user_id,
            CURRENT_TIMESTAMP
        );
    END IF;

    SELECT 1 AS success;
END$$



DELIMITER ;
