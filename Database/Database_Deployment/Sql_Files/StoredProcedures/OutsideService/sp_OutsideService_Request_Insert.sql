DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_OutsideService_Request_Insert`$$

CREATE PROCEDURE `sp_OutsideService_Request_Insert`(
    IN p_created_by_user VARCHAR(255),
    IN p_created_by_display VARCHAR(255),
    IN p_request_notes TEXT
)
BEGIN
    DECLARE v_new_id INT;
    DECLARE v_request_number VARCHAR(20);

    INSERT INTO outside_service_request
    (
        request_number,
        created_by_user,
        created_by_display,
        created_utc,
        request_notes
    )
    VALUES
    (
        '',
        p_created_by_user,
        p_created_by_display,
        UTC_TIMESTAMP(),
        p_request_notes
    );

    SET v_new_id = LAST_INSERT_ID();
    SET v_request_number = CONCAT('OS-', LPAD(v_new_id, 6, '0'));

    UPDATE outside_service_request
    SET request_number = v_request_number
    WHERE outside_service_request_id = v_new_id;

    SELECT
        v_new_id AS outside_service_request_id,
        v_request_number AS request_number,
        UTC_TIMESTAMP() AS created_utc;
END$$

DELIMITER ;