USE mtm_receiving_application_test;

DROP PROCEDURE IF EXISTS `sp_Volvo_GeneratedLabelData_DeleteByShipment`;

DELIMITER $$

CREATE PROCEDURE `sp_Volvo_GeneratedLabelData_DeleteByShipment`(
    IN p_shipment_id INT
)
BEGIN
    DECLARE v_old_foreign_key_checks INT DEFAULT @@FOREIGN_KEY_CHECKS;
    SET FOREIGN_KEY_CHECKS = 0;
    DELETE FROM volvo_generated_label_data
    WHERE shipment_id = p_shipment_id;
    SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
END $$

DELIMITER ;
