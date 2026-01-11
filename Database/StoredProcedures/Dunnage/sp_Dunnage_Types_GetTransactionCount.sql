DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Types_GetTransactionCount` $$

CREATE PROCEDURE `sp_Dunnage_Types_GetTransactionCount`(
    IN p_type_id INT,
    OUT p_count INT
)
BEGIN
    -- Get count of transaction records referencing this dunnage type
    SELECT COUNT(*) INTO p_count
    FROM dunnage_history
    WHERE DunnageTypeID = p_type_id;
END $$

DELIMITER ;
