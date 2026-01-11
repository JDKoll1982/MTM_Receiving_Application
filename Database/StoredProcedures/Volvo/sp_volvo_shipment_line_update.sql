-- =====================================================
-- Stored Procedure: sp_volvo_shipment_line_update
-- =====================================================
-- Purpose: Update an existing shipment line
-- Database: mtm_receiving_application
-- =====================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS sp_volvo_shipment_line_update$$

CREATE PROCEDURE sp_volvo_shipment_line_update(
  IN p_id INT,
  IN p_received_skid_count INT,
  IN p_calculated_piece_count INT,
  IN p_has_discrepancy TINYINT(1),
  IN p_expected_skid_count INT,
  IN p_discrepancy_note TEXT
)
BEGIN
  UPDATE volvo_line_data
  SET
    received_skid_count = p_received_skid_count,
    calculated_piece_count = p_calculated_piece_count,
    has_discrepancy = p_has_discrepancy,
    expected_skid_count = p_expected_skid_count,
    discrepancy_note = p_discrepancy_note
  WHERE id = p_id;
END$$

DELIMITER ;
