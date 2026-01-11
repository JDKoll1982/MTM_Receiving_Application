-- =====================================================
-- Stored Procedure: sp_Volvo_PartMaster_GetById
-- =====================================================
-- Purpose: Get a specific part by part number
-- Database: mtm_receiving_application
-- =====================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Volvo_PartMaster_GetById`$$

CREATE PROCEDURE `sp_Volvo_PartMaster_GetById`(
  IN p_part_number VARCHAR(20)
)
BEGIN
  SELECT part_number, quantity_per_skid, is_active, created_date, modified_date
  FROM volvo_masterdata
  WHERE part_number = p_part_number;
END$$

DELIMITER ;
