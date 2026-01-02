-- Stored Procedure: sp_update_user_default_dunnage_mode
-- Purpose: Update user's default dunnage workflow mode preference
-- Parameters:
--   @p_user_id: Employee number (PK)
--   @p_default_mode: New default mode ("guided", "manual", "edit", or NULL)

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS sp_update_user_default_dunnage_mode;

DELIMITER //

CREATE PROCEDURE sp_update_user_default_dunnage_mode(
    IN p_user_id INT,
    IN p_default_mode VARCHAR(20)
)
BEGIN
    UPDATE users
    SET default_dunnage_mode = p_default_mode,
        modified_date = NOW()
    WHERE employee_number = p_user_id;
    
    SELECT ROW_COUNT() AS affected_rows;
END //

DELIMITER ;
