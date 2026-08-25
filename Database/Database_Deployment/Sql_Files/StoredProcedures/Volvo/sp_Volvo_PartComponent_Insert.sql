-- =====================================================
-- Stored Procedure: sp_Volvo_PartComponent_Insert
-- =====================================================
-- Purpose: Insert a component relationship
-- Database: mtm_receiving_application_test
-- =====================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Volvo_PartComponent_Insert`$$

CREATE PROCEDURE `sp_Volvo_PartComponent_Insert`(
  IN p_parent_part_number VARCHAR(20),
  IN p_component_part_number VARCHAR(20),
  IN p_quantity INT
)
BEGIN
    DECLARE v_old_foreign_key_checks INT DEFAULT @@FOREIGN_KEY_CHECKS;
    SET FOREIGN_KEY_CHECKS = 0;
  INSERT INTO volvo_part_components (parent_part_number, component_part_number, quantity)
  VALUES (p_parent_part_number, p_component_part_number, p_quantity);
    SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
END $$

DELIMITER ;
