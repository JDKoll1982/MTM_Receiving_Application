DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_custom_fields_insert` $$

CREATE PROCEDURE `sp_custom_fields_insert`(
    IN p_dunnage_type_id INT,
    IN p_field_name VARCHAR(100),
    IN p_database_column_name VARCHAR(64),
    IN p_field_type VARCHAR(20),
    IN p_display_order INT,
    IN p_is_required BOOLEAN,
    IN p_validation_rules TEXT,
    IN p_user VARCHAR(50),
    OUT p_new_id INT,
    OUT p_status INT,
    OUT p_error_msg VARCHAR(500)
)
BEGIN
    -- Error handler
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1 p_error_msg = MESSAGE_TEXT;
        SET p_status = -1;
        SET p_new_id = NULL;
        ROLLBACK;
    END;

    -- Start transaction
    START TRANSACTION;

    -- Check for duplicate DatabaseColumnName for same type
    IF EXISTS (
        SELECT 1 FROM custom_field_definitions 
        WHERE DunnageTypeID = p_dunnage_type_id 
        AND DatabaseColumnName = p_database_column_name
    ) THEN
        SET p_status = -1;
        SET p_error_msg = CONCAT('Column name "', p_database_column_name, '" already exists for this type');
        SET p_new_id = NULL;
        ROLLBACK;
    ELSE
        -- Insert custom field definition
        INSERT INTO custom_field_definitions (
            DunnageTypeID,
            FieldName,
            DatabaseColumnName,
            FieldType,
            DisplayOrder,
            IsRequired,
            ValidationRules,
            CreatedDate,
            CreatedBy
        ) VALUES (
            p_dunnage_type_id,
            p_field_name,
            p_database_column_name,
            p_field_type,
            p_display_order,
            p_is_required,
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
END $$

DELIMITER ;
