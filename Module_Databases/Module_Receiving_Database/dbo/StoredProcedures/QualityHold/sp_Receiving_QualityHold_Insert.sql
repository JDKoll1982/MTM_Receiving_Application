-- ==============================================================================
-- Stored Procedure: sp_Receiving_QualityHold_Insert
-- Purpose: Insert a new quality hold record with initial acknowledgment
-- Module: Module_Receiving
-- Created: 2026-01-30
-- ==============================================================================

CREATE PROCEDURE [dbo].[sp_Receiving_QualityHold_Insert]
    @p_QualityHoldId CHAR(36),
    @p_LineId CHAR(36),
    @p_TransactionId CHAR(36),
    @p_PartNumber NVARCHAR(50),
    @p_PartPattern NVARCHAR(100),
    @p_RestrictionType NVARCHAR(50),
    @p_LoadNumber INT,
    @p_TotalWeight DECIMAL(18, 2) = NULL,
    @p_PackageType NVARCHAR(50) = NULL,
    @p_UserAcknowledgedDate DATETIME2 = NULL,
    @p_UserAcknowledgedBy NVARCHAR(100) = NULL,
    @p_UserAcknowledgmentMessage NVARCHAR(500) = NULL,
    @p_Notes NVARCHAR(MAX) = NULL,
    @p_CreatedBy NVARCHAR(100)
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
        
        IF @p_LineId IS NULL OR @p_LineId = ''
        BEGIN
            SELECT 0 AS IsSuccess, 'LineId is required' AS ErrorMessage;
            RETURN;
        END
        
        IF @p_TransactionId IS NULL OR @p_TransactionId = ''
        BEGIN
            SELECT 0 AS IsSuccess, 'TransactionId is required' AS ErrorMessage;
            RETURN;
        END
        
        IF @p_PartNumber IS NULL OR @p_PartNumber = ''
        BEGIN
            SELECT 0 AS IsSuccess, 'PartNumber is required' AS ErrorMessage;
            RETURN;
        END
        
        IF @p_PartPattern IS NULL OR @p_PartPattern = ''
        BEGIN
            SELECT 0 AS IsSuccess, 'PartPattern is required' AS ErrorMessage;
            RETURN;
        END
        
        IF @p_RestrictionType IS NULL OR @p_RestrictionType = ''
        BEGIN
            SELECT 0 AS IsSuccess, 'RestrictionType is required' AS ErrorMessage;
            RETURN;
        END
        
        IF @p_LoadNumber <= 0
        BEGIN
            SELECT 0 AS IsSuccess, 'LoadNumber must be greater than zero' AS ErrorMessage;
            RETURN;
        END
        
        BEGIN TRANSACTION;
        
        INSERT INTO [dbo].[tbl_Receiving_QualityHold] (
            [QualityHoldId], [LineId], [TransactionId], [PartNumber], [PartPattern],
            [RestrictionType], [LoadNumber], [TotalWeight], [PackageType],
            [UserAcknowledgedDate], [UserAcknowledgedBy], [UserAcknowledgmentMessage],
            [IsFullyAcknowledged], [Notes],
            [IsActive], [IsDeleted], [CreatedBy], [CreatedDate]
        )
        VALUES (
            @p_QualityHoldId, @p_LineId, @p_TransactionId, @p_PartNumber, @p_PartPattern,
            @p_RestrictionType, @p_LoadNumber, @p_TotalWeight, @p_PackageType,
            @p_UserAcknowledgedDate, @p_UserAcknowledgedBy, @p_UserAcknowledgmentMessage,
            0, @p_Notes,
            1, 0, @p_CreatedBy, GETUTCDATE()
        );
        
        -- Log to audit trail
        INSERT INTO [dbo].[tbl_Receiving_AuditLog] (
            [AuditId], [TableName], [RecordId], [Action], [TransactionId],
            [PerformedBy], [PerformedDate], [Details]
        )
        VALUES (
            NEWID(), 'tbl_Receiving_QualityHold', @p_QualityHoldId, 'INSERT', @p_TransactionId,
            @p_CreatedBy, GETUTCDATE(), 
            CONCAT('Created quality hold: Part=', @p_PartNumber, ', Pattern=', @p_PartPattern, ', Load=', @p_LoadNumber)
        );
        
        COMMIT TRANSACTION;
        
        SELECT 1 AS IsSuccess, '' AS ErrorMessage, @p_QualityHoldId AS QualityHoldId;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        
        SELECT 0 AS IsSuccess, ERROR_MESSAGE() AS ErrorMessage, NULL AS QualityHoldId;
    END CATCH
END
GO

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Inserts a new quality hold record with initial user acknowledgment. Part of two-step acknowledgment workflow.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'PROCEDURE', @level1name = N'sp_Receiving_QualityHold_Insert';
GO
