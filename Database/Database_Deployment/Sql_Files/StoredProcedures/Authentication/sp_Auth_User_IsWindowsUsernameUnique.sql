-- ============================================================================
-- Stored Procedure: sp_Auth_User_IsWindowsUsernameUnique
-- ============================================================================
-- Description:
--   Returns the number of auth_users rows that already use a Windows username,
--   optionally excluding one employee_number during edit scenarios.
--
-- Database: mtm_receiving_application
-- ============================================================================

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Auth_User_IsWindowsUsernameUnique`;

DELIMITER $$

CREATE PROCEDURE `sp_Auth_User_IsWindowsUsernameUnique`(
    IN p_windows_username VARCHAR(50),
    IN p_exclude_employee_number INT
)
BEGIN
    SELECT COUNT(*) AS username_count
    FROM auth_users
    WHERE windows_username = p_windows_username
      AND (
        p_exclude_employee_number IS NULL
        OR employee_number <> p_exclude_employee_number
      );
END $$

DELIMITER ;