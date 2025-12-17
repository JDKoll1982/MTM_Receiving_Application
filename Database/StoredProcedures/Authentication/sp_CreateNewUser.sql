-- ============================================================================
-- Stored Procedure: sp_CreateNewUser
-- Description: Create new user account with validation
-- Feature: User Authentication & Login System (001-user-login)
-- Created: December 16, 2025
-- ============================================================================

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS sp_CreateNewUser;

DELIMITER $$

CREATE PROCEDURE sp_CreateNewUser(
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
)
BEGIN
    DECLARE v_existing_count INT DEFAULT 0;
    DECLARE v_pin_count INT DEFAULT 0;
    DECLARE v_emp_count INT DEFAULT 0;
    
    -- Initialize output parameters
    SET p_error_message = NULL;
    
    -- Start transaction
    START TRANSACTION;
    
    -- Validate PIN format (4 digits)
    IF p_pin NOT REGEXP '^[0-9]{4}$' THEN
        SET p_error_message = 'PIN must be exactly 4 numeric digits';
        ROLLBACK;
    END IF;
    
    -- Validate employee number
    IF p_employee_number IS NULL OR p_employee_number <= 0 THEN
        SET p_error_message = 'Employee number must be a positive number';
        ROLLBACK;
    END IF;
    
    -- Check if employee number already exists
    SELECT COUNT(*) INTO v_emp_count
    FROM users
    WHERE employee_number = p_employee_number;
    
    IF v_emp_count > 0 THEN
        SET p_error_message = 'Employee number already exists in database';
        ROLLBACK;
    END IF;
    
    -- Check if Windows username already exists
    SELECT COUNT(*) INTO v_existing_count
    FROM users
    WHERE windows_username = p_windows_username;
    
    IF v_existing_count > 0 THEN
        SET p_error_message = 'Windows username already exists in database';
        ROLLBACK;
    END IF;
    
    -- Check if PIN already in use
    IF p_error_message IS NULL THEN
        SELECT COUNT(*) INTO v_pin_count
        FROM users
        WHERE pin = p_pin;
        
        IF v_pin_count > 0 THEN
            SET p_error_message = 'This PIN is already in use. Please choose a different PIN';
            ROLLBACK;
        END IF;
    END IF;
    
    -- Validate required fields
    IF p_error_message IS NULL AND (p_full_name IS NULL OR TRIM(p_full_name) = '') THEN
        SET p_error_message = 'Full Name is required';
        ROLLBACK;
    END IF;
    
    IF p_error_message IS NULL AND (p_department IS NULL OR TRIM(p_department) = '') THEN
        SET p_error_message = 'Department is required';
        ROLLBACK;
    END IF;
    
    IF p_error_message IS NULL AND p_shift NOT IN ('1st Shift', '2nd Shift', '3rd Shift') THEN
        SET p_error_message = 'Shift must be 1st Shift, 2nd Shift, or 3rd Shift';
        ROLLBACK;
    END IF;
    
    -- Insert new user only if no errors
    IF p_error_message IS NULL THEN
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
        created_by,
        created_date,
        modified_date
    ) VALUES (
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
        
        -- Commit transaction
        COMMIT;
    END IF;
    
END$$

DELIMITER ;

-- ============================================================================
-- Test Query (comment out in production)
-- ============================================================================
/*
CALL sp_CreateNewUser(
    'JDOE',                  -- windows_username
    'Jane Doe',              -- full_name
    '3456',                  -- pin
    'Receiving',             -- department
    '1st Shift',             -- shift
    'ADMIN',                 -- created_by
    NULL,                    -- visual_username
    NULL,                    -- visual_password
    @emp_num,                -- OUT employee_number
    @error                   -- OUT error_message
);
SELECT @emp_num AS employee_number, @error AS error_message;
*/
