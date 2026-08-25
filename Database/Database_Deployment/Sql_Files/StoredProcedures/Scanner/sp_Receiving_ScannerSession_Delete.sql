-- Stored Procedure: sp_Receiving_ScannerSession_Delete
-- Description: Deletes a scanner session and its child items.

USE mtm_receiving_application_test;

DROP PROCEDURE IF EXISTS `sp_Receiving_ScannerSession_Delete`;

DELIMITER $$

CREATE PROCEDURE `sp_Receiving_ScannerSession_Delete`(
    IN p_session_id CHAR(36),
    IN p_user_id VARCHAR(100)
)
BEGIN
    DELETE FROM receiving_scanner_session
    WHERE id = p_session_id
      AND user_id = p_user_id;
END $$

DELIMITER ;