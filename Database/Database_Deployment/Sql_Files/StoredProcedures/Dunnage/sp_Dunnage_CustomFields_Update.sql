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
    DECLARE v_type_id INT DEFAULT NULL;
    DECLARE v_slot INT DEFAULT NULL;
    DECLARE v_old_default VARCHAR(255) DEFAULT NULL;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
        RESIGNAL;
    END;

    SELECT DunnageTypeID, DisplayOrder, DefaultValue
      INTO v_type_id, v_slot, v_old_default
      FROM dunnage_custom_fields
     WHERE ID = p_field_id;

    SET FOREIGN_KEY_CHECKS = 0;

    START TRANSACTION;

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

    -- Propagate a default-value change to existing saved label-data and history
    -- rows that currently hold the OLD default value. Rows whose value was
    -- overridden by the user are left untouched.
    IF v_type_id IS NOT NULL
       AND v_slot IS NOT NULL
       AND COALESCE(v_old_default, '') <> COALESCE(p_default_value, '') THEN

        SET @new_default := NULLIF(p_default_value, '');
        SET @old_default := COALESCE(v_old_default, '');
        SET @type_id := v_type_id;
        SET @slot := v_slot;

        SET @sql := CONCAT(
            'UPDATE dunnage_history h ',
            'JOIN dunnage_parts p ON p.part_id = h.part_id ',
            'SET h.udc', @slot, ' = ? ',
            'WHERE p.type_id = ? AND COALESCE(h.udc', @slot, ', '''') = ?'
        );
        PREPARE stmt FROM @sql;
        EXECUTE stmt USING @new_default, @type_id, @old_default;
        DEALLOCATE PREPARE stmt;

        SET @sql := CONCAT(
            'UPDATE dunnage_label_data d ',
            'JOIN dunnage_parts p ON p.part_id = d.part_id ',
            'SET d.udc', @slot, ' = ? ',
            'WHERE p.type_id = ? AND COALESCE(d.udc', @slot, ', '''') = ?'
        );
        PREPARE stmt FROM @sql;
        EXECUTE stmt USING @new_default, @type_id, @old_default;
        DEALLOCATE PREPARE stmt;
    END IF;

    COMMIT;
    SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
END $$

DELIMITER ;
