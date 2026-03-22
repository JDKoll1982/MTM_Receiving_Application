/**
 * PROCEDURE: sp_Auth_User_GetDefaultMode
 * 
 * DESCRIPTION:
 * Retrieves the default receiving mode setting for a specific user based on their employee number.
 * This procedure is used during user authentication to determine the user's preferred receiving mode.
 * 
 * PARAMETERS:
 * @p_user_id INT - The employee number/ID of the user whose default receiving mode is to be retrieved.
 * 
 * RETURNS:
 * Single column result set:
 *   - default_receiving_mode: The user's configured default receiving mode
 * 
 * NOTES:
 * - Returns a single row if the user exists, otherwise returns empty result set
 * - Consider adding error handling for invalid user IDs
 * - Ensure the auth_users table has appropriate indexes on employee_number for performance
 * - This procedure is part of the Authentication module
 * 
 * USAGE EXAMPLE:
 * CALL sp_Auth_User_GetDefaultMode(12345);
 * 
 * DEPENDENCIES:
 * - auth_users table
 * 
 * CREATED: [DATE]
 * MODIFIED: [DATE]
 * AUTHOR: [AUTHOR NAME]
 */
DELIMITER $ $ DROP PROCEDURE IF EXISTS `sp_Auth_User_GetDefaultMode` $ $ CREATE PROCEDURE `sp_Auth_User_GetDefaultMode`(IN p_user_id INT) BEGIN -- Retrieve user's default receiving mode
SELECT
    default_receiving_mode
FROM
    auth_users
WHERE
    employee_number = p_user_id;

END $ $ DELIMITER;