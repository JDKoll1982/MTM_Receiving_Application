-- Stored Procedure: sp_Receiving_ScannerProfile_Delete
-- Description: Deletes one scanner profile for a user.

USE mtm_receiving_application_test;

DROP PROCEDURE IF EXISTS `sp_Receiving_ScannerProfile_Delete`;

DELIMITER $$

CREATE PROCEDURE `sp_Receiving_ScannerProfile_Delete`(
    IN p_id CHAR(36),
    IN p_user_id VARCHAR(100)
)
BEGIN
    DELETE FROM receiving_scanner_profile
    WHERE id = p_id
      AND user_id = p_user_id;
END $$

DELIMITER ;
