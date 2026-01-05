DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_routing_label_history_insert` $$

CREATE PROCEDURE `sp_routing_label_history_insert`(
    IN p_label_id INT,
    IN p_field_changed VARCHAR(50),
    IN p_old_value VARCHAR(200),
    IN p_new_value VARCHAR(200),
    IN p_edited_by INT,
    OUT p_status INT,
    OUT p_error_msg VARCHAR(500)
)
sp_routing_label_history_insert: BEGIN
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
        LEAVE sp_routing_label_history_insert;
    END IF;

    IF p_field_changed IS NULL OR p_field_changed = '' THEN
        SET p_status = -1;
        SET p_error_msg = 'Field changed is required';
        ROLLBACK;
        LEAVE sp_routing_label_history_insert;
    END IF;

    INSERT INTO routing_label_history (
        label_id,
        field_changed,
        old_value,
        new_value,
        edited_by
    ) VALUES (
        p_label_id,
        p_field_changed,
        p_old_value,
        p_new_value,
        p_edited_by
    );

    SET p_status = 1;
    SET p_error_msg = 'History entry created';

    COMMIT;
END $$

DELIMITER ;
