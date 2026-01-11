-- Stored Procedure: sp_Receiving_Load_Update
-- Description: Updates an existing receiving load record
-- Parameters: All load fields

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Receiving_Load_Update`;

DELIMITER $$

CREATE PROCEDURE `sp_Receiving_Load_Update`(
    IN p_LoadID VARCHAR(36),
    IN p_PartID VARCHAR(50),
    IN p_PartType VARCHAR(20),
    IN p_PONumber VARCHAR(20),
    IN p_POLineNumber VARCHAR(10),
    IN p_LoadNumber INT,
    IN p_WeightQuantity DECIMAL(18, 2),
    IN p_HeatLotNumber VARCHAR(50),
    IN p_PackagesPerLoad INT,
    IN p_PackageTypeName VARCHAR(50),
    IN p_WeightPerPackage DECIMAL(18, 2),
    IN p_IsNonPOItem BIT,
    IN p_ReceivedDate DATETIME
)
BEGIN
    UPDATE receiving_history
    SET
        PartID = p_PartID,
        PartType = p_PartType,
        PONumber = p_PONumber,
        POLineNumber = p_POLineNumber,
        LoadNumber = p_LoadNumber,
        WeightQuantity = p_WeightQuantity,
        HeatLotNumber = p_HeatLotNumber,
        PackagesPerLoad = p_PackagesPerLoad,
        PackageTypeName = p_PackageTypeName,
        WeightPerPackage = p_WeightPerPackage,
        IsNonPOItem = p_IsNonPOItem,
        ReceivedDate = p_ReceivedDate
    WHERE LoadID = p_LoadID;
END$$

DELIMITER ;
