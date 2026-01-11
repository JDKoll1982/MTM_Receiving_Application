-- =============================================
-- Stored Procedure: sp_Settings_System_ResetToDefault
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Reset Setting to Default
-- =============================================
DROP PROCEDURE IF EXISTS `sp_Settings_System_ResetToDefault`$$
CREATE PROCEDURE `sp_Settings_System_ResetToDefault`(
    IN p_setting_id INT,
    IN p_changed_by INT,
    IN p_ip_address VARCHAR(45),
    IN p_workstation_name VARCHAR(100)
)
BEGIN
    -- Use VARCHAR for routine variables (TEXT/BLOB types are not allowed for DECLARE in some MySQL versions)
    DECLARE v_old_value VARCHAR(4000);
    DECLARE v_default_value VARCHAR(4000);
    DECLARE v_is_locked TINYINT(1);
    DECLARE v_exists INT DEFAULT 0;

    -- Ensure the setting exists to avoid "SELECT ... INTO" returning no rows
    SELECT COUNT(*) INTO v_exists FROM settings_universal WHERE id = p_setting_id;
    IF v_exists = 0 THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Setting not found';
    END IF;

    -- Get current and default values
    SELECT setting_value, default_value, is_locked
    INTO v_old_value, v_default_value, v_is_locked
    FROM settings_universal
    WHERE id = p_setting_id
    LIMIT 1;

    -- Treat NULL as unlocked; use COALESCE to be explicit
    IF COALESCE(v_is_locked, 0) = 1 THEN
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
