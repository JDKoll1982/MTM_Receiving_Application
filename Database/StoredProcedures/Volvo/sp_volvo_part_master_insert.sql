-- =====================================================
-- Stored Procedure: sp_volvo_part_master_insert
-- =====================================================
-- Purpose: Insert a new Volvo part
-- Database: mtm_receiving_application
-- =====================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS sp_volvo_part_master_insert$$

CREATE PROCEDURE sp_volvo_part_master_insert(
  IN p_part_number VARCHAR(20),
  IN p_quantity_per_skid INT,
  IN p_is_active TINYINT(1)
)
BEGIN
  INSERT INTO volvo_masterdata (part_number, quantity_per_skid, is_active)
  VALUES (p_part_number, p_quantity_per_skid, p_is_active);
END$$

DELIMITER ;
