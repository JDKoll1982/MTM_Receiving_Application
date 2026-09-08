-- =====================================================
-- Stored Procedure: sp_Volvo_PartMaster_Update
-- =====================================================
-- Purpose: Update an existing Volvo part
-- Database: mtm_receiving_application
-- =====================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Volvo_PartMaster_Update`$$

CREATE PROCEDURE `sp_Volvo_PartMaster_Update`(
  IN p_part_number VARCHAR(20),
  IN p_quantity_per_skid INT
)
BEGIN
    DECLARE v_old_foreign_key_checks INT DEFAULT @@FOREIGN_KEY_CHECKS;
    SET FOREIGN_KEY_CHECKS = 0;
  UPDATE volvo_masterdata
  SET
    quantity_per_skid = p_quantity_per_skid,
    modified_date = CURRENT_TIMESTAMP
  WHERE part_number = p_part_number;
    SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
END $$

DELIMITER ;
