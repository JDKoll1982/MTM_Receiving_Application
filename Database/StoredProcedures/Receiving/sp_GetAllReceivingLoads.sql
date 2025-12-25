-- Stored Procedure: sp_GetAllReceivingLoads
-- Description: Retrieves all receiving loads within a date range for Edit Mode
-- Parameters:
--   p_StartDate - Start date for retrieval
--   p_EndDate - End date for retrieval

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS sp_GetAllReceivingLoads;

DELIMITER $$

CREATE PROCEDURE sp_GetAllReceivingLoads(
    IN p_StartDate DATE,
    IN p_EndDate DATE
)
BEGIN
    SELECT 
        LoadID,
        PartID,
        PartType,
        PONumber,
        POLineNumber,
        LoadNumber,
        WeightQuantity,
        HeatLotNumber,
        PackagesPerLoad,
        PackageTypeName,
        WeightPerPackage,
        IsNonPOItem,
        ReceivedDate
    FROM receiving_loads
    WHERE ReceivedDate >= p_StartDate 
      AND ReceivedDate <= p_EndDate
    ORDER BY ReceivedDate DESC, LoadNumber ASC;
END$$

DELIMITER ;
