-- ============================================================================
-- Stored Procedure: sp_LogUserActivity
-- Description: Log authentication events for audit trail
-- Feature: User Authentication & Login System (001-user-login)
-- Created: December 16, 2025
-- ============================================================================

USE mtm_receiving_db;

DROP PROCEDURE IF EXISTS sp_LogUserActivity;

DELIMITER $$

CREATE PROCEDURE sp_LogUserActivity(
    IN p_event_type VARCHAR(50),
    IN p_username VARCHAR(50),
    IN p_workstation_name VARCHAR(50),
    IN p_details TEXT
)
BEGIN
    -- Log user activity event
    -- Event types: login_success, login_failed, session_timeout, user_created, pin_reset
    
    INSERT INTO user_activity_log (
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
CALL sp_LogUserActivity(
    'login_success',
    'JSMITH',
    'OFFICE-PC-025',
    'Windows authentication successful'
);
*/
