-- Fixed for MySQL 5.7 compatibility
-- Reads previous state and logs old/new values; uses TINYINT(1) for boolean param
DROP PROCEDURE IF EXISTS `sp_Settings_System_SetLocked`;
DELIMITER $$

CREATE PROCEDURE `sp_Settings_System_SetLocked`(
    IN p_setting_id INT,
    IN p_is_locked TINYINT(1),
    IN p_changed_by INT,
    IN p_ip_address VARCHAR(45),
    IN p_workstation_name VARCHAR(100)
)
proc_main: BEGIN
    DECLARE v_old_value VARCHAR(10) DEFAULT NULL;

    START TRANSACTION;

    -- lock row and get previous value
    SELECT CASE WHEN is_locked THEN 'locked' ELSE 'unlocked' END
      INTO v_old_value
      FROM settings_universal
     WHERE id = p_setting_id
     FOR UPDATE;

    IF v_old_value IS NULL THEN
        ROLLBACK;
        SELECT 0 AS success, 'Setting not found' AS message;
        LEAVE proc_main;
    END IF;

    UPDATE settings_universal
    SET is_locked = p_is_locked,
        updated_at = CURRENT_TIMESTAMP,
        updated_by = p_changed_by
    WHERE id = p_setting_id;

    -- Log the lock/unlock (old_value captured above)
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
        CASE WHEN p_is_locked THEN 'locked' ELSE 'unlocked' END,
        CASE WHEN p_is_locked THEN 'lock' ELSE 'unlock' END,
        p_changed_by,
        p_ip_address,
        p_workstation_name
    );

    COMMIT;

    SELECT 1 AS success, 'OK' AS message;
END proc_main$$

DELIMITER ;
