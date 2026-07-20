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
    SET FOREIGN_KEY_CHECKS = 0;
  DELETE FROM volvo_line_data
  WHERE id = p_id;
    SET FOREIGN_KEY_CHECKS = 1;
END $$

DELIMITER ;
