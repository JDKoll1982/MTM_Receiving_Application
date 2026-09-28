-- Function: fn_Receiving_Scanner_StatusFromCounts
-- Description: Derives session/run status from sent, failed, and waiting counts.

USE mtm_receiving_application;

DROP FUNCTION IF EXISTS `fn_Receiving_Scanner_StatusFromCounts`;

DELIMITER $$

CREATE FUNCTION `fn_Receiving_Scanner_StatusFromCounts`(
    p_sent_count INT,
    p_failed_count INT,
    p_waiting_count INT
)
RETURNS VARCHAR(24)
DETERMINISTIC
BEGIN
    IF COALESCE(p_waiting_count, 0) > 0 THEN
        IF COALESCE(p_failed_count, 0) > 0 OR COALESCE(p_sent_count, 0) > 0 THEN
            RETURN 'Stopped';
        END IF;
        RETURN 'Ready';
    END IF;

    IF COALESCE(p_failed_count, 0) > 0 THEN
        IF COALESCE(p_sent_count, 0) > 0 THEN
            RETURN 'Stopped';
        END IF;
        RETURN 'Failed';
    END IF;

    IF COALESCE(p_sent_count, 0) > 0 THEN
        RETURN 'Completed';
    END IF;

    RETURN 'Draft';
END $$

DELIMITER ;