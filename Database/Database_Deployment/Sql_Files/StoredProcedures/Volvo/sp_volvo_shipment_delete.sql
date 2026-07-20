-- =====================================================
-- Stored Procedure: sp_volvo_shipment_delete
-- =====================================================
-- Purpose: Delete a shipment and all its lines (CASCADE)
-- Database: mtm_receiving_application
-- =====================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Volvo_Shipment_Delete`$$

CREATE PROCEDURE `sp_Volvo_Shipment_Delete`(
  IN p_shipment_id INT
)
BEGIN
    SET FOREIGN_KEY_CHECKS = 0;
  -- Lines will be deleted automatically by CASCADE DELETE
  DELETE FROM volvo_label_data
  WHERE id = p_shipment_id;
    SET FOREIGN_KEY_CHECKS = 1;
END $$

DELIMITER ;
