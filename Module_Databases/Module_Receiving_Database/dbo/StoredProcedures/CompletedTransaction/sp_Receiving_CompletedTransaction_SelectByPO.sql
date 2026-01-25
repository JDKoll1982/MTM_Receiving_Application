-- ==============================================================================
-- Stored Procedure: sp_Receiving_CompletedTransaction_SelectByPO
-- Purpose: Retrieve completed transactions by PO Number (for Edit Mode)
-- Module: Module_Receiving
-- Created: 2026-01-25
-- ==============================================================================

CREATE PROCEDURE [dbo].[sp_Receiving_CompletedTransaction_SelectByPO]
    @p_PONumber NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        IF @p_PONumber IS NULL OR @p_PONumber = ''
        BEGIN
            SELECT 0 AS IsSuccess, 'PONumber is required' AS ErrorMessage;
            RETURN;
        END
        
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
            [CSVFilePath],
            [CSVExportDate],
            [HasQualityHold],
            [QualityHoldCount],
            [HasBeenModified],
            [LastModifiedDate],
            [LastModifiedBy],
            [ModificationCount],
            [ReExportCount]
        FROM [dbo].[tbl_Receiving_CompletedTransaction]
        WHERE [PONumber] = @p_PONumber
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
    @value = N'Retrieves completed transactions by PO Number for Edit Mode search.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'PROCEDURE', @level1name = N'sp_Receiving_CompletedTransaction_SelectByPO';
GO
