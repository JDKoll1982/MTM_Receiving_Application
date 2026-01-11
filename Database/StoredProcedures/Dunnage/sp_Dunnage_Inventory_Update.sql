DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Inventory_Update`$$

CREATE PROCEDURE `sp_Dunnage_Inventory_Update`(
    IN p_id INT,
    IN p_inventory_method VARCHAR(100),
    IN p_notes TEXT,
    IN p_user VARCHAR(50)
)
BEGIN
    UPDATE dunnage_requires_inventory
    SET
        inventory_method = p_inventory_method,
        notes = p_notes,
        modified_by = p_user,
        modified_date = NOW()
    WHERE id = p_id;
END$$

DELIMITER ;
