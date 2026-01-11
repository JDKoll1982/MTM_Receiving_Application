DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_routing_label_mark_exported` $$

CREATE PROCEDURE `sp_routing_label_mark_exported`(
    IN p_label_id INT,
    OUT p_status INT,
    OUT p_error_msg VARCHAR(500)
)
sp_routing_label_mark_exported: BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1 p_error_msg = MESSAGE_TEXT;
        SET p_status = -1;
        ROLLBACK;
    END;

    START TRANSACTION;

    IF p_label_id IS NULL THEN
        SET p_status = -1;
        SET p_error_msg = 'Label ID is required';
        ROLLBACK;
        LEAVE sp_routing_label_mark_exported;
    END IF;

    UPDATE routing_label_data
    SET csv_exported = 1,
        csv_export_date = NOW()
    WHERE id = p_label_id;

    IF ROW_COUNT() = 0 THEN
        SET p_status = -1;
        SET p_error_msg = 'Label not found';
        ROLLBACK;
        LEAVE sp_routing_label_mark_exported;
    END IF;

    SET p_status = 1;
    SET p_error_msg = 'Label marked as exported';

    COMMIT;
END $$

DELIMITER ;
