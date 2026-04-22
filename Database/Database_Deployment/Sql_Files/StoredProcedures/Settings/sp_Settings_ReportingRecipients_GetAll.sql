DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Settings_ReportingRecipients_GetAll`$$
CREATE PROCEDURE `sp_Settings_ReportingRecipients_GetAll`()
BEGIN
    SELECT
        id,
        first_name,
        last_name,
        recipient_type,
        email
    FROM settings_reporting_recipients
    ORDER BY FIELD(recipient_type, 'To', 'CC'), id;
END $$

DELIMITER ;