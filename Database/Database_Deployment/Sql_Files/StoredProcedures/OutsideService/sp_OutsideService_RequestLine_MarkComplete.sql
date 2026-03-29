DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_OutsideService_RequestLine_MarkComplete`$$

CREATE PROCEDURE `sp_OutsideService_RequestLine_MarkComplete`(
    IN p_outside_service_request_line_id INT,
    IN p_completion_notes TEXT
)
BEGIN
    UPDATE outside_service_request_line
    SET
        completed_utc = UTC_TIMESTAMP(),
        completion_notes = p_completion_notes,
        line_phase = 'Complete'
    WHERE outside_service_request_line_id = p_outside_service_request_line_id;
END$$

DELIMITER ;