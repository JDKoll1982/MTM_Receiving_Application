-- ==============================================================================
-- Stored Procedure: sp_Receiving_Transaction_Update
-- Purpose: Update an existing receiving transaction
-- Module: Module_Receiving
-- Created: 2026-01-25
-- ==============================================================================

CREATE PROCEDURE [dbo].[sp_Receiving_Transaction_Update]
    @p_TransactionId CHAR(36),
    @p_PONumber NVARCHAR(50) = NULL,
    @p_PartNumber NVARCHAR(50) = NULL,
    @p_TotalLoads INT = NULL,
    @p_TotalWeight DECIMAL(18, 2) = NULL,
    @p_TotalQuantity INT = NULL,
    @p_Status NVARCHAR(20) = NULL,
    @p_CompletedDate DATETIME2 = NULL,
    @p_SaveDataTransferObjectsCSV BIT = NULL,
    @p_CSVFilePath NVARCHAR(500) = NULL,
    @p_CSVExportDate DATETIME2 = NULL,
    @p_ModifiedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    
    BEGIN TRY
        -- Validation
        IF @p_TransactionId IS NULL OR @p_TransactionId = ''
        BEGIN
            SELECT 0 AS IsSuccess, 'TransactionId is required' AS ErrorMessage;
            RETURN;
        END
        
        -- Check if transaction exists
        IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_Receiving_Transaction] WHERE [TransactionId] = @p_TransactionId AND [IsActive] = 1 AND [IsDeleted] = 0)
        BEGIN
            SELECT 0 AS IsSuccess, 'Transaction not found' AS ErrorMessage;
            RETURN;
        END
        
        BEGIN TRANSACTION;
        
        -- Update only provided fields
        UPDATE [dbo].[tbl_Receiving_Transaction]
        SET
            [PONumber] = ISNULL(@p_PONumber, [PONumber]),
            [PartNumber] = ISNULL(@p_PartNumber, [PartNumber]),
            [TotalLoads] = ISNULL(@p_TotalLoads, [TotalLoads]),
            [TotalWeight] = ISNULL(@p_TotalWeight, [TotalWeight]),
            [TotalQuantity] = ISNULL(@p_TotalQuantity, [TotalQuantity]),
            [Status] = ISNULL(@p_Status, [Status]),
            [CompletedDate] = ISNULL(@p_CompletedDate, [CompletedDate]),
            [SaveDataTransferObjectsCSV] = ISNULL(@p_SaveDataTransferObjectsCSV, [SaveDataTransferObjectsCSV]),
            [CSVFilePath] = ISNULL(@p_CSVFilePath, [CSVFilePath]),
            [CSVExportDate] = ISNULL(@p_CSVExportDate, [CSVExportDate]),
            [ModifiedBy] = @p_ModifiedBy,
            [ModifiedDate] = GETUTCDATE()
        WHERE [TransactionId] = @p_TransactionId;
        
        -- Log to audit trail
        INSERT INTO [dbo].[tbl_Receiving_AuditLog] (
            [AuditId], [TableName], [RecordId], [Action], [TransactionId],
            [PerformedBy], [PerformedDate], [Details]
        )
        VALUES (
            NEWID(), 'tbl_Receiving_Transaction', @p_TransactionId, 'UPDATE', @p_TransactionId,
            @p_ModifiedBy, GETUTCDATE(), 
            'Updated transaction'
        );
        
        COMMIT TRANSACTION;
        
        -- Return success
        SELECT 1 AS IsSuccess, '' AS ErrorMessage;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        
        SELECT 0 AS IsSuccess, ERROR_MESSAGE() AS ErrorMessage;
    END CATCH
END
GO

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Updates an existing receiving transaction. Only updates fields that are provided (not NULL). Logs to audit trail.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'PROCEDURE', @level1name = N'sp_Receiving_Transaction_Update';
GO
