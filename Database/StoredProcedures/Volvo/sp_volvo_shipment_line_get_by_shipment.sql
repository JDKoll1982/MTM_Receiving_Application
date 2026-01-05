-- =====================================================
-- Stored Procedure: sp_volvo_shipment_line_get_by_shipment
-- =====================================================
-- Purpose: Get all lines for a specific shipment
-- Database: mtm_receiving_application
-- =====================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS sp_volvo_shipment_line_get_by_shipment$$

CREATE PROCEDURE sp_volvo_shipment_line_get_by_shipment(
  IN p_shipment_id INT
)
BEGIN
  SELECT 
    id, shipment_id, part_number, received_skid_count, calculated_piece_count,
    has_discrepancy, expected_skid_count, discrepancy_note
  FROM volvo_shipment_lines
  WHERE shipment_id = p_shipment_id
  ORDER BY id;
END$$

DELIMITER ;
