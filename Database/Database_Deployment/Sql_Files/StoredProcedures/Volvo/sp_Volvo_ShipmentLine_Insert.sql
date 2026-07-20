-- =====================================================
-- Stored Procedure: sp_Volvo_ShipmentLine_Insert
-- =====================================================
-- Purpose: Insert shipment line with calculated piece count
-- Database: mtm_receiving_application
-- =====================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Volvo_ShipmentLine_Insert`$$

CREATE PROCEDURE `sp_Volvo_ShipmentLine_Insert`(
  IN p_shipment_id INT,
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
  INSERT INTO volvo_line_data (
    shipment_id, part_number, po_status, location, quantity_per_skid, received_skid_count, calculated_piece_count,
    has_discrepancy, expected_skid_count, discrepancy_note
  ) VALUES (
    p_shipment_id, p_part_number, COALESCE(NULLIF(TRIM(p_po_status), ''), 'Pending'), NULLIF(TRIM(p_location), ''), p_quantity_per_skid, p_received_skid_count, p_calculated_piece_count,
    p_has_discrepancy, p_expected_skid_count, p_discrepancy_note
  );
    SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
END $$

DELIMITER ;
