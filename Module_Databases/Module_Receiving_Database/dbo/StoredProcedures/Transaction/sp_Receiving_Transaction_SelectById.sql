-- ==============================================================================
-- Stored Procedure: sp_Receiving_Transaction_SelectById
-- Purpose: Retrieve a single transaction by ID
-- Module: Module_Receiving
-- Created: 2026-01-25
-- ==============================================================================

CREATE PROCEDURE [dbo].[sp_Receiving_Transaction_SelectById]
    @p_TransactionId CHAR(36)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        IF @p_TransactionId IS NULL OR @p_TransactionId = ''
        BEGIN
            SELECT 0 AS IsSuccess, 'TransactionId is required' AS ErrorMessage;
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
            [SavedToCSV],
            [SavedToDatabase],
            [CSVFilePath],
            [CSVExportDate],
            [SessionId],
            [IsActive],
            [IsDeleted],
            [CreatedBy],
            [CreatedDate],
            [ModifiedBy],
            [ModifiedDate]
        FROM [dbo].[tbl_Receiving_Transaction]
        WHERE [TransactionId] = @p_TransactionId
          AND [IsActive] = 1 
          AND [IsDeleted] = 0;
        
        IF @@ROWCOUNT = 0
        BEGIN
            SELECT 0 AS IsSuccess, 'Transaction not found' AS ErrorMessage;
        END
        ELSE
        BEGIN
            SELECT 1 AS IsSuccess, '' AS ErrorMessage;
        END
    END TRY
    BEGIN CATCH
        SELECT 0 AS IsSuccess, ERROR_MESSAGE() AS ErrorMessage;
    END CATCH
END
GO

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Retrieves a single receiving transaction by TransactionId. Returns all fields.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'PROCEDURE', @level1name = N'sp_Receiving_Transaction_SelectById';
GO
