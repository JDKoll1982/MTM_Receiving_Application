-- ==============================================================================
-- Stored Procedure: sp_Receiving_AuditLog_SelectByTransaction
-- Purpose: Retrieve audit trail for a transaction
-- Module: Module_Receiving
-- Created: 2026-01-25
-- ==============================================================================

CREATE PROCEDURE [dbo].[sp_Receiving_AuditLog_SelectByTransaction]
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
            [AuditId],
            [TableName],
            [RecordId],
            [Action],
            [FieldName],
            [OldValue],
            [NewValue],
            [TransactionId],
            [ChangeReason],
            [ChangeContext],
            [PerformedBy],
            [PerformedDate],
            [Details]
        FROM [dbo].[tbl_Receiving_AuditLog]
        WHERE [TransactionId] = @p_TransactionId
        ORDER BY [PerformedDate] DESC;
        
        SELECT 1 AS IsSuccess, '' AS ErrorMessage;
    END TRY
    BEGIN CATCH
        SELECT 0 AS IsSuccess, ERROR_MESSAGE() AS ErrorMessage;
    END CATCH
END
GO

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Retrieves complete audit trail for a transaction, ordered by date descending.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'PROCEDURE', @level1name = N'sp_Receiving_AuditLog_SelectByTransaction';
GO
