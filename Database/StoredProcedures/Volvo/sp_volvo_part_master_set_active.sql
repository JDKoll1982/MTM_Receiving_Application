-- =====================================================
-- Stored Procedure: sp_volvo_part_master_set_active
-- =====================================================
-- Purpose: Activate or deactivate a Volvo part
-- Database: mtm_receiving_application
-- =====================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS sp_volvo_part_master_set_active$$

CREATE PROCEDURE sp_volvo_part_master_set_active(
  IN p_part_number VARCHAR(20),
  IN p_is_active TINYINT(1)
)
BEGIN
  UPDATE volvo_parts_master
  SET 
    is_active = p_is_active,
    modified_date = CURRENT_TIMESTAMP
  WHERE part_number = p_part_number;
END$$

DELIMITER ;
