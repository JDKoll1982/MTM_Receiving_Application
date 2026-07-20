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
    DECLARE v_affected_rows INT DEFAULT 0;

    DECLARE v_old_foreign_key_checks INT DEFAULT @@FOREIGN_KEY_CHECKS;
    SET FOREIGN_KEY_CHECKS = 0;
    UPDATE volvo_label_data
    SET notes = p_notes,
        modified_date = CURRENT_TIMESTAMP
    WHERE id = p_id;

    SET v_affected_rows = ROW_COUNT();

    UPDATE volvo_label_history
    SET notes = p_notes,
        modified_date = CURRENT_TIMESTAMP
    WHERE original_id = p_id;

    SELECT v_affected_rows AS affected_rows;
    SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
END $$

DELIMITER;