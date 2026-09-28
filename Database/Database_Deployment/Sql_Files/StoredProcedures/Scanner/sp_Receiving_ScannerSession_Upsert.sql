-- Stored Procedure: sp_Receiving_ScannerSession_Upsert
-- Description: Creates or updates a scanner current-list session header.

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Receiving_ScannerSession_Upsert`;

DELIMITER $$

CREATE PROCEDURE `sp_Receiving_ScannerSession_Upsert`(
    IN p_id CHAR(36),
    IN p_user_id VARCHAR(100),
    IN p_profile_id CHAR(36),
    IN p_session_name VARCHAR(120),
    IN p_status VARCHAR(24),
    IN p_stop_requested TINYINT(1),
    IN p_last_message VARCHAR(500)
)
BEGIN
    INSERT INTO receiving_scanner_session
    (
        id,
        user_id,
        profile_id,
        session_name,
        status,
        stop_requested,
        last_message
    )
    VALUES
    (
        p_id,
        p_user_id,
        p_profile_id,
        p_session_name,
        p_status,
        COALESCE(p_stop_requested, 0),
        p_last_message
    )
    ON DUPLICATE KEY UPDATE
        profile_id = VALUES(profile_id),
        session_name = VALUES(session_name),
        status = VALUES(status),
        stop_requested = VALUES(stop_requested),
        last_message = VALUES(last_message),
        updated_at = CURRENT_TIMESTAMP;
END $$

DELIMITER ;
