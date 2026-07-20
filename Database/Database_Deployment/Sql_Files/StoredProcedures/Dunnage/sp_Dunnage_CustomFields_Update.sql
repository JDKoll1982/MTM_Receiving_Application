DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_CustomFields_Update` $$

CREATE PROCEDURE `sp_Dunnage_CustomFields_Update`(
    IN p_field_id INT,
    IN p_field_name VARCHAR(100),
    IN p_database_column_name VARCHAR(64),
    IN p_field_type VARCHAR(20),
    IN p_display_order INT,
    IN p_is_required BOOLEAN,
    IN p_validation_rules TEXT
)
BEGIN
    SET FOREIGN_KEY_CHECKS = 0;
    UPDATE dunnage_custom_fields
    SET
        FieldName = p_field_name,
        DatabaseColumnName = p_database_column_name,
        FieldType = p_field_type,
        DisplayOrder = p_display_order,
        IsRequired = p_is_required,
        ValidationRules = p_validation_rules
    WHERE ID = p_field_id;
    SET FOREIGN_KEY_CHECKS = 1;
END $$

DELIMITER ;
