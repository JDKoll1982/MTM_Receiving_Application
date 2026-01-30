-- ==============================================================================
-- Stored Procedure: sp_Receiving_QualityHold_UpdateFinalAcknowledgment
-- Purpose: Update quality hold with final acknowledgment (before save)
-- Module: Module_Receiving
-- Created: 2026-01-30
-- ==============================================================================

CREATE PROCEDURE [dbo].[sp_Receiving_QualityHold_UpdateFinalAcknowledgment]
    @p_QualityHoldId CHAR(36),
    @p_FinalAcknowledgedDate DATETIME2,
    @p_FinalAcknowledgedBy NVARCHAR(100),
    @p_FinalAcknowledgmentMessage NVARCHAR(500) = NULL,
    @p_ModifiedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    
    BEGIN TRY
        -- Validation
        IF @p_QualityHoldId IS NULL OR @p_QualityHoldId = ''
        BEGIN
            SELECT 0 AS IsSuccess, 'QualityHoldId is required' AS ErrorMessage;
            RETURN;
        END
        
        IF @p_FinalAcknowledgedBy IS NULL OR @p_FinalAcknowledgedBy = ''
        BEGIN
            SELECT 0 AS IsSuccess, 'FinalAcknowledgedBy is required' AS ErrorMessage;
            RETURN;
        END
        
        -- Check if quality hold exists
        IF NOT EXISTS (
            SELECT 1 FROM [dbo].[tbl_Receiving_QualityHold] 
            WHERE [QualityHoldId] = @p_QualityHoldId 
              AND [IsActive] = 1 
              AND [IsDeleted] = 0
        )
        BEGIN
            SELECT 0 AS IsSuccess, 'Quality hold record not found' AS ErrorMessage;
            RETURN;
        END
        
        -- Check if user acknowledgment exists (first step must be completed)
        IF NOT EXISTS (
            SELECT 1 FROM [dbo].[tbl_Receiving_QualityHold] 
            WHERE [QualityHoldId] = @p_QualityHoldId 
              AND [UserAcknowledgedDate] IS NOT NULL
        )
        BEGIN
            SELECT 0 AS IsSuccess, 'User acknowledgment (step 1) must be completed before final acknowledgment' AS ErrorMessage;
            RETURN;
        END
        
        BEGIN TRANSACTION;
        
        -- Capture old values for audit trail
        DECLARE @OldIsFullyAcknowledged BIT;
        DECLARE @TransactionId CHAR(36);
        DECLARE @PartNumber NVARCHAR(50);
        
        SELECT 
            @OldIsFullyAcknowledged = [IsFullyAcknowledged],
            @TransactionId = [TransactionId],
            @PartNumber = [PartNumber]
        FROM [dbo].[tbl_Receiving_QualityHold]
        WHERE [QualityHoldId] = @p_QualityHoldId;
        
        -- Update with final acknowledgment
        UPDATE [dbo].[tbl_Receiving_QualityHold]
        SET
            [FinalAcknowledgedDate] = @p_FinalAcknowledgedDate,
            [FinalAcknowledgedBy] = @p_FinalAcknowledgedBy,
            [FinalAcknowledgmentMessage] = @p_FinalAcknowledgmentMessage,
            [IsFullyAcknowledged] = 1,
            [ModifiedBy] = @p_ModifiedBy,
            [ModifiedDate] = GETUTCDATE()
        WHERE [QualityHoldId] = @p_QualityHoldId;
        
        -- Log to audit trail (field-level change tracking)
        INSERT INTO [dbo].[tbl_Receiving_AuditLog] (
            [AuditId], [TableName], [RecordId], [Action], [TransactionId],
            [FieldName], [OldValue], [NewValue],
            [PerformedBy], [PerformedDate], [Details]
        )
        VALUES (
            NEWID(), 'tbl_Receiving_QualityHold', @p_QualityHoldId, 'UPDATE', @TransactionId,
            'IsFullyAcknowledged', CAST(@OldIsFullyAcknowledged AS NVARCHAR(10)), '1',
            @p_ModifiedBy, GETUTCDATE(), 
            CONCAT('Final acknowledgment completed for Part=', @PartNumber, ' (Step 2 of 2)')
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
    @value = N'Updates quality hold record with final acknowledgment. Part of two-step acknowledgment workflow. Sets IsFullyAcknowledged to true.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'PROCEDURE', @level1name = N'sp_Receiving_QualityHold_UpdateFinalAcknowledgment';
GO
