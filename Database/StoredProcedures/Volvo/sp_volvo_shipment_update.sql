-- Stored Procedure: sp_volvo_shipment_update
-- Purpose: Update Volvo shipment notes
-- Created for Issue #2 (Code Review - SQL Injection Protection)

DROP PROCEDURE IF EXISTS `sp_Volvo_Shipment_Update`;

DELIMITER $$

CREATE PROCEDURE `sp_Volvo_Shipment_Update`(
    IN p_id INT,
    IN p_notes TEXT
)
BEGIN
    UPDATE volvo_label_data
    SET notes = p_notes,
        modified_date = CURRENT_TIMESTAMP
    WHERE id = p_id;

    SELECT ROW_COUNT() AS affected_rows;
END$$

DELIMITER ;
