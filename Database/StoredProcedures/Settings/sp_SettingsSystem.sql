-- =============================================
-- Stored Procedures for Settings System
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Get All System Settings
-- =============================================
DROP PROCEDURE IF EXISTS sp_SystemSettings_GetAll$$
CREATE PROCEDURE sp_SystemSettings_GetAll()
BEGIN
    SELECT 
        id,
        category,
        sub_category,
        setting_key,
        setting_name,
        description,
        setting_value,
        default_value,
        data_type,
        scope,
        permission_level,
        is_locked,
        is_sensitive,
        validation_rules,
        ui_control_type,
        ui_order,
        created_at,
        updated_at,
        updated_by
    FROM system_settings
    ORDER BY category, ui_order, setting_name;
END$$

-- =============================================
-- SP: Get System Settings by Category
-- =============================================
DROP PROCEDURE IF EXISTS sp_SystemSettings_GetByCategory$$
CREATE PROCEDURE sp_SystemSettings_GetByCategory(
    IN p_category VARCHAR(50)
)
BEGIN
    SELECT 
        id,
        category,
        sub_category,
        setting_key,
        setting_name,
        description,
        setting_value,
        default_value,
        data_type,
        scope,
        permission_level,
        is_locked,
        is_sensitive,
        validation_rules,
        ui_control_type,
        ui_order,
        created_at,
        updated_at,
        updated_by
    FROM system_settings
    WHERE category = p_category
    ORDER BY ui_order, setting_name;
END$$

-- =============================================
-- SP: Get Single System Setting
-- =============================================
DROP PROCEDURE IF EXISTS sp_SystemSettings_GetByKey$$
CREATE PROCEDURE sp_SystemSettings_GetByKey(
    IN p_category VARCHAR(50),
    IN p_setting_key VARCHAR(100)
)
BEGIN
    SELECT 
        id,
        category,
        sub_category,
        setting_key,
        setting_name,
        description,
        setting_value,
        default_value,
        data_type,
        scope,
        permission_level,
        is_locked,
        is_sensitive,
        validation_rules,
        ui_control_type,
        ui_order,
        created_at,
        updated_at,
        updated_by
    FROM system_settings
    WHERE category = p_category 
      AND setting_key = p_setting_key;
END$$

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
    FROM system_settings 
    WHERE id = p_setting_id;
    
    IF v_is_locked THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Setting is locked and cannot be reset';
    END IF;
    
    -- Reset to default
    UPDATE system_settings
    SET setting_value = v_default_value,
        updated_at = CURRENT_TIMESTAMP,
        updated_by = p_changed_by
    WHERE id = p_setting_id;
    
    -- Log the reset
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
        v_default_value,
        'reset',
        p_changed_by,
        p_ip_address,
        p_workstation_name
    );
    
    SELECT 1 AS success;
END$$

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
    UPDATE system_settings
    SET is_locked = p_is_locked,
        updated_at = CURRENT_TIMESTAMP,
        updated_by = p_changed_by
    WHERE id = p_setting_id;
    
    -- Log the lock/unlock
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
        NULL,
        IF(p_is_locked, 'locked', 'unlocked'),
        IF(p_is_locked, 'lock', 'unlock'),
        p_changed_by,
        p_ip_address,
        p_workstation_name
    );
    
    SELECT 1 AS success;
END$$

-- =============================================
-- SP: Get User Setting (with fallback to system default)
-- =============================================
DROP PROCEDURE IF EXISTS sp_UserSettings_Get$$
CREATE PROCEDURE sp_UserSettings_Get(
    IN p_user_id INT,
    IN p_category VARCHAR(50),
    IN p_setting_key VARCHAR(100)
)
BEGIN
    SELECT 
        ss.id AS setting_id,
        ss.category,
        ss.setting_key,
        ss.setting_name,
        ss.description,
        COALESCE(us.setting_value, ss.setting_value) AS effective_value,
        ss.setting_value AS system_default,
        us.setting_value AS user_override,
        ss.data_type,
        ss.validation_rules,
        ss.ui_control_type,
        (us.id IS NOT NULL) AS has_override
    FROM system_settings ss
    LEFT JOIN user_settings us ON ss.id = us.setting_id AND us.user_id = p_user_id
    WHERE ss.category = p_category 
      AND ss.setting_key = p_setting_key
      AND ss.scope = 'user';
