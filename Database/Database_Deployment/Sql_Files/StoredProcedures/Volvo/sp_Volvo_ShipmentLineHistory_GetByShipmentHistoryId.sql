-- =====================================================
-- Stored Procedure: sp_Volvo_ShipmentLineHistory_GetByShipmentHistoryId
-- =====================================================
-- Purpose: Get archived Volvo shipment lines for a specific archived header
-- Database: mtm_receiving_application
-- =====================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Volvo_ShipmentLineHistory_GetByShipmentHistoryId`$$

CREATE PROCEDURE `sp_Volvo_ShipmentLineHistory_GetByShipmentHistoryId`(
  IN p_shipment_history_id INT
)
BEGIN
  SELECT
    id,
    original_shipment_id AS shipment_id,
    part_number,
    po_status,
    location,
    quantity_per_skid,
    received_skid_count,
    calculated_piece_count,
    has_discrepancy,
    expected_skid_count,
    discrepancy_note
  FROM volvo_line_history
  WHERE shipment_history_id = p_shipment_history_id
  ORDER BY id;
END $$

DELIMITER ;