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
    DECLARE v_old_foreign_key_checks INT DEFAULT @@FOREIGN_KEY_CHECKS;
    SET FOREIGN_KEY_CHECKS = 0;
  -- MySQL does not fire ON DELETE CASCADE while FOREIGN_KEY_CHECKS is 0, so the lines
  -- are deleted explicitly instead of being left behind as orphans.
  DELETE FROM volvo_line_data
  WHERE shipment_id = p_shipment_id;
  DELETE FROM volvo_label_data
  WHERE id = p_shipment_id;
    SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
END $$

DELIMITER ;
