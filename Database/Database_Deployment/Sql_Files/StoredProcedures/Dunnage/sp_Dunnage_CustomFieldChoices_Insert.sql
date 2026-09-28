DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_CustomFieldChoices_Insert` $$

CREATE PROCEDURE `sp_Dunnage_CustomFieldChoices_Insert`(
    IN p_custom_field_id INT,
    IN p_choice VARCHAR(255),
    IN p_sort_order INT
)
BEGIN
    INSERT INTO dunnage_custom_field_choices (CustomFieldID, Choice, SortOrder)
    VALUES (p_custom_field_id, p_choice, p_sort_order)
    ON DUPLICATE KEY UPDATE SortOrder = p_sort_order;
END $$

DELIMITER ;
