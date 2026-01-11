DELIMITER $$

DROP PROCEDURE IF EXISTS sp_dunnage_requires_inventory_delete$$

CREATE PROCEDURE sp_dunnage_requires_inventory_delete(
    IN p_id INT
)
BEGIN
    DELETE FROM dunnage_requires_inventory
    WHERE id = p_id;
END$$

DELIMITER ;
