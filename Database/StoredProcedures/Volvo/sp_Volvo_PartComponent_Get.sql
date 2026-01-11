-- =====================================================
-- Stored Procedure: sp_Volvo_PartComponent_Get
-- =====================================================
-- Purpose: Get components for a parent part
-- Database: mtm_receiving_application
-- =====================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Volvo_PartComponent_Get`$$

CREATE PROCEDURE `sp_Volvo_PartComponent_Get`(
  IN p_parent_part_number VARCHAR(20)
)
BEGIN
  SELECT
    c.component_part_number,
    c.quantity,
    p.quantity_per_skid as component_quantity_per_skid
  FROM volvo_part_components c
  INNER JOIN volvo_masterdata p ON c.component_part_number = p.part_number
  WHERE c.parent_part_number = p_parent_part_number;
END$$

DELIMITER ;
