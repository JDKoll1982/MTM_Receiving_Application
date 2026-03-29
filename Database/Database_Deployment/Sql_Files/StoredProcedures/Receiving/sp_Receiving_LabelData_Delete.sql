-- Stored Procedure: sp_Receiving_LabelData_Delete
-- Description: Deletes one row from receiving_label_data using the persisted queue
--              record ID when available, otherwise falls back to the workflow GUID.

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Receiving_LabelData_Delete`;

DELIMITER $$

CREATE PROCEDURE `sp_Receiving_LabelData_Delete`(
    IN p_label_data_record_id INT,
    IN p_load_id CHAR(36)
)
BEGIN
    DELETE FROM receiving_label_data
    WHERE (p_label_data_record_id IS NOT NULL AND id = p_label_data_record_id)
       OR (
            p_label_data_record_id IS NULL
            AND p_load_id IS NOT NULL
            AND p_load_id <> ''
            AND load_id = p_load_id
        );
END $$

DELIMITER ;