-- =====================================================
-- Stored Procedure: sp_volvo_part_component_get
-- =====================================================
-- Purpose: Get components for a parent part
-- Database: mtm_receiving_application
-- =====================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS sp_volvo_part_component_get$$

CREATE PROCEDURE sp_volvo_part_component_get(
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
