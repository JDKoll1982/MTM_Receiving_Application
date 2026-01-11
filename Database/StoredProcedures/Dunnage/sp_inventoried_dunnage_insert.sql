DELIMITER $$

DROP PROCEDURE IF EXISTS sp_dunnage_requires_inventory_insert$$

CREATE PROCEDURE sp_dunnage_requires_inventory_insert(
    IN p_part_id VARCHAR(50),
    IN p_inventory_method VARCHAR(100),
    IN p_notes TEXT,
    IN p_user VARCHAR(50),
    OUT p_new_id INT
)
BEGIN
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
END$$

DELIMITER ;
