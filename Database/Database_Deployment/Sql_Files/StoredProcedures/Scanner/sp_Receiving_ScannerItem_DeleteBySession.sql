-- Stored Procedure: sp_Receiving_ScannerItem_DeleteBySession
-- Description: Clears session items, typically before reload/import.

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Receiving_ScannerItem_DeleteBySession`;

DELIMITER $$

CREATE PROCEDURE `sp_Receiving_ScannerItem_DeleteBySession`(
    IN p_session_id CHAR(36)
)
BEGIN
    DELETE FROM receiving_scanner_item
    WHERE session_id = p_session_id;
END $$

DELIMITER ;