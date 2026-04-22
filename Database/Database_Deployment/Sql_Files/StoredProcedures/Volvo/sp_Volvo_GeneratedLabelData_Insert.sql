USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Volvo_GeneratedLabelData_Insert`;

DELIMITER $$

CREATE PROCEDURE `sp_Volvo_GeneratedLabelData_Insert`(
    IN p_shipment_id INT,
    IN p_shipment_number INT,
    IN p_shipment_date DATE,
    IN p_part_number VARCHAR(20),
    IN p_quantity INT,
    IN p_skid_number INT,
    IN p_total_skids INT,
    IN p_part_description VARCHAR(255)
)
BEGIN
    INSERT INTO volvo_generated_label_data (
        shipment_id,
        shipment_number,
        shipment_date,
        part_number,
        quantity,
        skid_number,
        total_skids,
        part_description
    )
    VALUES (
        p_shipment_id,
        p_shipment_number,
        p_shipment_date,
        p_part_number,
        p_quantity,
        p_skid_number,
        p_total_skids,
        p_part_description
    );
END $$

DELIMITER ;