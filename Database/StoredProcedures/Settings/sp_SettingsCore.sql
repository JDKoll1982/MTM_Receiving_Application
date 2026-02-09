-- Core Settings stored procedures

DELIMITER $$

DROP PROCEDURE IF EXISTS sp_SettingsCore_System_GetByKey $$
CREATE PROCEDURE sp_SettingsCore_System_GetByKey(
    IN p_category VARCHAR(100),
    IN p_key VARCHAR(150)
)
BEGIN
    SELECT *
    FROM settings_universal
    WHERE category = p_category AND setting_key = p_key
    LIMIT 1;
END $$

DROP PROCEDURE IF EXISTS sp_SettingsCore_System_GetByCategory $$
CREATE PROCEDURE sp_SettingsCore_System_GetByCategory(
    IN p_category VARCHAR(100)
)
BEGIN
    SELECT *
    FROM settings_universal
    WHERE category = p_category;
END $$

DROP PROCEDURE IF EXISTS sp_SettingsCore_System_Upsert $$
CREATE PROCEDURE sp_SettingsCore_System_Upsert(
    IN p_category VARCHAR(100),
    IN p_key VARCHAR(150),
    IN p_value TEXT,
    IN p_data_type VARCHAR(50),
    IN p_is_sensitive TINYINT(1),
    IN p_updated_by VARCHAR(100)
)
BEGIN
    INSERT INTO settings_universal (
        category, setting_key, setting_value, data_type, is_sensitive, is_locked, updated_by, updated_at
    ) VALUES (
        p_category, p_key, p_value, p_data_type, p_is_sensitive, 0, p_updated_by, NOW()
    )
    ON DUPLICATE KEY UPDATE
        setting_value = VALUES(setting_value),
        data_type = VALUES(data_type),
        is_sensitive = VALUES(is_sensitive),
        updated_by = VALUES(updated_by),
        updated_at = NOW();
END $$

DROP PROCEDURE IF EXISTS sp_SettingsCore_System_Reset $$
CREATE PROCEDURE sp_SettingsCore_System_Reset(
    IN p_category VARCHAR(100),
    IN p_key VARCHAR(150),
    IN p_updated_by VARCHAR(100)
)
BEGIN
    UPDATE settings_universal
    SET updated_by = p_updated_by,
        updated_at = NOW()
    WHERE category = p_category AND setting_key = p_key;
END $$

DROP PROCEDURE IF EXISTS sp_SettingsCore_User_GetByKey $$
CREATE PROCEDURE sp_SettingsCore_User_GetByKey(
    IN p_user_id INT,
    IN p_category VARCHAR(100),
    IN p_key VARCHAR(150)
)
BEGIN
    SELECT *
    FROM settings_personal
    WHERE user_id = p_user_id AND category = p_category AND setting_key = p_key
    LIMIT 1;
END $$

DROP PROCEDURE IF EXISTS sp_SettingsCore_User_GetByCategory $$
CREATE PROCEDURE sp_SettingsCore_User_GetByCategory(
    IN p_user_id INT,
    IN p_category VARCHAR(100)
)
BEGIN
    SELECT *
    FROM settings_personal
    WHERE user_id = p_user_id AND category = p_category;
END $$

DROP PROCEDURE IF EXISTS sp_SettingsCore_User_Upsert $$
CREATE PROCEDURE sp_SettingsCore_User_Upsert(
    IN p_user_id INT,
    IN p_category VARCHAR(100),
    IN p_key VARCHAR(150),
    IN p_value TEXT,
    IN p_data_type VARCHAR(50),
    IN p_updated_by VARCHAR(100)
)
BEGIN
    INSERT INTO settings_personal (
        user_id, category, setting_key, setting_value, data_type, updated_by, updated_at
    ) VALUES (
        p_user_id, p_category, p_key, p_value, p_data_type, p_updated_by, NOW()
    )
    ON DUPLICATE KEY UPDATE
        setting_value = VALUES(setting_value),
        data_type = VALUES(data_type),
        updated_by = VALUES(updated_by),
        updated_at = NOW();
END $$

DROP PROCEDURE IF EXISTS sp_SettingsCore_User_Reset $$
CREATE PROCEDURE sp_SettingsCore_User_Reset(
    IN p_user_id INT,
    IN p_category VARCHAR(100),
    IN p_key VARCHAR(150),
    IN p_updated_by VARCHAR(100)
)
BEGIN
    UPDATE settings_personal
    SET updated_by = p_updated_by,
        updated_at = NOW()
    WHERE user_id = p_user_id AND category = p_category AND setting_key = p_key;
END $$

