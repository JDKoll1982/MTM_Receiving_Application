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
    DECLARE v_old_foreign_key_checks INT DEFAULT @@FOREIGN_KEY_CHECKS;
    SET FOREIGN_KEY_CHECKS = 0;
  DELETE FROM volvo_label_history
  WHERE id = p_shipment_history_id;
    SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
END $$

DELIMITER ;
