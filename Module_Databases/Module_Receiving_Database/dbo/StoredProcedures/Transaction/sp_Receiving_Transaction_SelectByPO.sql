-- ==============================================================================
-- Stored Procedure: sp_Receiving_Transaction_SelectByPO
-- Purpose: Retrieve transactions by PO Number
-- Module: Module_Receiving
-- Created: 2026-01-25
-- ==============================================================================

CREATE PROCEDURE [dbo].[sp_Receiving_Transaction_SelectByPO]
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
            [TransactionId],
            [PONumber],
            [PartNumber],
            [TotalLoads],
            [TotalWeight],
            [TotalQuantity],
            [WorkflowMode],
            [Status],
            [CompletedDate],
            [IsNonPO],
            [RequiresQualityHold],
            [QualityHoldAcknowledged],
            [SaveDataTransferObjectsCSV],
            [CSVFilePath],
            [CSVExportDate],
            [CreatedBy],
            [CreatedDate],
            [ModifiedBy],
            [ModifiedDate]
        FROM [dbo].[tbl_Receiving_Transaction]
        WHERE [PONumber] = @p_PONumber
          AND [IsActive] = 1 
          AND [IsDeleted] = 0
        ORDER BY [CreatedDate] DESC;
        
        SELECT 1 AS IsSuccess, '' AS ErrorMessage;
    END TRY
    BEGIN CATCH
        SELECT 0 AS IsSuccess, ERROR_MESSAGE() AS ErrorMessage;
    END CATCH
END
GO

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Retrieves transactions by PO Number. Returns all matching transactions.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'PROCEDURE', @level1name = N'sp_Receiving_Transaction_SelectByPO';
GO
