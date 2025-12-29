DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_dunnage_types_get_part_count` $$

CREATE PROCEDURE `sp_dunnage_types_get_part_count`(
    IN p_type_id INT,
    OUT p_count INT
)
BEGIN
    -- Get count of parts using this dunnage type
    SELECT COUNT(*) INTO p_count
    FROM dunnage_part_numbers
    WHERE DunnageTypeID = p_type_id;
END $$

DELIMITER ;
