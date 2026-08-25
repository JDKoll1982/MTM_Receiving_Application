DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_CustomFieldChoices_DeleteByField` $$

CREATE PROCEDURE `sp_Dunnage_CustomFieldChoices_DeleteByField`(
    IN p_custom_field_id INT
)
BEGIN
    DELETE FROM dunnage_custom_field_choices
    WHERE CustomFieldID = p_custom_field_id;
END $$

DELIMITER ;
