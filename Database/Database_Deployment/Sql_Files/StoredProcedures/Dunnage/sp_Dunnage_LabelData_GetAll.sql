-- Stored Procedure: sp_Dunnage_LabelData_GetAll
-- Description: Returns all rows from the active dunnage_label_data queue,
--              ordered by received_date ascending so callers see them in
--              the order they were added.

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Dunnage_LabelData_GetAll`;

DELIMITER $$

CREATE PROCEDURE `sp_Dunnage_LabelData_GetAll`()
BEGIN
    SELECT
        id,
        load_uuid,
        part_id,
        dunnage_type_id,
        dunnage_type_name,
        dunnage_type_icon,
        quantity,
        quantity_type,
        po_number,
        received_date,
        user_id,
        location,
        label_number,
        part_skid_sequence,
        part_skid_total,
        specs_json,
        created_at
    FROM dunnage_label_data
    ORDER BY received_date ASC;
END $$

DELIMITER ;
