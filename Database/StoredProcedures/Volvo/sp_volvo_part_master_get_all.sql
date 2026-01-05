-- =====================================================
-- Stored Procedure: sp_volvo_part_master_get_all
-- =====================================================
-- Purpose: Get all active Volvo parts for dropdown
-- Database: mtm_receiving_application
-- =====================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS sp_volvo_part_master_get_all$$

CREATE PROCEDURE sp_volvo_part_master_get_all(
  IN p_include_inactive TINYINT(1)
)
BEGIN
  SELECT part_number, quantity_per_skid, is_active, created_date, modified_date
  FROM volvo_parts_master
  WHERE (p_include_inactive = 1 OR is_active = 1)
  ORDER BY part_number;
END$$

DELIMITER ;