END$$

-- =============================================
-- SP: Get All User Settings for User
-- =============================================
DROP PROCEDURE IF EXISTS sp_UserSettings_GetAllForUser$$
CREATE PROCEDURE sp_UserSettings_GetAllForUser(
    IN p_user_id INT
)
BEGIN
    SELECT 
        ss.id AS setting_id,
        ss.category,
        ss.setting_key,
        ss.setting_name,
        ss.description,
        COALESCE(us.setting_value, ss.setting_value) AS effective_value,
        ss.setting_value AS system_default,
        us.setting_value AS user_override,
        ss.data_type,
        ss.validation_rules,
        ss.ui_control_type,
        ss.ui_order,
        (us.id IS NOT NULL) AS has_override
    FROM system_settings ss
    LEFT JOIN user_settings us ON ss.id = us.setting_id AND us.user_id = p_user_id
    WHERE ss.scope = 'user'
    ORDER BY ss.category, ss.ui_order, ss.setting_name;
END$$

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
    FROM user_settings 
    WHERE user_id = p_user_id 
      AND setting_id = p_setting_id;
    
    IF v_existing_id IS NULL THEN
        -- Insert new override
        INSERT INTO user_settings (user_id, setting_id, setting_value)
        VALUES (p_user_id, p_setting_id, p_setting_value);
        
        SET v_existing_id = LAST_INSERT_ID();
    ELSE
        -- Update existing override
        UPDATE user_settings
        SET setting_value = p_setting_value,
            updated_at = CURRENT_TIMESTAMP
        WHERE id = v_existing_id;
    END IF;
    
    -- Log the change
    INSERT INTO settings_audit_log (
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

-- =============================================
-- SP: Reset User Setting to System Default
-- =============================================
DROP PROCEDURE IF EXISTS sp_UserSettings_Reset$$
CREATE PROCEDURE sp_UserSettings_Reset(
    IN p_user_id INT,
    IN p_setting_id INT
)
BEGIN
    DECLARE v_user_setting_id INT DEFAULT NULL;
    DECLARE v_old_value TEXT DEFAULT NULL;
    
    -- Get the user override if it exists
    SELECT id, setting_value 
    INTO v_user_setting_id, v_old_value
    FROM user_settings 
    WHERE user_id = p_user_id 
      AND setting_id = p_setting_id;
    
    IF v_user_setting_id IS NOT NULL THEN
        -- Delete the override
        DELETE FROM user_settings 
        WHERE id = v_user_setting_id;
        
        -- Log the reset
        INSERT INTO settings_audit_log (
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

-- =============================================
-- SP: Reset All User Settings for User
-- =============================================
DROP PROCEDURE IF EXISTS sp_UserSettings_ResetAll$$
CREATE PROCEDURE sp_UserSettings_ResetAll(
    IN p_user_id INT,
    IN p_changed_by INT
)
BEGIN
    -- Log all resets before deletion
    INSERT INTO settings_audit_log (
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
    FROM user_settings us
    WHERE us.user_id = p_user_id;
    
    -- Delete all user overrides
    DELETE FROM user_settings 
    WHERE user_id = p_user_id;
    
    SELECT ROW_COUNT() AS reset_count;
END$$

-- =============================================
-- SP: Get Package Type Mappings
-- =============================================
DROP PROCEDURE IF EXISTS sp_PackageTypeMappings_GetAll$$
CREATE PROCEDURE sp_PackageTypeMappings_GetAll()
BEGIN
    SELECT 
        id,
        part_prefix,
        package_type,
        is_default,
        display_order,
        is_active,
        created_at,
        updated_at,
        created_by
    FROM package_type_mappings
    WHERE is_active = TRUE
    ORDER BY display_order, part_prefix;
END$$

-- =============================================
-- SP: Get Package Type for Part Prefix
-- =============================================
DROP PROCEDURE IF EXISTS sp_PackageTypeMappings_GetByPrefix$$
CREATE PROCEDURE sp_PackageTypeMappings_GetByPrefix(
    IN p_part_prefix VARCHAR(10)
)
BEGIN
    DECLARE v_package_type VARCHAR(50);
    
    -- Try exact match first
    SELECT package_type 
    INTO v_package_type
    FROM package_type_mappings
    WHERE part_prefix = p_part_prefix
      AND is_active = TRUE
    LIMIT 1;
    
    -- If no match, get default
    IF v_package_type IS NULL THEN
        SELECT package_type 
        INTO v_package_type
        FROM package_type_mappings
        WHERE is_default = TRUE
          AND is_active = TRUE
        LIMIT 1;
    END IF;
    
    SELECT v_package_type AS package_type;
END$$

-- =============================================
-- SP: Insert Package Type Mapping
-- =============================================
DROP PROCEDURE IF EXISTS sp_PackageTypeMappings_Insert$$
CREATE PROCEDURE sp_PackageTypeMappings_Insert(
    IN p_part_prefix VARCHAR(10),
    IN p_package_type VARCHAR(50),
    IN p_is_default BOOLEAN,
    IN p_display_order INT,
    IN p_created_by INT
)
BEGIN
    INSERT INTO package_type_mappings (
        part_prefix,
        package_type,
        is_default,
        display_order,
        is_active,
        created_by
    ) VALUES (
        p_part_prefix,
        p_package_type,
        p_is_default,
        p_display_order,
        TRUE,
        p_created_by
    );
    
    SELECT LAST_INSERT_ID() AS id;
END$$

-- =============================================
-- SP: Update Package Type Mapping
-- =============================================
DROP PROCEDURE IF EXISTS sp_PackageTypeMappings_Update$$
CREATE PROCEDURE sp_PackageTypeMappings_Update(
    IN p_id INT,
    IN p_package_type VARCHAR(50),
    IN p_is_default BOOLEAN,
    IN p_display_order INT
)
BEGIN
    UPDATE package_type_mappings
    SET package_type = p_package_type,
        is_default = p_is_default,
        display_order = p_display_order,
        updated_at = CURRENT_TIMESTAMP
    WHERE id = p_id;
    
    SELECT ROW_COUNT() AS affected_rows;
END$$

-- =============================================
-- SP: Delete Package Type Mapping (soft delete)
-- =============================================
DROP PROCEDURE IF EXISTS sp_PackageTypeMappings_Delete$$
CREATE PROCEDURE sp_PackageTypeMappings_Delete(
    IN p_id INT
)
BEGIN
    UPDATE package_type_mappings
    SET is_active = FALSE,
        updated_at = CURRENT_TIMESTAMP
    WHERE id = p_id;
    
    SELECT ROW_COUNT() AS affected_rows;
END$$

-- =============================================
-- SP: Get Settings Audit Log
-- =============================================
DROP PROCEDURE IF EXISTS sp_SettingsAuditLog_Get$$
CREATE PROCEDURE sp_SettingsAuditLog_Get(
    IN p_setting_id INT,
    IN p_limit INT
)
BEGIN
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
    FROM settings_audit_log sal
    INNER JOIN system_settings ss ON sal.setting_id = ss.id
    WHERE sal.setting_id = p_setting_id
    ORDER BY sal.changed_at DESC
    LIMIT p_limit;
END$$

DELIMITER ;
