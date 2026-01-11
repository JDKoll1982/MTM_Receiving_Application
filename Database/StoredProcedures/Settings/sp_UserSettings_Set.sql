-- =============================================
-- Stored Procedure: sp_UserSettings_Set
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Set User Setting Override
-- =============================================
DROP PROCEDURE IF EXISTS sp_UserSettings_Set$$
CREATE PROCEDURE sp_UserSettings_Set(
    IN p_user_id INT,
    IN p_setting_id INT,
    IN p_setting_value TEXT
)
BEGIN
    DECLARE v_existing_id INT DEFAULT NULL;
    DECLARE v_old_value TEXT DEFAULT NULL;

    -- Check if override already exists
    SELECT id, setting_value
    INTO v_existing_id, v_old_value
    FROM settings_personal
    WHERE user_id = p_user_id
      AND setting_id = p_setting_id;

    IF v_existing_id IS NULL THEN
        -- Insert new override
        INSERT INTO settings_personal (user_id, setting_id, setting_value)
        VALUES (p_user_id, p_setting_id, p_setting_value);

        SET v_existing_id = LAST_INSERT_ID();
    ELSE
        -- Update existing override
        UPDATE settings_personal
        SET setting_value = p_setting_value,
            updated_at = CURRENT_TIMESTAMP
        WHERE id = v_existing_id;
    END IF;

    -- Log the change
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
        v_existing_id,
        v_old_value,
        p_setting_value,
        IF(v_old_value IS NULL, 'create', 'update'),
        p_user_id,
        CURRENT_TIMESTAMP
    );

    SELECT 1 AS success, v_existing_id AS user_setting_id;
END$$



DELIMITER ;
