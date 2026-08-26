-- Stored Procedure: sp_Receiving_ScannerSession_GetByUser
-- Description: Returns scanner sessions for a user (drives History).

USE mtm_receiving_application_test;

DROP PROCEDURE IF EXISTS `sp_Receiving_ScannerSession_GetByUser`;

DELIMITER $$

CREATE PROCEDURE `sp_Receiving_ScannerSession_GetByUser`(
    IN p_user_id VARCHAR(100)
)
BEGIN
    SELECT
        id,
        user_id,
        profile_id,
        session_name,
        status,
        stop_requested,
        sent_count,
        failed_count,
        waiting_count,
        last_message,
        created_at,
        updated_at
    FROM receiving_scanner_session
    WHERE user_id = p_user_id
    ORDER BY updated_at DESC;
END $$

DELIMITER ;
