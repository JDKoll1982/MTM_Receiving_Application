-- =====================================================
-- Stored Procedure: sp_Volvo_ShipmentLine_GetByShipment
-- =====================================================
-- Purpose: Get all lines for a specific shipment
-- Database: mtm_receiving_application
-- =====================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Volvo_ShipmentLine_GetByShipment`$$

CREATE PROCEDURE `sp_Volvo_ShipmentLine_GetByShipment`(
  IN p_shipment_id INT
)
BEGIN
  SELECT
    id, shipment_id, part_number, quantity_per_skid, received_skid_count, calculated_piece_count,
    has_discrepancy, expected_skid_count, discrepancy_note
  FROM volvo_line_data
  WHERE shipment_id = p_shipment_id
  ORDER BY id;
END$$

DELIMITER ;
