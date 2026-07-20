DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Inventory_Insert`$$

CREATE PROCEDURE `sp_Dunnage_Inventory_Insert`(
    IN p_part_id VARCHAR(50),
    IN p_inventory_method VARCHAR(100),
    IN p_notes TEXT,
    IN p_user VARCHAR(50),
    OUT p_new_id INT
)
BEGIN
    DECLARE v_old_foreign_key_checks INT DEFAULT @@FOREIGN_KEY_CHECKS;
    SET FOREIGN_KEY_CHECKS = 0;
    INSERT INTO dunnage_requires_inventory (
        part_id,
        inventory_method,
        notes,
        created_by,
        created_date
    ) VALUES (
        p_part_id,
        p_inventory_method,
        p_notes,
        p_user,
        NOW()
    );

    SET p_new_id = LAST_INSERT_ID();
    SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
END $$

DELIMITER ;
