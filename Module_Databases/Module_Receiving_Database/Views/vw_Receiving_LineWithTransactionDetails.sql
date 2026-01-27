-- ==============================================================================
-- View: vw_Receiving_LineWithTransactionDetails
-- Purpose: Denormalized view of receiving lines with parent transaction info
-- Module: Module_Receiving
-- Created: 2026-01-25
-- ==============================================================================

CREATE VIEW [dbo].[vw_Receiving_LineWithTransactionDetails]
AS
SELECT 
    -- Line fields
    l.[LineId],
    l.[TransactionId],
    l.[LineNumber],
    l.[LoadNumber],
    l.[PONumber] AS LinePONumber,
    l.[PartNumber] AS LinePartNumber,
    l.[Quantity],
    l.[Weight],
    l.[HeatLot],
    l.[PackageType],
    l.[PackagesPerLoad],
    l.[WeightPerPackage],
    l.[ReceivingLocation],
    l.[PartType],
    l.[IsNonPO] AS LineIsNonPO,
    l.[IsAutoFilled],
    l.[AutoFillSource],
    l.[OnQualityHold],
    l.[QualityHoldReason],
    l.[CreatedBy] AS LineCreatedBy,
    l.[CreatedDate] AS LineCreatedDate,
    
    -- Transaction fields
    t.[PONumber] AS TransactionPONumber,
    t.[PartNumber] AS TransactionPartNumber,
    t.[TotalLoads],
    t.[TotalWeight] AS TransactionTotalWeight,
    t.[WorkflowMode],
    t.[Status] AS TransactionStatus,
    t.[CompletedDate],
    t.[CreatedBy] AS TransactionCreatedBy,
    t.[CreatedDate] AS TransactionCreatedDate
    
FROM [dbo].[tbl_Receiving_Line] l
INNER JOIN [dbo].[tbl_Receiving_Transaction] t 
    ON l.[TransactionId] = t.[TransactionId]
    AND t.[IsActive] = 1 
    AND t.[IsDeleted] = 0
    
WHERE l.[IsActive] = 1
  AND l.[IsDeleted] = 0;
GO

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Denormalized view of receiving lines with parent transaction details. Used for detailed reporting and CSV export.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'VIEW', @level1name = N'vw_Receiving_LineWithTransactionDetails
