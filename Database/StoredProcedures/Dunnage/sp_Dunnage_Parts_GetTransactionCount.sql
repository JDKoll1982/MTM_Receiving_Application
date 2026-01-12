DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Parts_GetTransactionCount` $$

CREATE PROCEDURE `sp_Dunnage_Parts_GetTransactionCount`(
    IN p_part_id VARCHAR(100),
    OUT p_count INT
)
BEGIN
    -- Get count of transaction records referencing this part
    SELECT COUNT(*) INTO p_count
    FROM dunnage_history
    WHERE part_id = p_part_id;
END $$

DELIMITER ;
