-- Stored Procedure: sp_Receiving_LabelData_Delete
-- Description: Deletes one row from receiving_label_data using the persisted queue
--              record ID when available, otherwise falls back to the workflow GUID.

USE mtm_receiving_application_test;

DROP PROCEDURE IF EXISTS `sp_Receiving_LabelData_Delete`;

DELIMITER $$

CREATE PROCEDURE `sp_Receiving_LabelData_Delete`(
    IN p_label_data_record_id INT,
    IN p_load_id CHAR(36)
)
BEGIN
    DECLARE v_old_foreign_key_checks INT DEFAULT @@FOREIGN_KEY_CHECKS;
    SET FOREIGN_KEY_CHECKS = 0;
    DELETE FROM receiving_label_data
    WHERE (p_label_data_record_id IS NOT NULL AND id = p_label_data_record_id)
       OR (
            p_label_data_record_id IS NULL
            AND p_load_id IS NOT NULL
            AND p_load_id <> ''
            AND load_id = p_load_id
        );
    SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
END $$

DELIMITER ;
