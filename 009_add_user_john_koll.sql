-- =====================================================
-- Migration: 009_add_user_john_koll
-- Purpose: Add John Koll user account for Receiving
-- Date: 2026-01-05
-- =====================================================

USE mtm_receiving_application;

-- Seed John Koll user with all credentials (idempotent)
CALL sp_UpsertUser(
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
);

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
