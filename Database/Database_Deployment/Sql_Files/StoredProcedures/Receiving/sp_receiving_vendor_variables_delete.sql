DELIMITER $$

DROP PROCEDURE IF EXISTS sp_receiving_vendor_variables_delete$$
CREATE PROCEDURE sp_receiving_vendor_variables_delete(IN p_vendor_name VARCHAR(100))
BEGIN
    DECLARE v_old_foreign_key_checks INT DEFAULT @@FOREIGN_KEY_CHECKS;
    SET FOREIGN_KEY_CHECKS = 0;
	DELETE FROM mtm_receiving_application.receiving_vendor_variables
	WHERE vendor_name = p_vendor_name;
    SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
END $$

DELIMITER ;
