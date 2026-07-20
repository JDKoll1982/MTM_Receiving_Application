USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Volvo_GeneratedLabelData_DeleteByShipment`;

DELIMITER $$

CREATE PROCEDURE `sp_Volvo_GeneratedLabelData_DeleteByShipment`(
    IN p_shipment_id INT
)
BEGIN
    SET FOREIGN_KEY_CHECKS = 0;
    DELETE FROM volvo_generated_label_data
    WHERE shipment_id = p_shipment_id;
    SET FOREIGN_KEY_CHECKS = 1;
END $$

DELIMITER ;
