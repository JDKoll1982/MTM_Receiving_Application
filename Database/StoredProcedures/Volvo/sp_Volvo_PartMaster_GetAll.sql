-- =====================================================
-- Stored Procedure: sp_Volvo_PartMaster_GetAll
-- =====================================================
-- Purpose: Get all active Volvo parts for dropdown
-- Database: mtm_receiving_application
-- =====================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Volvo_PartMaster_GetAll`$$

CREATE PROCEDURE `sp_Volvo_PartMaster_GetAll`(
  IN p_include_inactive TINYINT(1)
)
BEGIN
  SELECT part_number, quantity_per_skid, is_active, created_date, modified_date
  FROM volvo_masterdata
  WHERE (p_include_inactive = 1 OR is_active = 1)
  ORDER BY part_number;
END$$

DELIMITER ;
