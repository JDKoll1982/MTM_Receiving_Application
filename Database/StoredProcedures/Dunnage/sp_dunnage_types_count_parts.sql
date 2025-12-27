DELIMITER $$

DROP PROCEDURE IF EXISTS sp_dunnage_types_count_parts$$

CREATE PROCEDURE sp_dunnage_types_count_parts(
    IN p_type_id INT
)
BEGIN
    SELECT COUNT(*) as part_count
    FROM dunnage_parts
    WHERE type_id = p_type_id;
END$$

DELIMITER ;
