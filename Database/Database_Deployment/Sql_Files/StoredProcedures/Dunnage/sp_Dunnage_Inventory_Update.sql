DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Inventory_Update`$$

CREATE PROCEDURE `sp_Dunnage_Inventory_Update`(
    IN p_id INT,
    IN p_part_id VARCHAR(50),
    IN p_inventory_method VARCHAR(100),
    IN p_notes TEXT,
    IN p_user VARCHAR(50)
)
BEGIN
    DECLARE v_old_foreign_key_checks INT DEFAULT @@FOREIGN_KEY_CHECKS;
    SET FOREIGN_KEY_CHECKS = 0;
    UPDATE dunnage_requires_inventory
    SET
        part_id = p_part_id,
        inventory_method = p_inventory_method,
        notes = p_notes,
        modified_by = p_user,
        modified_date = NOW()
    WHERE id = p_id;
    SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
END $$

DELIMITER ;
