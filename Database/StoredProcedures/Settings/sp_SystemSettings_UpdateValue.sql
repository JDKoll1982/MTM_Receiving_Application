-- =============================================
-- Stored Procedure: sp_SystemSettings_UpdateValue
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Update System Setting Value
-- =============================================
DROP PROCEDURE IF EXISTS sp_SystemSettings_UpdateValue$$
CREATE PROCEDURE sp_SystemSettings_UpdateValue(
    IN p_setting_id INT,
    IN p_new_value TEXT,
    IN p_changed_by INT,
    IN p_ip_address VARCHAR(45),
    IN p_workstation_name VARCHAR(100)
)
BEGIN
    DECLARE v_old_value TEXT;
    DECLARE v_is_locked BOOLEAN;
    
    -- Check if setting is locked
    SELECT setting_value, is_locked 
    INTO v_old_value, v_is_locked
    FROM system_settings 
    WHERE id = p_setting_id;
    
    IF v_is_locked THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Setting is locked and cannot be modified';
    END IF;
    
    -- Update the setting
    UPDATE system_settings
    SET setting_value = p_new_value,
        updated_at = CURRENT_TIMESTAMP,
        updated_by = p_changed_by
    WHERE id = p_setting_id;
    
    -- Log the change
    INSERT INTO settings_audit_log (
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
