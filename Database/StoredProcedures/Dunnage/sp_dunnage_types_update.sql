-- =============================================
-- Stored Procedure: sp_Dunnage_Types_Update
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Update Dunnage Type
-- =============================================
DROP PROCEDURE IF EXISTS `sp_Dunnage_Types_Update`$$
CREATE PROCEDURE `sp_Dunnage_Types_Update`(
    IN p_id INT,
    IN p_type_name VARCHAR(100),
    IN p_icon VARCHAR(50),
    IN p_modified_by VARCHAR(50)
)
BEGIN
    -- Check for duplicate type_name (excluding self)
    IF EXISTS (SELECT 1 FROM dunnage_types WHERE type_name = p_type_name AND id != p_id) THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Dunnage type name already exists';
    END IF;

    UPDATE dunnage_types
    SET type_name = p_type_name,
        icon = p_icon,
        modified_by = p_modified_by,
        modified_date = CURRENT_TIMESTAMP
    WHERE id = p_id;

    SELECT ROW_COUNT() AS affected_rows;
END$$



DELIMITER ;
