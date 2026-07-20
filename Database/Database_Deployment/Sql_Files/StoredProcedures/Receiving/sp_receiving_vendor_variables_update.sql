DELIMITER $$

DROP PROCEDURE IF EXISTS sp_receiving_vendor_variables_update$$
CREATE PROCEDURE sp_receiving_vendor_variables_update(
	IN p_vendor_name VARCHAR(100),
	IN p_variable_name VARCHAR(100)
)
BEGIN
    SET FOREIGN_KEY_CHECKS = 0;
	UPDATE mtm_receiving_application.receiving_vendor_variables
	SET
		variable_name = p_variable_name,
		updated_at = CURRENT_TIMESTAMP
	WHERE vendor_name = p_vendor_name;
    SET FOREIGN_KEY_CHECKS = 1;
END $$

DELIMITER ;
