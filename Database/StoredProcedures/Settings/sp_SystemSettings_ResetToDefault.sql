-- =============================================
-- Stored Procedure: sp_SystemSettings_ResetToDefault
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Reset Setting to Default
-- =============================================
DROP PROCEDURE IF EXISTS sp_SystemSettings_ResetToDefault$$
CREATE PROCEDURE sp_SystemSettings_ResetToDefault(
    IN p_setting_id INT,
    IN p_changed_by INT,
    IN p_ip_address VARCHAR(45),
    IN p_workstation_name VARCHAR(100)
)
BEGIN
    DECLARE v_old_value TEXT;
    DECLARE v_default_value TEXT;
    DECLARE v_is_locked BOOLEAN;

    -- Get current and default values
    SELECT setting_value, default_value, is_locked
    INTO v_old_value, v_default_value, v_is_locked
    FROM settings_universal
    WHERE id = p_setting_id;

    IF v_is_locked THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Setting is locked and cannot be reset';
    END IF;

    -- Reset to default
    UPDATE settings_universal
    SET setting_value = v_default_value,
        updated_at = CURRENT_TIMESTAMP,
        updated_by = p_changed_by
    WHERE id = p_setting_id;

    -- Log the reset
    INSERT INTO settings_activity (
        setting_id,
        old_value,
        new_value,
        change_type,
        changed_by,
        ip_address,
        workstation_name
    ) VALUES (
        p_setting_id,
        v_old_value,
        v_default_value,
        'reset',
        p_changed_by,
        p_ip_address,
        p_workstation_name
    );

    SELECT 1 AS success;
END$$



DELIMITER ;
