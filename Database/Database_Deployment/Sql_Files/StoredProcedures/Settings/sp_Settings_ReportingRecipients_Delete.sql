DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Settings_ReportingRecipients_Delete`$$
CREATE PROCEDURE `sp_Settings_ReportingRecipients_Delete`(
    IN p_id INT
)
BEGIN
    DELETE FROM settings_reporting_recipients
    WHERE id = p_id;
END $$

DELIMITER ;