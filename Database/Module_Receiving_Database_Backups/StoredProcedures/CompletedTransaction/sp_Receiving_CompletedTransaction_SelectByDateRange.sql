-- ==============================================================================
-- Stored Procedure: sp_Receiving_CompletedTransaction_SelectByDateRange
-- Purpose: Retrieve completed transactions within date range
-- Module: Module_Receiving
-- Created: 2026-01-25
-- ==============================================================================

CREATE PROCEDURE [dbo].[sp_Receiving_CompletedTransaction_SelectByDateRange]
    @p_StartDate DATETIME2,
    @p_EndDate DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        SELECT 
            [CompletedTransactionId],
            [OriginalTransactionId],
            [PONumber],
            [PartNumber],
            [TotalLoads],
            [TotalWeight],
            [TotalQuantity],
            [WorkflowMode],
            [CompletedDate],
            [CompletedBy],
            [HasQualityHold],
            [HasBeenModified],
            [LastModifiedDate],
            [ModificationCount]
        FROM [dbo].[tbl_Receiving_CompletedTransaction]
        WHERE [CompletedDate] >= @p_StartDate
          AND [CompletedDate] <= @p_EndDate
          AND [IsActive] = 1 
          AND [IsDeleted] = 0
        ORDER BY [CompletedDate] DESC;
        
        SELECT 1 AS IsSuccess, '' AS ErrorMessage;
    END TRY
    BEGIN CATCH
        SELECT 0 AS IsSuccess, ERROR_MESSAGE() AS ErrorMessage;
    END CATCH
END
GO

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Retrieves completed transactions within a date range.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'PROCEDURE', @level1name = N'sp_Receiving_CompletedTransaction_SelectByDateRange';
GO
