-- =====================================================
-- Stored Procedure: sp_Volvo_ShipmentLine_Update
-- =====================================================
-- Purpose: Update an existing shipment line
-- Database: mtm_receiving_application
-- =====================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Volvo_ShipmentLine_Update` $$

CREATE PROCEDURE `sp_Volvo_ShipmentLine_Update`(
  IN p_id INT,
  IN p_part_number VARCHAR(20),
  IN p_po_status VARCHAR(20),
  IN p_location VARCHAR(50),
  IN p_quantity_per_skid INT,
  IN p_received_skid_count INT,
  IN p_calculated_piece_count INT,
  IN p_has_discrepancy TINYINT(1),
  IN p_expected_skid_count INT,
  IN p_discrepancy_note TEXT
)
BEGIN
    DECLARE v_old_foreign_key_checks INT DEFAULT @@FOREIGN_KEY_CHECKS;
    SET FOREIGN_KEY_CHECKS = 0;
  UPDATE volvo_line_data
  SET
    part_number = p_part_number,
    po_status = COALESCE(NULLIF(TRIM(p_po_status), ''), 'Pending'),
    location = NULLIF(TRIM(p_location), ''),
    quantity_per_skid = p_quantity_per_skid,
    received_skid_count = p_received_skid_count,
    calculated_piece_count = p_calculated_piece_count,
    has_discrepancy = p_has_discrepancy,
    expected_skid_count = p_expected_skid_count,
    discrepancy_note = p_discrepancy_note
  WHERE id = p_id;

  UPDATE volvo_line_history
  SET
    part_number = p_part_number,
    po_status = COALESCE(NULLIF(TRIM(p_po_status), ''), 'Pending'),
    location = NULLIF(TRIM(p_location), ''),
    quantity_per_skid = p_quantity_per_skid,
    received_skid_count = p_received_skid_count,
    calculated_piece_count = p_calculated_piece_count,
    has_discrepancy = p_has_discrepancy,
    expected_skid_count = p_expected_skid_count,
    discrepancy_note = p_discrepancy_note
  WHERE original_id = p_id;
    SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
END $$

DELIMITER;