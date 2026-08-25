DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_CustomFieldChoices_GetByField` $$

CREATE PROCEDURE `sp_Dunnage_CustomFieldChoices_GetByField`(
    IN p_custom_field_id INT
)
BEGIN
    SELECT ID, CustomFieldID, Choice, SortOrder
    FROM dunnage_custom_field_choices
    WHERE CustomFieldID = p_custom_field_id
    ORDER BY SortOrder;
END $$

DELIMITER ;
