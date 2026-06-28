DELIMITER $$

DROP PROCEDURE IF EXISTS sp_receiving_vendor_variables_delete$$
CREATE PROCEDURE sp_receiving_vendor_variables_delete(IN p_vendor_name VARCHAR(100))
BEGIN
	DELETE FROM mtm_receiving_application.receiving_vendor_variables
	WHERE vendor_name = p_vendor_name;
END $$

DELIMITER ;
