DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Parts_CountTransactions`$$

CREATE PROCEDURE `sp_Dunnage_Parts_CountTransactions`(
    IN p_part_id VARCHAR(50)
)
BEGIN
    SELECT COUNT(*) as transaction_count
    FROM dunnage_history
    WHERE part_id = p_part_id;
END$$

DELIMITER ;
