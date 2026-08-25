DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_CustomFields_Update` $$

CREATE PROCEDURE `sp_Dunnage_CustomFields_Update`(
    IN p_field_id INT,
    IN p_field_name VARCHAR(100),
    IN p_field_type VARCHAR(20),
    IN p_display_order INT,
    IN p_is_required BOOLEAN,
    IN p_unit VARCHAR(50),
    IN p_min_value DECIMAL(18,4),
    IN p_max_value DECIMAL(18,4),
    IN p_default_value VARCHAR(255),
    IN p_validation_rules TEXT
)
BEGIN
    DECLARE v_old_foreign_key_checks INT DEFAULT @@FOREIGN_KEY_CHECKS;
    SET FOREIGN_KEY_CHECKS = 0;
    UPDATE dunnage_custom_fields
    SET
        FieldName = p_field_name,
        FieldType = p_field_type,
        DisplayOrder = p_display_order,
        IsRequired = p_is_required,
        Unit = NULLIF(p_unit, ''),
        MinValue = p_min_value,
        `MaxValue` = p_max_value,
        DefaultValue = NULLIF(p_default_value, ''),
        ValidationRules = p_validation_rules
    WHERE ID = p_field_id;
    SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
END $$

DELIMITER ;
