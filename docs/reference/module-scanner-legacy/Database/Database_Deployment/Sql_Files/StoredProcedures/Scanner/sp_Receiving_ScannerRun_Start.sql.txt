-- Stored Procedure: sp_Receiving_ScannerRun_Start
-- Description: Creates a run header at send start.

USE mtm_receiving_application_test;

DROP PROCEDURE IF EXISTS `sp_Receiving_ScannerRun_Start`;

DELIMITER $$

CREATE PROCEDURE `sp_Receiving_ScannerRun_Start`(
    IN p_run_id CHAR(36),
    IN p_session_id CHAR(36),
    IN p_user_id VARCHAR(100),
    IN p_profile_id CHAR(36)
)
BEGIN
    INSERT INTO receiving_scanner_run
    (
        id,
        session_id,
        user_id,
        profile_id,
        run_status,
        started_at
    )
    VALUES
    (
        p_run_id,
        p_session_id,
        p_user_id,
        p_profile_id,
        'Running',
        NOW()
    );
END $$

DELIMITER ;