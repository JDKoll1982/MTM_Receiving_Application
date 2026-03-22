DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Types_GetTransactionCount` $$

CREATE PROCEDURE `sp_Dunnage_Types_GetTransactionCount`(
    IN p_type_id INT,
    OUT p_count INT
)
BEGIN
    -- Get count of transaction records referencing this dunnage type
    -- Note: dunnage_history table doesn't have type_id column
    -- This would require joining through dunnage_parts
    SELECT COUNT(*) INTO p_count
    FROM dunnage_history dh
    INNER JOIN dunnage_parts dp ON dh.part_id = dp.part_id
    WHERE dp.type_id = p_type_id;
END $$

DELIMITER ;
