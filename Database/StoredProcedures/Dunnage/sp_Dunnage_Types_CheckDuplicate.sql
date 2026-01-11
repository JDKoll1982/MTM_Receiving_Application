DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Types_CheckDuplicate` $$

CREATE PROCEDURE `sp_Dunnage_Types_CheckDuplicate`(
    IN p_type_name VARCHAR(100),
    IN p_exclude_id INT,
    OUT p_exists BOOLEAN
)
BEGIN
    -- Check if duplicate type name exists (excluding specified ID)
    IF EXISTS (
        SELECT 1 FROM dunnage_types
        WHERE DunnageType = p_type_name
        AND (p_exclude_id IS NULL OR ID != p_exclude_id)
    ) THEN
        SET p_exists = TRUE;
    ELSE
        SET p_exists = FALSE;
    END IF;
END $$

DELIMITER ;
