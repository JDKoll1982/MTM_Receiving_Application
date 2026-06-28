DELIMITER $$

DROP PROCEDURE IF EXISTS sp_receiving_vendor_variables_insert$$
CREATE PROCEDURE sp_receiving_vendor_variables_insert(
	IN p_vendor_name VARCHAR(100),
	IN p_variable_name VARCHAR(100)
)
BEGIN
	INSERT INTO mtm_receiving_application.receiving_vendor_variables (
		vendor_name,
		variable_name
	)
	VALUES (
		p_vendor_name,
		p_variable_name
	);
END $$

DELIMITER ;
