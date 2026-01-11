DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Loads_InsertBatch`$$

CREATE PROCEDURE `sp_Dunnage_Loads_InsertBatch`(
    IN p_load_data JSON,
    IN p_user VARCHAR(50)
)
BEGIN
    DECLARE i INT DEFAULT 0;
    DECLARE count INT DEFAULT 0;
    DECLARE v_load_uuid CHAR(36);
    DECLARE v_part_id VARCHAR(50);
    DECLARE v_quantity DECIMAL(10,2);

    SET count = JSON_LENGTH(p_load_data);

    WHILE i < count DO
        SET v_load_uuid = JSON_UNQUOTE(JSON_EXTRACT(p_load_data, CONCAT('$[', i, '].load_uuid')));
        SET v_part_id = JSON_UNQUOTE(JSON_EXTRACT(p_load_data, CONCAT('$[', i, '].part_id')));
        SET v_quantity = JSON_EXTRACT(p_load_data, CONCAT('$[', i, '].quantity'));

        INSERT INTO dunnage_history (
            load_uuid,
            part_id,
            quantity,
            received_date,
            created_by,
            created_date
        ) VALUES (
            v_load_uuid,
            v_part_id,
            v_quantity,
            NOW(),
            p_user,
            NOW()
        );

        SET i = i + 1;
    END WHILE;
END$$

DELIMITER ;
