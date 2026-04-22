DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Settings_ReportingRecipients_Insert`$$
CREATE PROCEDURE `sp_Settings_ReportingRecipients_Insert`(
    IN p_first_name VARCHAR(100),
    IN p_last_name VARCHAR(100),
    IN p_recipient_type VARCHAR(10),
    IN p_email VARCHAR(255)
)
BEGIN
    INSERT INTO settings_reporting_recipients (
        first_name,
        last_name,
        recipient_type,
        email
    )
    VALUES (
        p_first_name,
        p_last_name,
        p_recipient_type,
        p_email
    );
END $$

DELIMITER ;