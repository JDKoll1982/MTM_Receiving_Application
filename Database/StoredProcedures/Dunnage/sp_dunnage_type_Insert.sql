-- =============================================
-- Stored Procedure: sp_dunnage_type_Insert
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Insert Dunnage Type
-- =============================================
DROP PROCEDURE IF EXISTS sp_dunnage_type_Insert$$
CREATE PROCEDURE sp_dunnage_type_Insert(
    IN p_type_name VARCHAR(100),
    IN p_icon VARCHAR(50),
    IN p_created_by VARCHAR(50)
)
BEGIN
    -- Check for duplicate type_name
    IF EXISTS (SELECT 1 FROM dunnage_types WHERE type_name = p_type_name) THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Dunnage type name already exists';
    END IF;

    INSERT INTO dunnage_types (type_name, icon, created_by, created_date)
    VALUES (p_type_name, p_icon, p_created_by, NOW());

    SELECT LAST_INSERT_ID() AS id;
END$$



DELIMITER ;
