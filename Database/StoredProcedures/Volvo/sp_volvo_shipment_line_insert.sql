-- =====================================================
-- Stored Procedure: sp_volvo_shipment_line_insert
-- =====================================================
-- Purpose: Insert shipment line with calculated piece count
-- Database: mtm_receiving_application
-- =====================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS sp_volvo_shipment_line_insert$$

CREATE PROCEDURE sp_volvo_shipment_line_insert(
  IN p_shipment_id INT,
  IN p_part_number VARCHAR(20),
  IN p_quantity_per_skid INT,
  IN p_received_skid_count INT,
  IN p_calculated_piece_count INT,
  IN p_has_discrepancy TINYINT(1),
  IN p_expected_skid_count INT,
  IN p_discrepancy_note TEXT
)
BEGIN
  INSERT INTO volvo_shipment_lines (
    shipment_id, part_number, quantity_per_skid, received_skid_count, calculated_piece_count,
    has_discrepancy, expected_skid_count, discrepancy_note
  ) VALUES (
    p_shipment_id, p_part_number, p_quantity_per_skid, p_received_skid_count, p_calculated_piece_count,
    p_has_discrepancy, p_expected_skid_count, p_discrepancy_note
  );
END$$

DELIMITER ;
