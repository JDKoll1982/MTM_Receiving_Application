-- =============================================
-- Stored Procedure: sp_Settings_AuditLog_GetByUser
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Get Audit Log Entries by User
-- Spec compatibility wrapper.
-- =============================================
DROP PROCEDURE IF EXISTS `sp_Settings_AuditLog_GetByUser`$$
CREATE PROCEDURE `sp_Settings_AuditLog_GetByUser`(
    IN p_changed_by INT,
    IN p_limit INT
)
BEGIN
    -- MySQL 5.7 may not allow a routine parameter directly in LIMIT,
    -- so use a prepared statement when a positive limit is provided.
    IF p_limit IS NULL OR p_limit <= 0 THEN
        SELECT
            sal.id,
            sal.setting_id,
            ss.category,
            ss.setting_key,
            ss.setting_name,
            sal.user_setting_id,
            sal.old_value,
            sal.new_value,
            sal.change_type,
            sal.changed_by,
            sal.changed_at,
            sal.ip_address,
            sal.workstation_name
        FROM settings_activity sal
        INNER JOIN settings_universal ss ON sal.setting_id = ss.id
        WHERE sal.changed_by = p_changed_by
        ORDER BY sal.changed_at DESC;
    ELSE
        SET @sql = CONCAT(
            'SELECT
                sal.id,
                sal.setting_id,
                ss.category,
                ss.setting_key,
                ss.setting_name,
                sal.user_setting_id,
                sal.old_value,
                sal.new_value,
                sal.change_type,
                sal.changed_by,
                sal.changed_at,
                sal.ip_address,
                sal.workstation_name
            FROM settings_activity sal
            INNER JOIN settings_universal ss ON sal.setting_id = ss.id
            WHERE sal.changed_by = ? 
            ORDER BY sal.changed_at DESC
            LIMIT ',
            p_limit
        );
        PREPARE stmt FROM @sql;
        SET @p_changed_by = p_changed_by;
        EXECUTE stmt USING @p_changed_by;
        DEALLOCATE PREPARE stmt;
    END IF;
END$$



DELIMITER ;
