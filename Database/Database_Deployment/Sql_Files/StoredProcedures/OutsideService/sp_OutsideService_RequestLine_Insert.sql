DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_OutsideService_RequestLine_Insert`$$

CREATE PROCEDURE `sp_OutsideService_RequestLine_Insert`(
    IN p_outside_service_request_id INT,
    IN p_line_number INT,
    IN p_part_id VARCHAR(50),
    IN p_package_count INT,
    IN p_package_summary VARCHAR(500)
)
BEGIN
    INSERT INTO outside_service_request_line
    (
        outside_service_request_id,
        line_number,
        part_id,
        package_count,
        package_summary,
        line_phase
    )
    VALUES
    (
        p_outside_service_request_id,
        p_line_number,
        p_part_id,
        p_package_count,
        p_package_summary,
        'Initialize'
    );

    SELECT LAST_INSERT_ID() AS outside_service_request_line_id;
END$$

DELIMITER ;