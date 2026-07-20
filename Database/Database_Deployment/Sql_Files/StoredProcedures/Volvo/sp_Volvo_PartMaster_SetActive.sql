-- =====================================================
-- Stored Procedure: sp_Volvo_PartMaster_SetActive
-- =====================================================
-- Purpose: Activate or deactivate a Volvo part
-- Database: mtm_receiving_application
-- =====================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Volvo_PartMaster_SetActive`$$

CREATE PROCEDURE `sp_Volvo_PartMaster_SetActive`(
  IN p_part_number VARCHAR(20),
  IN p_is_active TINYINT(1)
)
BEGIN
    DECLARE v_old_foreign_key_checks INT DEFAULT @@FOREIGN_KEY_CHECKS;
    SET FOREIGN_KEY_CHECKS = 0;
  UPDATE volvo_masterdata
  SET
    is_active = p_is_active,
    modified_date = CURRENT_TIMESTAMP
  WHERE part_number = p_part_number;
    SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
END $$

DELIMITER ;
