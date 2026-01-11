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
  DELETE FROM volvo_line_data
  WHERE id = p_id;
END$$

DELIMITER ;
