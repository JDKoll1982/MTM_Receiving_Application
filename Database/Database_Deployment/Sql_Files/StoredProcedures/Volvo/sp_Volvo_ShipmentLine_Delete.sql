-- =====================================================
-- Stored Procedure: sp_Volvo_ShipmentLine_Delete
-- =====================================================
-- Purpose: Delete a shipment line
-- Database: mtm_receiving_application
-- =====================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Volvo_ShipmentLine_Delete`$$

CREATE PROCEDURE `sp_Volvo_ShipmentLine_Delete`(
  IN p_id INT
)
BEGIN
    DECLARE v_old_foreign_key_checks INT DEFAULT @@FOREIGN_KEY_CHECKS;
    SET FOREIGN_KEY_CHECKS = 0;
  DELETE FROM volvo_line_data
  WHERE id = p_id;
    SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
END $$

DELIMITER ;
