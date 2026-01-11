DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_dunnage_custom_fields_get_by_type` $$

CREATE PROCEDURE `sp_dunnage_custom_fields_get_by_type`(
    IN p_dunnage_type_id INT
)
BEGIN
    -- Retrieve all custom fields for a dunnage type, ordered by DisplayOrder
    SELECT
        ID,
        DunnageTypeID,
        FieldName,
        DatabaseColumnName,
        FieldType,
        DisplayOrder,
        IsRequired,
        ValidationRules,
        CreatedDate,
        CreatedBy
    FROM dunnage_custom_fields
    WHERE DunnageTypeID = p_dunnage_type_id
    ORDER BY DisplayOrder;
END $$

DELIMITER ;
