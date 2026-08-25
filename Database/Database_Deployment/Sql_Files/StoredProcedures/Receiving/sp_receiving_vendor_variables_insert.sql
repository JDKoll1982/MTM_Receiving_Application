DELIMITER $$

DROP PROCEDURE IF EXISTS sp_receiving_vendor_variables_insert$$
CREATE PROCEDURE sp_receiving_vendor_variables_insert(
	IN p_vendor_name VARCHAR(100),
	IN p_variable_name VARCHAR(100)
)
BEGIN
    DECLARE v_old_foreign_key_checks INT DEFAULT @@FOREIGN_KEY_CHECKS;
    SET FOREIGN_KEY_CHECKS = 0;
	INSERT INTO mtm_receiving_application_test.receiving_vendor_variables (
		vendor_name,
		variable_name
	)
	VALUES (
		p_vendor_name,
		p_variable_name
	);
    SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
END $$

DELIMITER ;
