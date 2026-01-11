-- ============================================================================
-- Stored Procedure: sp_Auth_Activity_Log
-- Description: Log authentication events for audit trail
-- Feature: User Authentication & Login System (002-user-login)
-- Created: December 16, 2025
-- ============================================================================

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Auth_Activity_Log`;

DELIMITER $$

CREATE PROCEDURE `sp_Auth_Activity_Log`(
    IN p_event_type VARCHAR(50),
    IN p_username VARCHAR(50),
    IN p_workstation_name VARCHAR(50),
    IN p_details TEXT
)
BEGIN
    -- Log user activity event
    -- Event types: login_success, login_failed, session_timeout, user_created, pin_reset

    INSERT INTO settings_personal_activity_log (
        event_type,
        username,
        workstation_name,
        event_timestamp,
        details
    ) VALUES (
        p_event_type,
        p_username,
        p_workstation_name,
        NOW(),
        p_details
    );

    -- Return success indicator
    SELECT ROW_COUNT() AS rows_affected;

END$$

DELIMITER ;

-- ============================================================================
-- Test Query (comment out in production)
-- ============================================================================
/*
CALL sp_Auth_Activity_Log(
    'login_success',
    'JSMITH',
    'OFFICE-PC-025',
    'Windows authentication successful'
);
*/
