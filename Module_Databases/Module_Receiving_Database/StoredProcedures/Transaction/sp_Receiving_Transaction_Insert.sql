-- ==============================================================================
-- Stored Procedure: sp_Receiving_Transaction_Insert
-- Purpose: Insert a new receiving transaction
-- Module: Module_Receiving
-- Created: 2026-01-25
-- ==============================================================================

CREATE PROCEDURE [dbo].[sp_Receiving_Transaction_Insert]
    @p_TransactionId CHAR(36),
    @p_PONumber NVARCHAR(50) = NULL,
    @p_PartNumber NVARCHAR(50),
    @p_TotalLoads INT,
    @p_TotalWeight DECIMAL(18, 2) = NULL,
    @p_TotalQuantity INT = NULL,
    @p_WorkflowMode NVARCHAR(20),
    @p_Status NVARCHAR(20) = 'Draft',
    @p_IsNonPO BIT = 0,
    @p_RequiresQualityHold BIT = 0,
    @p_QualityHoldAcknowledged BIT = 0,
    @p_SessionId CHAR(36) = NULL,
    @p_CreatedBy NVARCHAR(100)
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
        
        IF @p_PartNumber IS NULL OR @p_PartNumber = ''
        BEGIN
            SELECT 0 AS IsSuccess, 'PartNumber is required' AS ErrorMessage;
            RETURN;
        END
        
        IF @p_TotalLoads <= 0
        BEGIN
            SELECT 0 AS IsSuccess, 'TotalLoads must be greater than zero' AS ErrorMessage;
            RETURN;
        END
        
        IF @p_WorkflowMode NOT IN ('Wizard', 'Manual', 'Edit')
        BEGIN
            SELECT 0 AS IsSuccess, 'Invalid WorkflowMode. Must be Wizard, Manual, or Edit' AS ErrorMessage;
            RETURN;
        END
        
        -- Insert
        BEGIN TRANSACTION;
        
        INSERT INTO [dbo].[tbl_Receiving_Transaction] (
            [TransactionId], [PONumber], [PartNumber], [TotalLoads], [TotalWeight],
            [TotalQuantity], [WorkflowMode], [Status], [IsNonPO],
            [RequiresQualityHold], [QualityHoldAcknowledged], [SessionId],
            [IsActive], [IsDeleted], [CreatedBy], [CreatedDate]
        )
        VALUES (
            @p_TransactionId, @p_PONumber, @p_PartNumber, @p_TotalLoads, @p_TotalWeight,
            @p_TotalQuantity, @p_WorkflowMode, @p_Status, @p_IsNonPO,
            @p_RequiresQualityHold, @p_QualityHoldAcknowledged, @p_SessionId,
            1, 0, @p_CreatedBy, GETUTCDATE()
        );
        
        -- Log to audit trail
        INSERT INTO [dbo].[tbl_Receiving_AuditLog] (
            [AuditId], [TableName], [RecordId], [Action], [TransactionId],
            [PerformedBy], [PerformedDate], [Details]
        )
        VALUES (
            NEWID(), 'tbl_Receiving_Transaction', @p_TransactionId, 'INSERT', @p_TransactionId,
            @p_CreatedBy, GETUTCDATE(), 
            CONCAT('Created transaction: PO=', ISNULL(@p_PONumber, 'N/A'), ', Part=', @p_PartNumber, ', Loads=', @p_TotalLoads)
        );
        
        COMMIT TRANSACTION;
        
        -- Return success
        SELECT 1 AS IsSuccess, '' AS ErrorMessage, @p_TransactionId AS TransactionId;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        
        -- Return error
        SELECT 
            0 AS IsSuccess, 
            ERROR_MESSAGE() AS ErrorMessage,
            NULL AS TransactionId;
    END CATCH
END
GO

-- Extended Property
EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Inserts a new receiving transaction. Returns IsSuccess, ErrorMessage, and TransactionId. Automatically logs to audit trail.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'PROCEDURE', @level1name = N'sp_Receiving_Transaction_Insert';
GO
