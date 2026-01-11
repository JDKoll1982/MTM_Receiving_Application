DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Types_CountTransactions`$$

CREATE PROCEDURE `sp_Dunnage_Types_CountTransactions`(
    IN p_type_id INT
)
BEGIN
    SELECT COUNT(*) as transaction_count
    FROM dunnage_history l
    JOIN dunnage_parts p ON l.part_id = p.part_id
    WHERE p.type_id = p_type_id;
END$$

DELIMITER ;
