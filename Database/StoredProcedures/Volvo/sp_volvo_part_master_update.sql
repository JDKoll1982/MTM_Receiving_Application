-- =====================================================
-- Stored Procedure: sp_volvo_part_master_update
-- =====================================================
-- Purpose: Update an existing Volvo part
-- Database: mtm_receiving_application
-- =====================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS sp_volvo_part_master_update$$

CREATE PROCEDURE sp_volvo_part_master_update(
  IN p_part_number VARCHAR(20),
  IN p_description VARCHAR(200),
  IN p_quantity_per_skid INT
)
BEGIN
  UPDATE volvo_parts_master
  SET 
    description = p_description,
    quantity_per_skid = p_quantity_per_skid,
    modified_date = CURRENT_TIMESTAMP
  WHERE part_number = p_part_number;
END$$

DELIMITER ;
