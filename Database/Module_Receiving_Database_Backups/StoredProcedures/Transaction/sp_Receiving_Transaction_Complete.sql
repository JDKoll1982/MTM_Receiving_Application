-- ==============================================================================
-- Stored Procedure: sp_Receiving_Transaction_Complete
-- Purpose: Mark a transaction as completed
-- Module: Module_Receiving
-- Created: 2026-01-25
-- ==============================================================================

CREATE PROCEDURE [dbo].[sp_Receiving_Transaction_Complete]
    @p_TransactionId CHAR(36),
    @p_CompletedBy NVARCHAR(100),
    @p_CSVFilePath NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    
    BEGIN TRY
        IF @p_TransactionId IS NULL OR @p_TransactionId = ''
        BEGIN
            SELECT 0 AS IsSuccess, 'TransactionId is required' AS ErrorMessage;
            RETURN;
        END
        
        IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_Receiving_Transaction] WHERE [TransactionId] = @p_TransactionId AND [IsActive] = 1 AND [IsDeleted] = 0)
        BEGIN
            SELECT 0 AS IsSuccess, 'Transaction not found' AS ErrorMessage;
            RETURN;
        END
        
        BEGIN TRANSACTION;
        
        -- Update transaction to completed status
        UPDATE [dbo].[tbl_Receiving_Transaction]
        SET
            [Status] = 'Completed',
            [CompletedDate] = GETUTCDATE(),
            [SavedToCSV] = CASE WHEN @p_CSVFilePath IS NOT NULL THEN 1 ELSE 0 END,
            [CSVFilePath] = @p_CSVFilePath,
            [CSVExportDate] = CASE WHEN @p_CSVFilePath IS NOT NULL THEN GETUTCDATE() ELSE NULL END,
            [ModifiedBy] = @p_CompletedBy,
            [ModifiedDate] = GETUTCDATE()
        WHERE [TransactionId] = @p_TransactionId;
        
        -- Archive to completed transactions table
        INSERT INTO [dbo].[tbl_Receiving_CompletedTransaction] (
            [CompletedTransactionId], [OriginalTransactionId], [PONumber], [PartNumber],
            [TotalLoads], [TotalWeight], [TotalQuantity], [WorkflowMode],
            [CompletedDate], [CompletedBy], [CSVFilePath], [CSVExportDate],
            [CreatedBy]
        )
        SELECT 
            NEWID(), [TransactionId], [PONumber], [PartNumber],
            [TotalLoads], [TotalWeight], [TotalQuantity], [WorkflowMode],
            GETUTCDATE(), @p_CompletedBy, @p_CSVFilePath, 
            CASE WHEN @p_CSVFilePath IS NOT NULL THEN GETUTCDATE() ELSE NULL END,
            @p_CompletedBy
        FROM [dbo].[tbl_Receiving_Transaction]
        WHERE [TransactionId] = @p_TransactionId;
        
        -- Log to audit trail
        INSERT INTO [dbo].[tbl_Receiving_AuditLog] (
            [AuditId], [TableName], [RecordId], [Action], [TransactionId],
            [PerformedBy], [PerformedDate], [Details]
        )
        VALUES (
            NEWID(), 'tbl_Receiving_Transaction', @p_TransactionId, 'UPDATE', @p_TransactionId,
            @p_CompletedBy, GETUTCDATE(), 
            'Transaction completed and archived'
        );
        
        COMMIT TRANSACTION;
        
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
    @value = N'Marks a transaction as completed and archives it. Updates status, completion date, and CSV path if provided.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'PROCEDURE', @level1name = N'sp_Receiving_Transaction_Complete';
GO
