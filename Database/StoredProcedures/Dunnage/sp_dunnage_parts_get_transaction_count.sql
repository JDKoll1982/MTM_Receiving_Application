DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_dunnage_parts_get_transaction_count` $$

CREATE PROCEDURE `sp_dunnage_parts_get_transaction_count`(
    IN p_part_id VARCHAR(100),
    OUT p_count INT
)
BEGIN
    -- Get count of transaction records referencing this part
    SELECT COUNT(*) INTO p_count
    FROM dunnage_history
    WHERE PartID = p_part_id;
END $$

DELIMITER ;
