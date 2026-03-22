-- ============================================================================
-- Stored Procedure: sp_Auth_User_Update
-- Description: Full user record update (all editable fields) with audit log
-- Feature: User Management UI
-- Created: 2026-03-08
-- ============================================================================
USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Auth_User_Update`;

DELIMITER $$ 

CREATE PROCEDURE `sp_Auth_User_Update`(
    IN p_employee_number INT,
    IN p_full_name VARCHAR(100),
    IN p_pin VARCHAR(4),
    IN p_department VARCHAR(50),
    IN p_shift VARCHAR(20),
    IN p_is_active TINYINT(1),
    IN p_visual_username VARCHAR(50),
    IN p_visual_password VARCHAR(100),
    IN p_updated_by VARCHAR(50),
    OUT p_error_message VARCHAR(500)
) sp: BEGIN DECLARE v_existing_count INT DEFAULT 0;

DECLARE v_workstation VARCHAR(50) DEFAULT 'SETTINGS_UI';

SET
    p_error_message = NULL;

-- Validate employee exists
SELECT
    COUNT(*) INTO v_existing_count
FROM
    auth_users
WHERE
    employee_number = p_employee_number;

IF v_existing_count = 0 THEN
SET
    p_error_message = 'Employee not found';

LEAVE sp;

END IF;

-- Validate PIN
IF p_pin IS NOT NULL
AND p_pin NOT REGEXP '^[0-9]{4}$' THEN
SET
    p_error_message = 'PIN must be exactly 4 numeric digits';

LEAVE sp;

END IF;

-- Validate shift
IF p_shift NOT IN ('1st Shift', '2nd Shift', '3rd Shift') THEN
SET
    p_error_message = 'Shift must be 1st Shift, 2nd Shift, or 3rd Shift';

LEAVE sp;

END IF;

START TRANSACTION;

UPDATE
    auth_users
SET
    full_name = TRIM(p_full_name),
    pin = p_pin,
    department = TRIM(p_department),
    shift = p_shift,
    is_active = p_is_active,
    visual_username = NULLIF(TRIM(COALESCE(p_visual_username, '')), ''),
    visual_password = NULLIF(TRIM(COALESCE(p_visual_password, '')), ''),
    modified_date = NOW()
WHERE
    employee_number = p_employee_number;

-- Write audit row
INSERT INTO
    settings_personal_activity_log (
        event_type,
        username,
        workstation_name,
        event_timestamp,
        details
    )
VALUES
    (
        'user_updated',
        p_updated_by,
        v_workstation,
        NOW(),
        CONCAT(
            'User record updated for employee_number=',
            p_employee_number
        )
    );

COMMIT;

END $$ 

DELIMITER ;

-- ============================================================================
-- Test Query (comment out in production)
-- ============================================================================
/*
 SET @err = NULL;
 CALL sp_Auth_User_Update(1001, 'John Smith', '1234', 'Receiving', '1st Shift', 1, 'jsmith', 'secret', 'ADMIN', @err);
 SELECT @err;
 */