-- =====================================================
-- Stored Procedure: sp_Volvo_PartComponent_DeleteByParent
-- =====================================================
-- Purpose: Delete all components for a parent part
-- Database: mtm_receiving_application
-- =====================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Volvo_PartComponent_DeleteByParent`$$

CREATE PROCEDURE `sp_Volvo_PartComponent_DeleteByParent`(
  IN p_parent_part_number VARCHAR(20)
)
BEGIN
  DELETE FROM volvo_part_components
  WHERE parent_part_number = p_parent_part_number;
END$$

DELIMITER ;
