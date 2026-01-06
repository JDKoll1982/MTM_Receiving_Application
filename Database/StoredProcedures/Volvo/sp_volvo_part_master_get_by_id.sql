-- =====================================================
-- Stored Procedure: sp_volvo_part_master_get_by_id
-- =====================================================
-- Purpose: Get a specific part by part number
-- Database: mtm_receiving_application
-- =====================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS sp_volvo_part_master_get_by_id$$

CREATE PROCEDURE sp_volvo_part_master_get_by_id(
  IN p_part_number VARCHAR(20)
)
BEGIN
  SELECT part_number, quantity_per_skid, is_active, created_date, modified_date
  FROM volvo_parts_master
  WHERE part_number = p_part_number;
END$$

DELIMITER ;
