-- =====================================================
-- Stored Procedure: sp_Volvo_ShipmentHistory_Delete
-- =====================================================
-- Purpose: Delete an archived shipment history header and its archived lines (CASCADE)
-- Database: mtm_receiving_application
-- =====================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Volvo_ShipmentHistory_Delete`$$

CREATE PROCEDURE `sp_Volvo_ShipmentHistory_Delete`(
  IN p_shipment_history_id INT
)
BEGIN
  DELETE FROM volvo_label_history
  WHERE id = p_shipment_history_id;
END $$

DELIMITER ;