-- ============================================================================
-- Stored Procedure: sp_Auth_User_GetByWindowsUsername
-- Description: Retrieve user by Windows username for automatic authentication
-- Feature: User Authentication & Login System (002-user-login)
-- Created: December 16, 2025
-- ============================================================================

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Auth_User_GetByWindowsUsername`;

DELIMITER $$

CREATE PROCEDURE `sp_Auth_User_GetByWindowsUsername`(
    IN p_windows_username VARCHAR(50)
)
BEGIN
    -- Get user by Windows username
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
    FROM auth_users
    WHERE windows_username = p_windows_username
      AND is_active = TRUE
    LIMIT 1;

END$$

DELIMITER ;

-- ============================================================================
-- Test Query (comment out in production)
-- ============================================================================
-- CALL sp_Auth_User_GetByWindowsUsername('JSMITH');
