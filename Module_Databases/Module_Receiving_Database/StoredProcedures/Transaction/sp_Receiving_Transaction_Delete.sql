-- ==============================================================================
-- Stored Procedure: sp_Receiving_Transaction_Delete
-- Purpose: Soft delete a transaction
-- Module: Module_Receiving
-- Created: 2026-01-25
-- ==============================================================================

CREATE PROCEDURE [dbo].[sp_Receiving_Transaction_Delete]
    @p_TransactionId CHAR(36),
    @p_ModifiedBy NVARCHAR(100)
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
        
        -- Soft delete the transaction
        UPDATE [dbo].[tbl_Receiving_Transaction]
        SET
            [IsDeleted] = 1,
            [ModifiedBy] = @p_ModifiedBy,
            [ModifiedDate] = GETUTCDATE()
        WHERE [TransactionId] = @p_TransactionId;
        
        -- Soft delete associated lines (CASCADE via UPDATE)
        UPDATE [dbo].[tbl_Receiving_Line]
        SET
            [IsDeleted] = 1,
            [ModifiedBy] = @p_ModifiedBy,
            [ModifiedDate] = GETUTCDATE()
        WHERE [TransactionId] = @p_TransactionId
          AND [IsDeleted] = 0;
        
        -- Log to audit trail
        INSERT INTO [dbo].[tbl_Receiving_AuditLog] (
            [AuditId], [TableName], [RecordId], [Action], [TransactionId],
            [PerformedBy], [PerformedDate], [Details]
        )
        VALUES (
            NEWID(), 'tbl_Receiving_Transaction', @p_TransactionId, 'DELETE', @p_TransactionId,
            @p_ModifiedBy, GETUTCDATE(), 
            'Soft deleted transaction and associated lines'
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
    @value = N'Soft deletes a transaction and all associated lines. Logs to audit trail.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'PROCEDURE', @level1name = N'sp_Receiving_Transaction_Delete';
GO
