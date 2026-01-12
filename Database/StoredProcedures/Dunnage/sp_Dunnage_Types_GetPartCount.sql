DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Types_GetPartCount` $$

CREATE PROCEDURE `sp_Dunnage_Types_GetPartCount`(
    IN p_type_id INT,
    OUT p_count INT
)
BEGIN
    -- Get count of parts using this dunnage type
    SELECT COUNT(*) INTO p_count
    FROM dunnage_parts
    WHERE type_id = p_type_id;
END $$

DELIMITER ;
