DELIMITER //

DROP PROCEDURE IF EXISTS sp_InsertReceivingLoad //

CREATE PROCEDURE sp_InsertReceivingLoad(
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
    INSERT INTO receiving_loads 
    (LoadID, PartID, PartType, PONumber, POLineNumber, LoadNumber, 
     WeightQuantity, HeatLotNumber, PackagesPerLoad, PackageTypeName, 
     WeightPerPackage, IsNonPOItem, ReceivedDate)
    VALUES 
    (p_LoadID, p_PartID, p_PartType, p_PONumber, p_POLineNumber, p_LoadNumber, 
     p_WeightQuantity, p_HeatLotNumber, p_PackagesPerLoad, p_PackageTypeName, 
     p_WeightPerPackage, p_IsNonPOItem, p_ReceivedDate);
END //

DELIMITER ;
