USE mtm_receiving_application;

DELIMITER //

DROP PROCEDURE IF EXISTS sp_routing_label_mark_exported //
CREATE PROCEDURE sp_routing_label_mark_exported(
    IN p_label_id INT,
    OUT p_status INT,
    OUT p_error_msg VARCHAR(500)
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        SET p_status = -1;
        SET p_error_msg = 'A database error occurred';
        ROLLBACK;
    END;

    START TRANSACTION;

    IF p_label_id IS NULL THEN
        SET p_status = -1;
        SET p_error_msg = 'Label ID is required';
        ROLLBACK;
    ELSE
        UPDATE routing_label_data
        SET csv_exported = 1,
            csv_export_date = NOW()
        WHERE id = p_label_id;

        IF ROW_COUNT() = 0 THEN
            SET p_status = -1;
            SET p_error_msg = 'Label not found';
            ROLLBACK;
        ELSE
            SET p_status = 1;
            SET p_error_msg = 'Label marked as exported';
            COMMIT;
        END IF;
    END IF;
END //
DELIMITER ;
