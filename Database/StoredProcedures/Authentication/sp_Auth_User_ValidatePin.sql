-- ============================================================================
-- Stored Procedure: sp_Auth_User_ValidatePin
-- Description: Validate username and PIN combination for shared terminal login
-- Feature: User Authentication & Login System (002-user-login)
-- Created: December 16, 2025
-- ============================================================================

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Auth_User_ValidatePin`;

DELIMITER $$

CREATE PROCEDURE `sp_Auth_User_ValidatePin`(
    IN p_username VARCHAR(50),
    IN p_pin VARCHAR(4)
)
BEGIN
    -- Validate username and PIN combination
    -- Returns user data if credentials valid and account active

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
    FROM auth_users
    WHERE (windows_username = p_username OR full_name = p_username)
      AND pin = p_pin
      AND is_active = TRUE
    LIMIT 1;

END$$

DELIMITER ;

-- ============================================================================
-- Test Query (comment out in production)
-- ============================================================================
-- CALL sp_Auth_User_ValidatePin('JSMITH', '1234');
-- CALL sp_Auth_User_ValidatePin('John Smith', '1234');
