DELIMITER $$

DROP PROCEDURE IF EXISTS sp_dunnage_types_count_transactions$$

CREATE PROCEDURE sp_dunnage_types_count_transactions(
    IN p_type_id INT
)
BEGIN
    SELECT COUNT(*) as transaction_count
    FROM dunnage_loads l
    JOIN dunnage_parts p ON l.part_id = p.part_id
    WHERE p.type_id = p_type_id;
END$$

DELIMITER ;
