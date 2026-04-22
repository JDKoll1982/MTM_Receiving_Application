DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Settings_VolvoRecipients_GetAll`$$
CREATE PROCEDURE `sp_Settings_VolvoRecipients_GetAll`()
BEGIN
    SELECT
        id,
        first_name,
        last_name,
        recipient_type,
        email
    FROM settings_volvo_recipents
    ORDER BY FIELD(recipient_type, 'To', 'CC'), id;
END $$

DELIMITER ;