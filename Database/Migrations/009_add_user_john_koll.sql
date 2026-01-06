-- =====================================================
-- Migration: 009_add_user_john_koll
-- Purpose: Add John Koll user account for Receiving
-- Date: 2026-01-05
-- =====================================================

USE mtm_receiving_application;

-- Insert John Koll user with all credentials
INSERT INTO users (
    employee_number,
    windows_username,
    full_name,
    pin,
    department,
    shift,
    is_active,
    visual_username,
    visual_password,
    created_by
) VALUES (
    6229,
    'johnk',
    'John Koll',
    '0831',
    'Receiving',
    '1st Shift',
    TRUE,
    'JKOLL',
    'KOLL',
    'SYSTEM'
)
ON DUPLICATE KEY UPDATE
    windows_username = VALUES(windows_username),
    full_name = VALUES(full_name),
    pin = VALUES(pin),
    department = VALUES(department),
    shift = VALUES(shift),
    is_active = VALUES(is_active),
    visual_username = VALUES(visual_username),
    visual_password = VALUES(visual_password);

-- Verification
SELECT 
    employee_number,
    windows_username,
    full_name,
    department,
    shift,
    visual_username,
    'Password Set' as visual_password_status
FROM users 
WHERE employee_number = 6229;
