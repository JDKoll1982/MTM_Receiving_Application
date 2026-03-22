-- ============================================================================
-- Stored Procedure: sp_Auth_User_Deactivate
-- Description: Soft-delete — sets is_active = 0 + audit log row
-- Feature: User Management UI
-- Created: 2026-03-08
-- ============================================================================

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Auth_User_Deactivate`;

DELIMITER $$

CREATE PROCEDURE `sp_Auth_User_Deactivate`(
    IN p_employee_number    INT,
    IN p_updated_by         VARCHAR(50),
    OUT p_error_message     VARCHAR(500)
)
sp: BEGIN
    DECLARE v_existing_count INT DEFAULT 0;

    SET p_error_message = NULL;

    -- Validate employee exists
    SELECT COUNT(*) INTO v_existing_count
    FROM auth_users
    WHERE employee_number = p_employee_number;

    IF v_existing_count = 0 THEN
        SET p_error_message = 'Employee not found';
        LEAVE sp;
    END IF;

    START TRANSACTION;

    UPDATE auth_users
    SET
        is_active     = 0,
        modified_date = NOW()
    WHERE employee_number = p_employee_number;

    -- Write audit row
    INSERT INTO settings_personal_activity_log (
        event_type,
        username,
        workstation_name,
        event_timestamp,
        details
    ) VALUES (
        'user_deactivated',
        p_updated_by,
        'SETTINGS_UI',
        NOW(),
        CONCAT('User deactivated: employee_number=', p_employee_number)
    );

    COMMIT;
END$$

DELIMITER ;

-- ============================================================================
-- Test Query (comment out in production)
-- ============================================================================
/*
SET @err = NULL;
CALL sp_Auth_User_Deactivate(1001, 'ADMIN', @err);
SELECT @err;
*/
