-- ==============================================================================
-- View: vw_Receiving_TransactionSummary
-- Purpose: Summary view of receiving transactions with aggregated line data
-- Module: Module_Receiving
-- Created: 2026-01-25
-- ==============================================================================

CREATE VIEW [dbo].[vw_Receiving_TransactionSummary]
AS
SELECT 
    t.[TransactionId],
    t.[PONumber],
    t.[PartNumber],
    t.[TotalLoads],
    t.[TotalWeight],
    t.[TotalQuantity],
    t.[WorkflowMode],
    t.[Status],
    t.[IsNonPO],
    t.[RequiresQualityHold],
    t.[CompletedDate],
    t.[SavedToCSV],
    t.[CSVFilePath],
    t.[CSVExportDate],
    t.[CreatedBy],
    t.[CreatedDate],
    t.[ModifiedBy],
    t.[ModifiedDate],
    
    -- Aggregated data from lines
    COUNT(l.[LineId]) AS LineCount,
    SUM(CASE WHEN l.[IsAutoFilled] = 1 THEN 1 ELSE 0 END) AS AutoFilledCount,
    SUM(CASE WHEN l.[OnQualityHold] = 1 THEN 1 ELSE 0 END) AS QualityHoldCount,
    AVG(l.[Weight]) AS AverageLoadWeight,
    MIN(l.[LoadNumber]) AS MinLoadNumber,
    MAX(l.[LoadNumber]) AS MaxLoadNumber
    
FROM [dbo].[tbl_Receiving_Transaction] t
LEFT JOIN [dbo].[tbl_Receiving_Line] l 
    ON t.[TransactionId] = l.[TransactionId]
    AND l.[IsActive] = 1 
    AND l.[IsDeleted] = 0
    
WHERE t.[IsActive] = 1
  AND t.[IsDeleted] = 0

GROUP BY 
    t.[TransactionId], t.[PONumber], t.[PartNumber], t.[TotalLoads],
    t.[TotalWeight], t.[TotalQuantity], t.[WorkflowMode], t.[Status],
    t.[IsNonPO], t.[RequiresQualityHold], t.[CompletedDate], t.[SavedToCSV],
    t.[CSVFilePath], t.[CSVExportDate], t.[CreatedBy], t.[CreatedDate],
    t.[ModifiedBy], t.[ModifiedDate];
GO

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Summary view of receiving transactions with aggregated line data. Used for reporting and Edit Mode search.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'VIEW', @level1name = N'vw_Receiving_TransactionSummary';
GO
