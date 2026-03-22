DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_CustomFields_GetByType` $$

CREATE PROCEDURE `sp_Dunnage_CustomFields_GetByType`(
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
