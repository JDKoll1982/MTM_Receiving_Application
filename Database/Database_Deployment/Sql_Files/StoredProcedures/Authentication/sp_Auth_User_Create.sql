-- ============================================================================
-- Stored Procedure: sp_Auth_User_Create
-- ============================================================================
-- Description:
--   Creates a new user account with comprehensive validation. Ensures data
--   integrity by validating PIN format, employee number uniqueness, Windows
--   username uniqueness, and required field presence before insertion.
--
-- Feature: User Authentication & Login System (002-user-login)
-- Created: December 16, 2025
-- Last Updated: 2026-03-22
--
-- ============================================================================
-- PARAMETERS
-- ============================================================================
-- Input Parameters:
--   @p_employee_number       INT           - Employee ID (must be positive, unique)
--   @p_windows_username      VARCHAR(50)   - Windows domain username (must be unique)
--   @p_full_name             VARCHAR(100)  - Employee full name (required, trimmed)
--   @p_pin                   VARCHAR(4)    - 4-digit PIN for login (numeric only)
--   @p_department            VARCHAR(50)   - Department assignment (required)
--   @p_shift                 VARCHAR(20)   - Shift assignment (1st/2nd/3rd Shift)
--   @p_created_by            VARCHAR(50)   - Username of creating user (audit trail)
--   @p_visual_username       VARCHAR(50)   - Infor Visual ERP username (optional)
--   @p_visual_password       VARCHAR(100)  - Infor Visual ERP password (optional, encrypted)
--
-- Output Parameters:
--   @p_error_message         VARCHAR(500)  - Error description if validation fails (NULL = success)
--
-- ============================================================================
-- VALIDATION RULES
-- ============================================================================
-- 1. PIN Format:        Must be exactly 4 numeric digits (0-9)
-- 2. Employee Number:   Must be positive integer, not already in database
-- 3. Windows Username:  Must be unique, not already in database
-- 4. Full Name:         Required, non-empty after trimming
-- 5. Department:        Required, non-empty after trimming
-- 6. Shift:             Must be one of: '1st Shift', '2nd Shift', '3rd Shift'
--
-- ============================================================================
-- TRANSACTION BEHAVIOR
-- ============================================================================
-- - Uses explicit transaction (START TRANSACTION / COMMIT / ROLLBACK)
-- - On any validation failure: transaction rolls back, @p_error_message set, no insert occurs
-- - On success: user record inserted with is_active=TRUE, timestamps set to NOW()
-- - Visual credentials stored as provided (consider encryption at application layer)
-- - Calls sp_Auth_User_SeedDefaultModes() to initialize default workflow preference
--
-- ============================================================================
-- RETURN VALUES
-- ============================================================================
-- Success:  @p_error_message = NULL, new user record created
-- Failure:  @p_error_message contains specific validation error, transaction rolled back
--
-- ============================================================================
-- DEPENDENCIES
-- ============================================================================
-- - Table: auth_users (columns: employee_number, windows_username, full_name, pin,
--          department, shift, is_active, visual_username, visual_password,
--          created_by, created_date, modified_date)
-- - Procedure: sp_Auth_User_SeedDefaultModes() - initializes user workflow preferences
--
-- ============================================================================
-- SECURITY NOTES
-- ============================================================================
-- - Infor Visual password is stored in plaintext; consider application-level encryption
-- - Created_by field should be validated/audited by calling application
-- - No explicit permission checks; calling application must enforce access control
-- - Windows username should be validated against actual Active Directory (app-layer)
-- - PIN should be hashed before storage (app-layer responsibility)
--
-- ============================================================================
-- USAGE EXAMPLES
-- ============================================================================
-- Successful user creation:
--   CALL sp_Auth_User_Create(
--       1001,
--       'DOMAIN\\jsmith',
--       'John Smith',
--       '1234',
--       'Manufacturing',
--       '1st Shift',
--       'ADMIN',
--       'jsmith_visual',
--       'encrypted_password_here',
--       @error_msg
--   );
--   SELECT @error_msg;
--
-- Duplicate employee number (should fail):
--   CALL sp_Auth_User_Create(
--       1001,
--       'DOMAIN\\jdoe',
--       'Jane Doe',
--       '5678',
--       'Receiving',
--       '2nd Shift',
--       'ADMIN',
--       'jdoe_visual',
--       'visual_pass',
--       @error_msg
--   );
--   SELECT @error_msg;
--
-- ============================================================================
USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Auth_User_Create`;

DELIMITER $$

CREATE PROCEDURE `sp_Auth_User_Create`(
    IN p_employee_number INT,
    IN p_windows_username VARCHAR(50),
    IN p_full_name VARCHAR(100),
    IN p_pin VARCHAR(4),
    IN p_department VARCHAR(50),
    IN p_shift VARCHAR(20),
    IN p_created_by VARCHAR(50),
    IN p_visual_username VARCHAR(50),
    IN p_visual_password VARCHAR(100),
    OUT p_error_message VARCHAR(500)
) BEGIN DECLARE v_existing_count INT DEFAULT 0;

DECLARE v_emp_count INT DEFAULT 0;

-- Initialize output parameter (NULL indicates success)
SET
    p_error_message = NULL;

-- Start transaction to ensure atomicity of validation + insert
START TRANSACTION;

-- VALIDATION 1: PIN Format (must be exactly 4 numeric digits)
IF p_pin NOT REGEXP '^[0-9]{4}$' THEN
SET
    p_error_message = 'PIN must be exactly 4 numeric digits';

ROLLBACK;

END IF;

-- VALIDATION 2: Employee number (must be positive)
IF p_error_message IS NULL
AND (
    p_employee_number IS NULL
    OR p_employee_number <= 0
) THEN
SET
    p_error_message = 'Employee number must be a positive number';

ROLLBACK;

END IF;

-- VALIDATION 3: Employee number uniqueness
IF p_error_message IS NULL THEN
SELECT
    COUNT(*) INTO v_emp_count
FROM
    auth_users
WHERE
    employee_number = p_employee_number;

IF v_emp_count > 0 THEN
SET
    p_error_message = 'Employee number already exists in database';

ROLLBACK;

END IF;

END IF;

-- VALIDATION 4: Windows username uniqueness
IF p_error_message IS NULL THEN
SELECT
    COUNT(*) INTO v_existing_count
FROM
    auth_users
WHERE
    windows_username = p_windows_username;

IF v_existing_count > 0 THEN
SET
    p_error_message = 'Windows username already exists in database';

ROLLBACK;

END IF;

END IF;

-- VALIDATION 5: Full Name (required, non-empty)
IF p_error_message IS NULL
AND (
    p_full_name IS NULL
    OR TRIM(p_full_name) = ''
) THEN
SET
    p_error_message = 'Full Name is required';

ROLLBACK;

END IF;

-- VALIDATION 6: Department (required, non-empty)
IF p_error_message IS NULL
AND (
    p_department IS NULL
    OR TRIM(p_department) = ''
) THEN
SET
    p_error_message = 'Department is required';

ROLLBACK;

END IF;

-- VALIDATION 7: Shift (must be one of three allowed values)
IF p_error_message IS NULL
AND p_shift NOT IN ('1st Shift', '2nd Shift', '3rd Shift') THEN
SET
    p_error_message = 'Shift must be 1st Shift, 2nd Shift, or 3rd Shift';

ROLLBACK;

END IF;

-- INSERT: Create new user record if all validations passed
IF p_error_message IS NULL THEN
INSERT INTO
    auth_users (
        employee_number,
        windows_username,
        full_name,
        pin,
        department,
        shift,
        is_active,
        visual_username,
        visual_password,
        created_by,
        created_date,
        modified_date
    )
VALUES
    (
        p_employee_number,
        p_windows_username,
        TRIM(p_full_name),
        p_pin,
        TRIM(p_department),
        p_shift,
        TRUE,
        NULLIF(TRIM(p_visual_username), ''),
        NULLIF(TRIM(p_visual_password), ''),
        p_created_by,
        NOW(),
        NOW()
    );

-- BEST-EFFORT: Seed default workflow modes for new user
-- This call is safe even if default_* columns haven't been migrated yet
-- The called procedure checks column existence before attempting updates
CALL sp_Auth_User_SeedDefaultModes(p_employee_number);

-- Commit transaction on success
COMMIT;

END IF;

END $$ 

DELIMITER ;

-- ============================================================================
-- TEST CASES
-- ============================================================================
-- NOTE: Run these tests in a development/test database only.
-- NOTE: Use Database/Scripts/Set-SqlTestBlockState.ps1 to enable, disable, or
--       toggle the test bodies below while preserving the block markers.
-- NOTE: Headers and footers stay commented so the PowerShell script can find
--       each block reliably.
--
-- TEST BLOCK START: TEST 1 - Valid user creation
-- SET @error_msg = 'NOT EXECUTED';
-- CALL sp_Auth_User_Create(
--     1001,
--     'DOMAIN\\testuser01',
--     'Test User One',
--     '1234',
--     'Receiving',
--     '1st Shift',
--     'SYSTEM_ADMIN',
--     'testuser01_vis',
--     'visual_pwd_1',
--     @error_msg
-- );
-- SELECT
--     'TEST 1 - Valid Creation' AS TestName,
--     @error_msg AS Result,
--     (
--         SELECT COUNT(*)
--         FROM auth_users
--         WHERE employee_number = 1001
--     ) AS UserExists;
-- TEST BLOCK END: TEST 1 - Valid user creation
--
-- TEST BLOCK START: TEST 2 - Invalid PIN format
-- SET @error_msg = 'NOT EXECUTED';
-- CALL sp_Auth_User_Create(
--     1002,
--     'DOMAIN\\testuser02',
--     'Test User Two',
--     '12345', -- Invalid: 5 digits instead of 4
--     'Receiving',
--     '1st Shift',
--     'SYSTEM_ADMIN',
--     '',
--     '',
--     @error_msg
-- );
-- SELECT
--     'TEST 2 - Invalid PIN' AS TestName,
--     @error_msg AS Result;
-- TEST BLOCK END: TEST 2 - Invalid PIN format
--
-- TEST BLOCK START: TEST 3 - Duplicate employee number
-- SET @error_msg = 'NOT EXECUTED';
-- CALL sp_Auth_User_Create(
--     1001, -- Already exists from TEST 1
--     'DOMAIN\\testuser03',
--     'Test User Three',
--     '5678',
--     'Manufacturing',
--     '2nd Shift',
--     'SYSTEM_ADMIN',
--     '',
--     '',
--     @error_msg
-- );
-- SELECT
--     'TEST 3 - Duplicate Employee Number' AS TestName,
--     @error_msg AS Result;
-- TEST BLOCK END: TEST 3 - Duplicate employee number
--
-- TEST BLOCK START: TEST 4 - Invalid shift value
-- SET @error_msg = 'NOT EXECUTED';
-- CALL sp_Auth_User_Create(
--     1004,
--     'DOMAIN\\testuser04',
--     'Test User Four',
--     '9999',
--     'Receiving',
--     'INVALID_SHIFT', -- Invalid shift
--     'SYSTEM_ADMIN',
--     '',
--     '',
--     @error_msg
-- );
-- SELECT
--     'TEST 4 - Invalid Shift' AS TestName,
--     @error_msg AS Result;
-- TEST BLOCK END: TEST 4 - Invalid shift value
--
-- TEST BLOCK START: TEST 5 - Missing full name
-- SET @error_msg = 'NOT EXECUTED';
-- CALL sp_Auth_User_Create(
--     1005,
--     'DOMAIN\\testuser05',
--     '', -- Empty full name
--     '1111',
--     'Receiving',
--     '1st Shift',
--     'SYSTEM_ADMIN',
--     '',
--     '',
--     @error_msg
-- );
-- SELECT
--     'TEST 5 - Missing Full Name' AS TestName,
--     @error_msg AS Result;
-- TEST BLOCK END: TEST 5 - Missing full name
--
-- TEST BLOCK START: TEST 6 - Duplicate Windows username
-- SET @error_msg = 'NOT EXECUTED';
-- CALL sp_Auth_User_Create(
--     1006,
--     'DOMAIN\\testuser01', -- Already exists from TEST 1
--     'Test User Six',
--     '2222',
--     'Receiving',
--     '1st Shift',
--     'SYSTEM_ADMIN',
--     '',
--     '',
--     @error_msg
-- );
-- SELECT
--     'TEST 6 - Duplicate Windows Username' AS TestName,
--     @error_msg AS Result;
-- TEST BLOCK END: TEST 6 - Duplicate Windows username
--
-- TEST BLOCK START: TEST 7 - Cleanup test users
-- DELETE FROM auth_users
-- WHERE employee_number IN (1001, 1002, 1003, 1004, 1005, 1006);
-- TEST BLOCK END: TEST 7 - Cleanup test users