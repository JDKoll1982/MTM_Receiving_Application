-- =============================================
-- Stored Procedure: sp_Settings_System_UpdateValue
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Update System Setting Value
-- =============================================
DROP PROCEDURE IF EXISTS `sp_Settings_System_UpdateValue`$$
CREATE PROCEDURE `sp_Settings_System_UpdateValue`(
    IN p_setting_id INT,
    IN p_new_value TEXT,
    IN p_changed_by INT,
    IN p_ip_address VARCHAR(45),
    IN p_workstation_name VARCHAR(100)
)
BEGIN
    DECLARE v_old_value TEXT;
    DECLARE v_is_locked TINYINT(1) DEFAULT 0;

    -- Retrieve current value and lock state; signal if the setting does not exist
    SELECT setting_value, is_locked
    INTO v_old_value, v_is_locked
    FROM settings_universal
    WHERE id = p_setting_id;

    IF ROW_COUNT() = 0 THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Setting not found';
    END IF;

    -- Respect lock (treat NULL as unlocked)
    IF COALESCE(v_is_locked, 0) = 1 THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Setting is locked and cannot be modified';
    END IF;

    -- Update the setting
    UPDATE settings_universal
    SET setting_value = p_new_value,
        updated_at = CURRENT_TIMESTAMP,
        updated_by = p_changed_by
    WHERE id = p_setting_id;

    -- Log the change
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
        p_new_value,
        'update',
        p_changed_by,
        p_ip_address,
        p_workstation_name
    );

    SELECT 1 AS success;
END$$



DELIMITER ;
