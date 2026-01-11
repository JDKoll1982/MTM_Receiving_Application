DELIMITER $$

DROP PROCEDURE IF EXISTS sp_dunnage_parts_count_transactions$$

CREATE PROCEDURE sp_dunnage_parts_count_transactions(
    IN p_part_id VARCHAR(50)
)
BEGIN
    SELECT COUNT(*) as transaction_count
    FROM dunnage_history
    WHERE part_id = p_part_id;
END$$

DELIMITER ;
