DELIMITER $$

DROP PROCEDURE IF EXISTS sp_receiving_vendor_variables_get_by_vendor$$
CREATE PROCEDURE sp_receiving_vendor_variables_get_by_vendor(IN p_vendor_name VARCHAR(100))
BEGIN
	SELECT
		vendor_name,
		variable_name,
		created_at,
		updated_at,
		created_by
	FROM mtm_receiving_application.receiving_vendor_variables
	WHERE vendor_name = p_vendor_name;
END $$

DELIMITER ;