DROP PROCEDURE IF EXISTS sp_SettingsCore_Audit_Insert $$
CREATE PROCEDURE sp_SettingsCore_Audit_Insert(
    IN p_scope VARCHAR(20),
    IN p_category VARCHAR(100),
    IN p_setting_key VARCHAR(150),
    IN p_old_value TEXT,
    IN p_new_value TEXT,
    IN p_change_type VARCHAR(50),
    IN p_user_id INT,
    IN p_changed_by VARCHAR(100),
    IN p_ip_address VARCHAR(50),
    IN p_workstation VARCHAR(100)
)
BEGIN
    INSERT INTO settings_activity (
        scope, category, setting_key, old_value, new_value, change_type, user_id, changed_by, changed_at, ip_address, workstation
    ) VALUES (
        p_scope, p_category, p_setting_key, p_old_value, p_new_value, p_change_type, p_user_id, p_changed_by, NOW(), p_ip_address, p_workstation
    );
END $$

DROP PROCEDURE IF EXISTS sp_SettingsCore_Audit_GetBySetting $$
CREATE PROCEDURE sp_SettingsCore_Audit_GetBySetting(
    IN p_category VARCHAR(100),
    IN p_setting_key VARCHAR(150)
)
BEGIN
    SELECT *
    FROM settings_activity
    WHERE category = p_category AND setting_key = p_setting_key
    ORDER BY changed_at DESC
    LIMIT 100;
END $$

DROP PROCEDURE IF EXISTS sp_SettingsCore_Audit_GetByUser $$
CREATE PROCEDURE sp_SettingsCore_Audit_GetByUser(
    IN p_user_id INT
)
BEGIN
    SELECT *
    FROM settings_activity
    WHERE user_id = p_user_id
    ORDER BY changed_at DESC
    LIMIT 100;
END $$

DROP PROCEDURE IF EXISTS sp_SettingsCore_Roles_GetAll $$
CREATE PROCEDURE sp_SettingsCore_Roles_GetAll()
BEGIN
    SELECT * FROM settings_roles ORDER BY role_name;
END $$

DROP PROCEDURE IF EXISTS sp_SettingsCore_UserRoles_GetByUser $$
CREATE PROCEDURE sp_SettingsCore_UserRoles_GetByUser(
    IN p_user_id INT
)
BEGIN
    SELECT * FROM settings_user_roles WHERE user_id = p_user_id;
END $$

DROP PROCEDURE IF EXISTS sp_SettingsCore_Meta_GetTables $$
CREATE PROCEDURE sp_SettingsCore_Meta_GetTables()
BEGIN
    SELECT table_name
    FROM information_schema.tables
    WHERE table_schema = DATABASE()
      AND table_name IN (
        'settings_universal',
        'settings_personal',
        'settings_activity',
        'settings_roles',
        'settings_user_roles'
      );
END $$

DROP PROCEDURE IF EXISTS sp_SettingsCore_Meta_GetStoredProcedures $$
CREATE PROCEDURE sp_SettingsCore_Meta_GetStoredProcedures()
BEGIN
    SELECT routine_name AS procedure_name
    FROM information_schema.routines
    WHERE routine_schema = DATABASE()
      AND routine_type = 'PROCEDURE'
      AND routine_name LIKE 'sp_SettingsCore_%';
END $$

DROP PROCEDURE IF EXISTS sp_SettingsCore_Roles_GetByName $$
CREATE PROCEDURE sp_SettingsCore_Roles_GetByName(
    IN p_role_name VARCHAR(100)
)
BEGIN
    SELECT 
        id,
        role_name,
        description,
        created_at
    FROM settings_roles
    WHERE role_name = p_role_name
    LIMIT 1;
END $$

DROP PROCEDURE IF EXISTS sp_SettingsCore_UserRoles_Assign $$
CREATE PROCEDURE sp_SettingsCore_UserRoles_Assign(
    IN p_user_id INT,
    IN p_role_id INT
)
BEGIN
    -- Check if user already has this role
    IF NOT EXISTS (SELECT 1 FROM settings_user_roles 
                   WHERE user_id = p_user_id AND role_id = p_role_id)
    THEN
        -- Insert the role assignment with error handling
        BEGIN
            DECLARE EXIT HANDLER FOR SQLEXCEPTION
            BEGIN
                -- Role may already exist or other constraint violation
                -- This is non-critical so we continue silently
            END;
            
            INSERT INTO settings_user_roles (user_id, role_id, assigned_at)
            VALUES (p_user_id, p_role_id, NOW());
        END;
    END IF;
END $$

DELIMITER ;

INSERT IGNORE INTO settings_roles (role_name, description, created_at)
VALUES 
    ('User', 'Basic user role - can modify own settings', NOW()),
    ('Supervisor', 'Supervisor role - can manage user settings', NOW()),
    ('Admin', 'Administrator role - full access to all settings', NOW()),
    ('Developer', 'Developer role - unrestricted access for development', NOW());
