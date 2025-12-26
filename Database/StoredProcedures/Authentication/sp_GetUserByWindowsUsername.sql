-- ============================================================================
-- Stored Procedure: sp_GetUserByWindowsUsername
-- Description: Retrieve user by Windows username for automatic authentication
-- Feature: User Authentication & Login System (002-user-login)
-- Created: December 16, 2025
-- ============================================================================

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS sp_GetUserByWindowsUsername;

DELIMITER $$

CREATE PROCEDURE sp_GetUserByWindowsUsername(
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
        created_date,
        created_by,
        modified_date
    FROM users
    WHERE windows_username = p_windows_username
      AND is_active = TRUE
    LIMIT 1;
    
END$$

DELIMITER ;

-- ============================================================================
-- Test Query (comment out in production)
-- ============================================================================
-- CALL sp_GetUserByWindowsUsername('JSMITH');
