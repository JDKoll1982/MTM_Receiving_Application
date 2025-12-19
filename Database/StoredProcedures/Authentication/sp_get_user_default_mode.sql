DELIMITER $$

DROP PROCEDURE IF EXISTS sp_get_user_default_mode$$

CREATE PROCEDURE sp_get_user_default_mode(
    IN p_user_id INT
)
BEGIN
    -- Retrieve user's default receiving mode
    
    SELECT default_receiving_mode
    FROM users
    WHERE id = p_user_id;
END$$

DELIMITER ;
