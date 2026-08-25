DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_CustomFields_Insert` $$

CREATE PROCEDURE `sp_Dunnage_CustomFields_Insert`(
    IN p_dunnage_type_id INT,
    IN p_field_name VARCHAR(100),
    IN p_field_type VARCHAR(20),
    IN p_display_order INT,
    IN p_is_required BOOLEAN,
    IN p_unit VARCHAR(50),
    IN p_min_value DECIMAL(18,4),
    IN p_max_value DECIMAL(18,4),
    IN p_default_value VARCHAR(255),
    IN p_validation_rules TEXT,
    IN p_user VARCHAR(50),
    OUT p_new_id INT,
    OUT p_status INT,
    OUT p_error_msg VARCHAR(500)
)
BEGIN
    -- Error handler
    DECLARE v_old_foreign_key_checks INT DEFAULT @@FOREIGN_KEY_CHECKS;
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
        GET DIAGNOSTICS CONDITION 1 p_error_msg = MESSAGE_TEXT;
        SET p_status = -1;
        SET p_new_id = NULL;
        ROLLBACK;
    END;

    SET FOREIGN_KEY_CHECKS = 0;

    -- Start transaction
    START TRANSACTION;

    -- Validate UDC slot (1-10) and reject duplicates within the type
    IF p_display_order < 1 OR p_display_order > 10 THEN
        SET p_status = -1;
        SET p_error_msg = CONCAT('DisplayOrder must be between 1 and 10 (got ', p_display_order, ')');
        SET p_new_id = NULL;
        ROLLBACK;
    ELSEIF EXISTS (
        SELECT 1 FROM dunnage_custom_fields
        WHERE DunnageTypeID = p_dunnage_type_id
        AND DisplayOrder = p_display_order
    ) THEN
        SET p_status = -1;
        SET p_error_msg = CONCAT('A field already occupies slot ', p_display_order, ' for this type');
        SET p_new_id = NULL;
        ROLLBACK;
    ELSEIF EXISTS (
        SELECT 1 FROM dunnage_custom_fields
        WHERE DunnageTypeID = p_dunnage_type_id
        AND FieldName = p_field_name
    ) THEN
        SET p_status = -1;
        SET p_error_msg = CONCAT('Field name "', p_field_name, '" already exists for this type');
        SET p_new_id = NULL;
        ROLLBACK;
    ELSEIF (SELECT COUNT(*) FROM dunnage_custom_fields WHERE DunnageTypeID = p_dunnage_type_id) >= 10 THEN
        SET p_status = -1;
        SET p_error_msg = 'This type already has 10 custom fields (UDC limit)';
        SET p_new_id = NULL;
        ROLLBACK;
    ELSE
        -- Insert custom field definition
        INSERT INTO dunnage_custom_fields (
            DunnageTypeID,
            FieldName,
            FieldType,
            DisplayOrder,
            IsRequired,
            Unit,
            MinValue,
            `MaxValue`,
            DefaultValue,
            ValidationRules,
            CreatedDate,
            CreatedBy
        ) VALUES (
            p_dunnage_type_id,
            p_field_name,
            p_field_type,
            p_display_order,
            p_is_required,
            NULLIF(p_unit, ''),
            p_min_value,
            p_max_value,
            NULLIF(p_default_value, ''),
            p_validation_rules,
            NOW(),
            p_user
        );

        -- Get new ID
        SET p_new_id = LAST_INSERT_ID();
        SET p_status = 1;
        SET p_error_msg = 'Custom field created successfully';
        COMMIT;
    END IF;
    SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
END $$

DELIMITER ;