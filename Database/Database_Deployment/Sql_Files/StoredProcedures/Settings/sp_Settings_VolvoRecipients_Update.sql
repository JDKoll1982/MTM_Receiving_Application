DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Settings_VolvoRecipients_Update`$$
CREATE PROCEDURE `sp_Settings_VolvoRecipients_Update`(
    IN p_id INT,
    IN p_first_name VARCHAR(100),
    IN p_last_name VARCHAR(100),
    IN p_recipient_type VARCHAR(10),
    IN p_email VARCHAR(255)
)
BEGIN
    UPDATE settings_volvo_recipents
    SET
        first_name = p_first_name,
        last_name = p_last_name,
        recipient_type = p_recipient_type,
        email = p_email
    WHERE id = p_id;
END $$

DELIMITER ;