-- =====================================================
-- Stored Procedure: sp_volvo_part_component_delete_by_parent
-- =====================================================
-- Purpose: Delete all components for a parent part
-- Database: mtm_receiving_application
-- =====================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS sp_volvo_part_component_delete_by_parent$$

CREATE PROCEDURE sp_volvo_part_component_delete_by_parent(
  IN p_parent_part_number VARCHAR(20)
)
BEGIN
  DELETE FROM volvo_part_components
  WHERE parent_part_number = p_parent_part_number;
END$$

DELIMITER ;
