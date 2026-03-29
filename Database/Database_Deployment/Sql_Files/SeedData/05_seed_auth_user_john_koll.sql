-- =============================================
-- Author: MTM Development
-- Create date: 2026-03-28
-- Description: Seeds the startup authentication footprint for John Koll.
--              This covers the auth_users row used by the first Windows
--              username lookup during startup and the default User role
--              assignment expected later in startup.
-- =============================================

USE mtm_receiving_application;

SET NAMES utf8mb4 COLLATE utf8mb4_unicode_ci;

SET @target_employee_number = 6229;
SET @target_windows_username = _utf8mb4'johnk' COLLATE utf8mb4_unicode_ci;
SET @target_full_name = _utf8mb4'John Koll' COLLATE utf8mb4_unicode_ci;
SET @target_pin_hash = 'lEODioKTou15LvmLpehPwNEdLVPqMz/0m2CxAemzM1M=';
SET @target_department = _utf8mb4'Receiving' COLLATE utf8mb4_unicode_ci;
SET @target_shift = _utf8mb4'1st Shift' COLLATE utf8mb4_unicode_ci;
SET @target_visual_username = 'OMiw1oI7BDE1ucZ+b7R0iqGb8vu1+tNT16aYPW/1TD8=';
SET @target_visual_password = 'cSyitBLgH2K3hz7zb/Bxuk6vZY/A/njke5G6S3cWJFc=';
SET @target_created_by = _utf8mb4'johnk' COLLATE utf8mb4_unicode_ci;
SET @default_role_name = _utf8mb4'User' COLLATE utf8mb4_unicode_ci;

UPDATE auth_users
SET
    employee_number = @target_employee_number,
    windows_username = @target_windows_username,
    full_name = @target_full_name,
    pin = @target_pin_hash,
    department = @target_department,
    shift = @target_shift,
    is_active = TRUE,
    visual_username = @target_visual_username,
    visual_password = @target_visual_password,
    default_receiving_mode = COALESCE(default_receiving_mode, 'guided'),
    default_dunnage_mode = COALESCE(default_dunnage_mode, 'guided'),
    modified_date = NOW()
WHERE employee_number = @target_employee_number
    OR windows_username = @target_windows_username COLLATE utf8mb4_unicode_ci;

INSERT INTO auth_users (
    employee_number,
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
    @target_employee_number,
    @target_windows_username,
    @target_full_name,
    @target_pin_hash,
    @target_department,
    @target_shift,
    TRUE,
    @target_visual_username,
    @target_visual_password,
    'guided',
    'guided',
    @target_created_by,
    NOW(),
    NOW()
WHERE NOT EXISTS (
    SELECT 1
    FROM auth_users
    WHERE employee_number = @target_employee_number
         OR windows_username = @target_windows_username COLLATE utf8mb4_unicode_ci
);

INSERT INTO settings_user_roles (user_id, role_id, assigned_at)
SELECT
        @target_employee_number,
        sr.id,
        NOW()
FROM settings_roles sr
WHERE sr.role_name = @default_role_name COLLATE utf8mb4_unicode_ci
    AND NOT EXISTS (
            SELECT 1
            FROM settings_user_roles sur
            WHERE sur.user_id = @target_employee_number
                AND sur.role_id = sr.id
    );