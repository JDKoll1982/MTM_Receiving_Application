DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Loads_Insert`$$

CREATE PROCEDURE `sp_Dunnage_Loads_Insert`(
    IN p_load_uuid CHAR(36),
    IN p_part_id VARCHAR(50),
    IN p_quantity DECIMAL(10,2),
    IN p_user VARCHAR(50)
)
BEGIN
    INSERT INTO dunnage_history (
        load_uuid,
        part_id,
        quantity,
        received_date,
        created_by,
        created_date
    ) VALUES (
        p_load_uuid,
        p_part_id,
        p_quantity,
        NOW(),
        p_user,
        NOW()
    );
END$$

DELIMITER ;
