-- Stored Procedure: sp_Receiving_Load_Update
-- Description: Updates an existing receiving load record by GUID.
--              Parameter names match Dao_ReceivingLoad.UpdateLoadsAsync exactly
--              (the DAO helper auto-prepends p_ to each key).
--              Parameters for fields not stored in receiving_history
--              (PartType, POLineNumber, PackagesPerLoad, PackageTypeName,
--              WeightPerPackage) are accepted but not used in the UPDATE.

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Receiving_Load_Update` $$

CREATE PROCEDURE `sp_Receiving_Load_Update`(
    IN p_LoadID           CHAR(36),
    IN p_PartID           VARCHAR(50),
    IN p_PartType         VARCHAR(100),
    IN p_PONumber         VARCHAR(20),
    IN p_POLineNumber     VARCHAR(50),
    IN p_LoadNumber       INT,
    IN p_WeightQuantity   DECIMAL(18,4),
    IN p_HeatLotNumber    VARCHAR(100),
    IN p_PackagesPerLoad  INT,
    IN p_PackageTypeName  VARCHAR(100),
    IN p_WeightPerPackage DECIMAL(18,4),
    IN p_IsNonPOItem      TINYINT(1),
    IN p_ReceivedDate     DATETIME
)
BEGIN
    UPDATE receiving_history
    SET
        part_id          = p_PartID,
        po_number        = p_PONumber,
        quantity         = ROUND(p_WeightQuantity, 0),
        heat             = p_HeatLotNumber,
        transaction_date = DATE(p_ReceivedDate),
        label_number     = IFNULL(p_LoadNumber, 1),
        is_non_po_item   = IFNULL(p_IsNonPOItem, 0)
    WHERE load_guid = p_LoadID;
END$$

DELIMITER ;
