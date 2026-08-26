-- Stored Procedure: sp_Receiving_ScannerProfile_SetDefault
-- Description: Marks one scanner profile as the user's default.

USE mtm_receiving_application_test;

DROP PROCEDURE IF EXISTS `sp_Receiving_ScannerProfile_SetDefault`;

DELIMITER $$

CREATE PROCEDURE `sp_Receiving_ScannerProfile_SetDefault`(
    IN p_id CHAR(36),
    IN p_user_id VARCHAR(100)
)
BEGIN
    UPDATE receiving_scanner_profile
    SET is_default = CASE WHEN id = p_id THEN 1 ELSE 0 END,
        updated_at = CURRENT_TIMESTAMP
    WHERE user_id = p_user_id;
END $$

DELIMITER ;
