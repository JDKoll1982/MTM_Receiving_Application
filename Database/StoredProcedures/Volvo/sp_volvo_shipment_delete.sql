-- =====================================================
-- Stored Procedure: sp_volvo_shipment_delete
-- =====================================================
-- Purpose: Delete a shipment and all its lines (CASCADE)
-- Database: mtm_receiving_application
-- =====================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS sp_volvo_shipment_delete$$

CREATE PROCEDURE sp_volvo_shipment_delete(
  IN p_shipment_id INT
)
BEGIN
  -- Lines will be deleted automatically by CASCADE DELETE
  DELETE FROM volvo_label_data
  WHERE id = p_shipment_id;
END$$

DELIMITER ;
