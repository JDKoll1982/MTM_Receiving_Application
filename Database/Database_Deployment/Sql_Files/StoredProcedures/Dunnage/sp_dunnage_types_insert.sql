-- =============================================
-- Stored Procedure: sp_Dunnage_Types_Insert
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Insert Dunnage Type
-- Parameters aligned with all other Dunnage insert SPs:
--   p_user (not p_created_by) and OUT p_new_id (not SELECT result set).
-- =============================================
DROP PROCEDURE IF EXISTS `sp_Dunnage_Types_Insert`$$
CREATE PROCEDURE `sp_Dunnage_Types_Insert`(
    IN  p_type_name VARCHAR(100),
    IN  p_icon      VARCHAR(50),
    IN  p_user      VARCHAR(50),
    OUT p_new_id    INT
)
BEGIN
    -- Check for duplicate type_name (SP-level guard against race conditions)
    IF EXISTS (SELECT 1 FROM dunnage_types WHERE type_name = p_type_name) THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Dunnage type name already exists';
    END IF;

    INSERT INTO dunnage_types (type_name, icon, created_by, created_date)
    VALUES (p_type_name, p_icon, p_user, NOW());

    SET p_new_id = LAST_INSERT_ID();
END $$



DELIMITER ;
