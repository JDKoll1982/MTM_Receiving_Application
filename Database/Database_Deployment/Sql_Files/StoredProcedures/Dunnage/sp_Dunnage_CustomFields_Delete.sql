DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_CustomFields_Delete` $$

CREATE PROCEDURE `sp_Dunnage_CustomFields_Delete`(
    IN p_field_id INT
)
BEGIN
    DELETE FROM dunnage_custom_fields
    WHERE ID = p_field_id;
END $$

DELIMITER ;