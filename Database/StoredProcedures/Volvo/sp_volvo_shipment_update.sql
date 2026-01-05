DELIMITER $$

CREATE PROCEDURE sp_volvo_shipment_update(
    IN p_id INT,
    IN p_notes TEXT
)
BEGIN
    UPDATE volvo_shipments 
    SET notes = p_notes,
        modified_date = CURRENT_TIMESTAMP
    WHERE id = p_id;
    
    SELECT ROW_COUNT() AS affected_rows;
END$$

DELIMITER ;
