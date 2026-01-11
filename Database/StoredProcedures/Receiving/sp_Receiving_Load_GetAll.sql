-- Stored Procedure: sp_Receiving_Load_GetAll
-- Description: Retrieves all receiving loads within a date range for Edit Mode
-- Parameters:
--   p_StartDate - Start date for retrieval
--   p_EndDate - End date for retrieval

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Receiving_Load_GetAll`;

DELIMITER $$

CREATE PROCEDURE `sp_Receiving_Load_GetAll`(
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
    FROM receiving_history
    WHERE ReceivedDate >= p_StartDate
      AND ReceivedDate <= p_EndDate
    ORDER BY ReceivedDate DESC, LoadNumber ASC;
END$$

DELIMITER ;
