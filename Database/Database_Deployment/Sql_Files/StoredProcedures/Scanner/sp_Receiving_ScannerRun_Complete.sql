-- Stored Procedure: sp_Receiving_ScannerRun_Complete
-- Description: Finalizes run status and summary counts.

USE mtm_receiving_application_test;

DROP PROCEDURE IF EXISTS `sp_Receiving_ScannerRun_Complete`;

DELIMITER $$

CREATE PROCEDURE `sp_Receiving_ScannerRun_Complete`(
    IN p_run_id CHAR(36),
    IN p_sent_count INT,
    IN p_failed_count INT,
    IN p_waiting_count INT,
    IN p_stop_reason VARCHAR(120),
    IN p_summary_message VARCHAR(500)
)
BEGIN
    UPDATE receiving_scanner_run
    SET sent_count = COALESCE(p_sent_count, 0),
        failed_count = COALESCE(p_failed_count, 0),
        waiting_count = COALESCE(p_waiting_count, 0),
        stop_reason = p_stop_reason,
        run_status = fn_Receiving_Scanner_StatusFromCounts(
            COALESCE(p_sent_count, 0),
            COALESCE(p_failed_count, 0),
            COALESCE(p_waiting_count, 0)
        ),
        summary_message = p_summary_message,
        ended_at = NOW()
    WHERE id = p_run_id;
END $$

DELIMITER ;