DELIMITER $$

DROP PROCEDURE IF EXISTS sp_receiving_vendor_variables_get_all$$
CREATE PROCEDURE sp_receiving_vendor_variables_get_all()
BEGIN
	SELECT
		vendor_name,
		variable_name,
		created_at,
		updated_at,
		created_by
	FROM mtm_receiving_application_test.receiving_vendor_variables
	ORDER BY vendor_name;
END $$

DELIMITER ;
