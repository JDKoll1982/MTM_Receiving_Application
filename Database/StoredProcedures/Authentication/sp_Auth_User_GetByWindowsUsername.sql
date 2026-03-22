-- ============================================================================
-- Stored Procedure: sp_Auth_User_GetByWindowsUsername
-- ============================================================================
-- Description: 
--   Retrieve user by Windows username for automatic authentication.
--   This procedure is called during application startup to validate the current
--   Windows user and load their profile data for MVVM binding.
--
-- Feature: 
--   User Authentication & Login System (002-user-login)
--
-- Usage:
--   CALL sp_Auth_User_GetByWindowsUsername('DOMAIN\USERNAME');
--
-- Parameters:
--   @p_windows_username VARCHAR(50)
--     The Windows username (typically DOMAIN\username format) of the user
--     to authenticate. Case-insensitive matching is performed.
--
-- Returns:
--   Single result set with columns:
--     - employee_number: Unique identifier for the employee
--     - windows_username: Windows domain username (e.g., DOMAIN\JSMITH)
--     - full_name: Employee's full name for display
--     - pin: Personal identification number (if required)
--     - department: Department assignment
--     - shift: Work shift assignment
--     - is_active: Boolean flag (1=active, 0=inactive)
--     - visual_username: Infor Visual ERP login username
--     - visual_password: Infor Visual ERP login password (encrypted in production)
--     - default_receiving_mode: User's default receiving workflow mode
--     - default_dunnage_mode: User's default dunnage workflow mode
--     - created_date: Timestamp when record was created
--     - created_by: User who created the record
--     - modified_date: Timestamp of last modification
--
-- Behavior:
--   - Returns exactly 1 row if user exists and is_active = TRUE
--   - Returns 0 rows if user not found or is_active = FALSE
--   - Returns NULL columns if no match found (ViewModel handles empty result)
--   - Uses LIMIT 1 to prevent accidental multiple results
--   - Case-insensitive matching via direct string comparison (MySQL default)
--
-- Error Handling:
--   - If p_windows_username is NULL or empty, no rows returned
--   - No exceptions thrown; returns empty result set for invalid input
--   - Calling code (DAO) must handle empty result set gracefully
--   - DAO should return Model_Dao_Result with success=false if no user found
--
-- Performance Notes:
--   - Ensure `auth_users.windows_username` column has a UNIQUE INDEX
--   - LIMIT 1 prevents multiple results; should be impossible with UK constraint
--   - Result set is typically <1ms for indexed lookup
--   - Monitor query performance if auth_users table grows beyond 10k rows
--
-- Security Notes:
--   - NEVER expose visual_password in UI or logs in production
--   - Encrypt visual_password column at rest in database
--   - Call only from trusted application layer (not from web APIs)
--   - Windows username validation should be performed at OS level before calling
--   - Sanitize input in calling code; use parameterized queries via MySqlParameter
--   - Consider adding audit logging for authentication attempts
--   - Validate is_active flag to prevent deleted/disabled users from logging in
--
-- Related Procedures:
--   - sp_Auth_User_ValidatePIN: Validate user PIN for manual login fallback
--   - sp_Auth_User_UpdateLastLogin: Record login timestamp
--   - sp_Auth_User_GetByEmployeeNumber: Alternative lookup by employee ID
--
-- Dependencies:
--   - Table: auth_users
--   - Required Columns: employee_number, windows_username, is_active, full_name, pin, 
--     department, shift, visual_username, visual_password, default_receiving_mode, 
--     default_dunnage_mode, created_date, created_by, modified_date
--   - Index: UNIQUE INDEX on auth_users.windows_username (recommended)
--
-- Notes for DAO Implementation:
--   - Wrap this call in MySqlParameter to prevent injection attacks
--   - Return Model_Dao_Result with success=true and data if user found
--   - Return Model_Dao_Result with success=false if user not found or inactive
--   - Log authentication attempts (success and failure) for audit trail
--   - Cache result in ViewModel if calling multiple times per session
--
-- Last Modified: December 16, 2025
-- Modified By: System Setup
-- ============================================================================
USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Auth_User_GetByWindowsUsername`;

DELIMITER $ $ CREATE PROCEDURE `sp_Auth_User_GetByWindowsUsername`(IN p_windows_username VARCHAR(50)) BEGIN -- Get user by Windows username
-- Returns user data if found and active, NULL if not found or inactive
SELECT
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
    created_date,
    created_by,
    modified_date
FROM
    auth_users
WHERE
    windows_username = p_windows_username
    AND is_active = TRUE
LIMIT
    1;

END $ $ DELIMITER;

-- ============================================================================
-- Test Queries (comment out in production environments)
-- ============================================================================
-- Test 1: Authenticate an active user
-- Expected: Returns user row if user exists and is active
-- CALL sp_Auth_User_GetByWindowsUsername('JSMITH');
-- Test 2: Query with domain prefix (common Windows format)
-- Expected: Returns user row if DOMAIN\JSMITH exists
-- CALL sp_Auth_User_GetByWindowsUsername('DOMAIN\JSMITH');
-- Test 3: Query non-existent user
-- Expected: Returns empty result set (0 rows)
-- CALL sp_Auth_User_GetByWindowsUsername('NONEXISTENT');
-- Test 4: Query with NULL input
-- Expected: Returns empty result set (0 rows)
-- CALL sp_Auth_User_GetByWindowsUsername(NULL);
-- Test 5: Query inactive user
-- Expected: Returns empty result set (0 rows) because WHERE is_active = TRUE filters them out
-- CALL sp_Auth_User_GetByWindowsUsername('INACTIVE_USER');
-- Test 6: Verify correct columns are returned
-- Run once to validate schema matches DAO expectations
-- SELECT * FROM auth_users WHERE is_active = TRUE LIMIT 1;