DELIMITER $$

DROP PROCEDURE IF EXISTS sp_dunnage_history_update$$

CREATE PROCEDURE sp_dunnage_history_update(
    IN p_load_uuid CHAR(36),
    IN p_quantity DECIMAL(10,2),
    IN p_user VARCHAR(50)
)
BEGIN
    UPDATE dunnage_history
    SET
        quantity = p_quantity,
        modified_by = p_user,
        modified_date = NOW()
    WHERE load_uuid = p_load_uuid;
END$$

DELIMITER ;
