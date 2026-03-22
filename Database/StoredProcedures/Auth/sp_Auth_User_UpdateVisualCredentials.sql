-- ============================================================================
-- Stored Procedure: sp_Auth_User_UpdateVisualCredentials
-- Description: Update only the Visual credential fields + audit log row
-- Feature: User Management UI
-- Created: 2026-03-08
-- ============================================================================

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Auth_User_UpdateVisualCredentials`;

DELIMITER $$

CREATE PROCEDURE `sp_Auth_User_UpdateVisualCredentials`(
    IN p_employee_number    INT,
    IN p_visual_username    VARCHAR(50),
    IN p_visual_password    VARCHAR(100),
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
        visual_username  = NULLIF(TRIM(COALESCE(p_visual_username, '')), ''),
        visual_password  = NULLIF(TRIM(COALESCE(p_visual_password, '')), ''),
        modified_date    = NOW()
    WHERE employee_number = p_employee_number;

    -- Write audit row
    INSERT INTO settings_personal_activity_log (
        event_type,
        username,
        workstation_name,
        event_timestamp,
        details
    ) VALUES (
        'visual_credentials_updated',
        p_updated_by,
        'SETTINGS_UI',
        NOW(),
        CONCAT('Visual credentials updated for employee_number=', p_employee_number)
    );

    COMMIT;
END$$

DELIMITER ;

-- ============================================================================
-- Test Query (comment out in production)
-- ============================================================================
/*
SET @err = NULL;
CALL sp_Auth_User_UpdateVisualCredentials(1001, 'jsmith_visual', 'NewPass123', 'ADMIN', @err);
SELECT @err;
*/
