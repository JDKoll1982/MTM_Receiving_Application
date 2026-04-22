DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Settings_VolvoRecipients_Delete`$$
CREATE PROCEDURE `sp_Settings_VolvoRecipients_Delete`(
    IN p_id INT
)
BEGIN
    DELETE FROM settings_volvo_recipents
    WHERE id = p_id;
END $$

DELIMITER ;