-- =============================================
-- Author: MTM Development
-- Create date: 2026-03-29
-- Description: Seeds bootstrap auth accounts for the core admin and developer users.
--              Both accounts use the protected hash for PIN 0000 and receive their
--              corresponding settings role assignment if it is not already present.
-- =============================================

USE mtm_receiving_application_test;

SET NAMES utf8mb4 COLLATE utf8mb4_unicode_ci;

SET @bootstrap_pin_hash = 'C0AASYi42xs6uUrqkjIoAoRRRoR0gOvBXnrk9LhXYpQ=';
SET @bootstrap_department = _utf8mb4'Receiving' COLLATE utf8mb4_unicode_ci;
SET @bootstrap_shift = _utf8mb4'1st Shift' COLLATE utf8mb4_unicode_ci;
SET @bootstrap_created_by = _utf8mb4'system' COLLATE utf8mb4_unicode_ci;

SET @admin_windows_username = _utf8mb4'admin' COLLATE utf8mb4_unicode_ci;
SET @admin_full_name = _utf8mb4'Core Administrator' COLLATE utf8mb4_unicode_ci;
SET @admin_role_name = _utf8mb4'Admin' COLLATE utf8mb4_unicode_ci;

UPDATE auth_users
SET
    full_name = @admin_full_name,
    pin = @bootstrap_pin_hash,
    department = @bootstrap_department,
    shift = @bootstrap_shift,
    is_active = TRUE,
    visual_username = NULL,
    visual_password = NULL,
    default_receiving_mode = COALESCE(default_receiving_mode, 'guided'),
    default_dunnage_mode = COALESCE(default_dunnage_mode, 'guided'),
    created_by = COALESCE(created_by, @bootstrap_created_by),
    modified_date = NOW()
WHERE windows_username = @admin_windows_username COLLATE utf8mb4_unicode_ci;

INSERT INTO auth_users (
    windows_username,
    full_name,
    pin,
    department,
    shift,
    is_active,
    visual_username,
    visual_password,
    default_receiving_mode,
    default_dunnage_mode,
    created_by,
    created_date,
    modified_date
)
SELECT
    @admin_windows_username,
    @admin_full_name,
    @bootstrap_pin_hash,
    @bootstrap_department,
    @bootstrap_shift,
    TRUE,
    NULL,
    NULL,
    'guided',
    'guided',
    @bootstrap_created_by,
    NOW(),
    NOW()
WHERE NOT EXISTS (
    SELECT 1
    FROM auth_users
    WHERE windows_username = @admin_windows_username COLLATE utf8mb4_unicode_ci
);

SET @admin_user_id = (
    SELECT employee_number
    FROM auth_users
    WHERE windows_username = @admin_windows_username COLLATE utf8mb4_unicode_ci
    LIMIT 1
);

INSERT INTO settings_user_roles (user_id, role_id, assigned_at)
SELECT
    @admin_user_id,
    sr.id,
    NOW()
FROM settings_roles sr
WHERE sr.role_name = @admin_role_name COLLATE utf8mb4_unicode_ci
    AND NOT EXISTS (
        SELECT 1
        FROM settings_user_roles sur
        WHERE sur.user_id = @admin_user_id
            AND sur.role_id = sr.id
    );

SET @developer_windows_username = _utf8mb4'developer' COLLATE utf8mb4_unicode_ci;
SET @developer_full_name = _utf8mb4'Core Developer' COLLATE utf8mb4_unicode_ci;
SET @developer_role_name = _utf8mb4'Developer' COLLATE utf8mb4_unicode_ci;

UPDATE auth_users
SET
    full_name = @developer_full_name,
    pin = @bootstrap_pin_hash,
    department = @bootstrap_department,
    shift = @bootstrap_shift,
    is_active = TRUE,
    visual_username = NULL,
    visual_password = NULL,
    default_receiving_mode = COALESCE(default_receiving_mode, 'guided'),
    default_dunnage_mode = COALESCE(default_dunnage_mode, 'guided'),
    created_by = COALESCE(created_by, @bootstrap_created_by),
    modified_date = NOW()
WHERE windows_username = @developer_windows_username COLLATE utf8mb4_unicode_ci;

INSERT INTO auth_users (
    windows_username,
    full_name,
    pin,
    department,
    shift,
    is_active,
    visual_username,
    visual_password,
    default_receiving_mode,
    default_dunnage_mode,
    created_by,
    created_date,
    modified_date
)
SELECT
    @developer_windows_username,
    @developer_full_name,
    @bootstrap_pin_hash,
    @bootstrap_department,
    @bootstrap_shift,
    TRUE,
    NULL,
    NULL,
    'guided',
    'guided',
    @bootstrap_created_by,
    NOW(),
    NOW()
WHERE NOT EXISTS (
    SELECT 1
    FROM auth_users
    WHERE windows_username = @developer_windows_username COLLATE utf8mb4_unicode_ci
);

SET @developer_user_id = (
    SELECT employee_number
    FROM auth_users
    WHERE windows_username = @developer_windows_username COLLATE utf8mb4_unicode_ci
    LIMIT 1
);

INSERT INTO settings_user_roles (user_id, role_id, assigned_at)
SELECT
    @developer_user_id,
    sr.id,
    NOW()
FROM settings_roles sr
WHERE sr.role_name = @developer_role_name COLLATE utf8mb4_unicode_ci
    AND NOT EXISTS (
        SELECT 1
        FROM settings_user_roles sur
        WHERE sur.user_id = @developer_user_id
            AND sur.role_id = sr.id
    );