-- ==============================================================================
-- Stored Procedure: sp_Receiving_AuditLog_Insert
-- Purpose: Insert audit log entry
-- Module: Module_Receiving
-- Created: 2026-01-25
-- ==============================================================================

CREATE PROCEDURE [dbo].[sp_Receiving_AuditLog_Insert]
    @p_TableName NVARCHAR(100),
    @p_RecordId CHAR(36),
    @p_Action NVARCHAR(20),
    @p_FieldName NVARCHAR(100) = NULL,
    @p_OldValue NVARCHAR(MAX) = NULL,
    @p_NewValue NVARCHAR(MAX) = NULL,
    @p_TransactionId CHAR(36) = NULL,
    @p_ChangeReason NVARCHAR(500) = NULL,
    @p_ChangeContext NVARCHAR(50) = NULL,
    @p_PerformedBy NVARCHAR(100),
    @p_Details NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    
    BEGIN TRY
        IF @p_TableName IS NULL OR @p_TableName = ''
        BEGIN
            SELECT 0 AS IsSuccess, 'TableName is required' AS ErrorMessage;
            RETURN;
        END
        
        IF @p_Action NOT IN ('INSERT', 'UPDATE', 'DELETE', 'SELECT')
        BEGIN
            SELECT 0 AS IsSuccess, 'Invalid Action. Must be INSERT, UPDATE, DELETE, or SELECT' AS ErrorMessage;
            RETURN;
        END
        
        BEGIN TRANSACTION;
        
        DECLARE @AuditId CHAR(36) = NEWID();
        
        INSERT INTO [dbo].[tbl_Receiving_AuditLog] (
            [AuditId], [TableName], [RecordId], [Action],
            [FieldName], [OldValue], [NewValue], [TransactionId],
            [ChangeReason], [ChangeContext], [PerformedBy], [PerformedDate],
            [Details]
        )
        VALUES (
            @AuditId, @p_TableName, @p_RecordId, @p_Action,
            @p_FieldName, @p_OldValue, @p_NewValue, @p_TransactionId,
            @p_ChangeReason, @p_ChangeContext, @p_PerformedBy, GETUTCDATE(),
            @p_Details
        );
        
        COMMIT TRANSACTION;
        
        SELECT 1 AS IsSuccess, '' AS ErrorMessage, @AuditId AS AuditId;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        
        SELECT 0 AS IsSuccess, ERROR_MESSAGE() AS ErrorMessage, NULL AS AuditId;
    END CATCH
END
GO

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Inserts an audit log entry with field-level change tracking.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'PROCEDURE', @level1name = N'sp_Receiving_AuditLog_Insert';
GO
