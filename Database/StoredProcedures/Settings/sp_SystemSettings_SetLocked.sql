-- =============================================
-- Stored Procedure: sp_SystemSettings_SetLocked
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Lock/Unlock Setting
-- =============================================
DROP PROCEDURE IF EXISTS sp_SystemSettings_SetLocked$$
CREATE PROCEDURE sp_SystemSettings_SetLocked(
    IN p_setting_id INT,
    IN p_is_locked BOOLEAN,
    IN p_changed_by INT,
    IN p_ip_address VARCHAR(45),
    IN p_workstation_name VARCHAR(100)
)
BEGIN
    UPDATE settings_universal
    SET is_locked = p_is_locked,
        updated_at = CURRENT_TIMESTAMP,
        updated_by = p_changed_by
    WHERE id = p_setting_id;

    -- Log the lock/unlock
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
        NULL,
        IF(p_is_locked, 'locked', 'unlocked'),
        IF(p_is_locked, 'lock', 'unlock'),
        p_changed_by,
        p_ip_address,
        p_workstation_name
    );

    SELECT 1 AS success;
END$$



DELIMITER ;
