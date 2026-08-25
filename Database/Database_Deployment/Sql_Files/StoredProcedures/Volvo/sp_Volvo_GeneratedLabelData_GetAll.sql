USE mtm_receiving_application_test;

DROP PROCEDURE IF EXISTS `sp_Volvo_GeneratedLabelData_GetAll`;

DELIMITER $$

CREATE PROCEDURE `sp_Volvo_GeneratedLabelData_GetAll`()
BEGIN
    SELECT
        id,
        shipment_id,
        shipment_number,
        shipment_date,
        part_number,
        quantity,
        skid_number,
        total_skids,
        part_description,
        employee_number,
        created_at,
        updated_at
    FROM volvo_generated_label_data
    ORDER BY shipment_date ASC, shipment_number ASC, part_number ASC, skid_number ASC;
END $$

DELIMITER ;