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
    DECLARE v_old_foreign_key_checks INT DEFAULT @@FOREIGN_KEY_CHECKS;
    SET FOREIGN_KEY_CHECKS = 0;
  DELETE FROM volvo_part_components
  WHERE parent_part_number = p_parent_part_number;
    SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
END $$

DELIMITER ;
