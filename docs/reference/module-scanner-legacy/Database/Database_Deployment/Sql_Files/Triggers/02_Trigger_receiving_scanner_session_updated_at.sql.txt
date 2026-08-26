-- Trigger: tr_receiving_scanner_session_bu
-- Description: Maintains updated_at for receiving_scanner_session.

USE mtm_receiving_application_test;

DROP TRIGGER IF EXISTS `tr_receiving_scanner_session_bu`;

DELIMITER $$

CREATE TRIGGER `tr_receiving_scanner_session_bu`
BEFORE UPDATE ON receiving_scanner_session
FOR EACH ROW
BEGIN
    SET NEW.updated_at = CURRENT_TIMESTAMP;
END $$

DELIMITER ;