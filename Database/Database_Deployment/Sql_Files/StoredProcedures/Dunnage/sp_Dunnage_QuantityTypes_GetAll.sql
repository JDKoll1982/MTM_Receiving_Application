USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Dunnage_QuantityTypes_GetAll`;

DELIMITER $$

CREATE PROCEDURE `sp_Dunnage_QuantityTypes_GetAll`()
BEGIN
    SELECT
        id,
        quantity_type
    FROM dunnage_quantity_types
    ORDER BY quantity_type;
END $$

DELIMITER ;