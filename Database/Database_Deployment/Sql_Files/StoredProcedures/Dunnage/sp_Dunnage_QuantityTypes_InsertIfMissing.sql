USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Dunnage_QuantityTypes_InsertIfMissing`;

DELIMITER $$

CREATE PROCEDURE `sp_Dunnage_QuantityTypes_InsertIfMissing`(
    IN p_quantity_type VARCHAR(100),
    IN p_user VARCHAR(50)
)
BEGIN
    SET FOREIGN_KEY_CHECKS = 0;
    IF p_quantity_type IS NOT NULL AND TRIM(p_quantity_type) <> '' THEN
        INSERT IGNORE INTO dunnage_quantity_types (
            quantity_type,
            created_by,
            created_date
        ) VALUES (
            TRIM(p_quantity_type),
            COALESCE(NULLIF(TRIM(p_user), ''), 'SYSTEM'),
            NOW()
        );
    END IF;
    SET FOREIGN_KEY_CHECKS = 1;
END $$

DELIMITER ;
