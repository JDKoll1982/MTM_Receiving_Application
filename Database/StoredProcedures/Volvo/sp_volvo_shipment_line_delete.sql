-- =====================================================
-- Stored Procedure: sp_volvo_shipment_line_delete
-- =====================================================
-- Purpose: Delete a shipment line
-- Database: mtm_receiving_application
-- =====================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS sp_volvo_shipment_line_delete$$

CREATE PROCEDURE sp_volvo_shipment_line_delete(
  IN p_id INT
)
BEGIN
  DELETE FROM volvo_shipment_lines
  WHERE id = p_id;
END$$

DELIMITER ;
