-- ==============================================================================
-- Stored Procedure: sp_Receiving_Line_Delete
-- Purpose: Soft delete a receiving line
-- Module: Module_Receiving
-- Created: 2026-01-25
-- ==============================================================================

CREATE PROCEDURE [dbo].[sp_Receiving_Line_Delete]
    @p_LineId CHAR(36),
    @p_ModifiedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    
    BEGIN TRY
        IF @p_LineId IS NULL OR @p_LineId = ''
        BEGIN
            SELECT 0 AS IsSuccess, 'LineId is required' AS ErrorMessage;
            RETURN;
        END
        
        IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_Receiving_Line] WHERE [LineId] = @p_LineId AND [IsActive] = 1 AND [IsDeleted] = 0)
        BEGIN
            SELECT 0 AS IsSuccess, 'Line not found' AS ErrorMessage;
            RETURN;
        END
        
        BEGIN TRANSACTION;
        
        UPDATE [dbo].[tbl_Receiving_Line]
        SET
            [IsDeleted] = 1,
            [ModifiedBy] = @p_ModifiedBy,
            [ModifiedDate] = GETUTCDATE()
        WHERE [LineId] = @p_LineId;
        
        -- Log to audit trail
        INSERT INTO [dbo].[tbl_Receiving_AuditLog] (
            [AuditId], [TableName], [RecordId], [Action], [TransactionId],
            [PerformedBy], [PerformedDate], [Details]
        )
        SELECT 
            NEWID(), 'tbl_Receiving_Line', @p_LineId, 'DELETE', [TransactionId],
            @p_ModifiedBy, GETUTCDATE(), 
            'Soft deleted receiving line'
        FROM [dbo].[tbl_Receiving_Line]
        WHERE [LineId] = @p_LineId;
        
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
    @value = N'Soft deletes a receiving line. Logs to audit trail.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'PROCEDURE', @level1name = N'sp_Receiving_Line_Delete';
GO
