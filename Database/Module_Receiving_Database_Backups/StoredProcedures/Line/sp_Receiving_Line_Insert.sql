-- ==============================================================================
-- Stored Procedure: sp_Receiving_Line_Insert
-- Purpose: Insert a new receiving line
-- Module: Module_Receiving
-- Created: 2026-01-25
-- ==============================================================================

CREATE PROCEDURE [dbo].[sp_Receiving_Line_Insert]
    @p_LineId CHAR(36),
    @p_TransactionId CHAR(36),
    @p_LineNumber INT,
    @p_PONumber NVARCHAR(50) = NULL,
    @p_PartNumber NVARCHAR(50),
    @p_LoadNumber INT,
    @p_Quantity INT = NULL,
    @p_Weight DECIMAL(18, 2) = NULL,
    @p_HeatLot NVARCHAR(100) = NULL,
    @p_PackageType NVARCHAR(50) = NULL,
    @p_PackagesPerLoad INT = NULL,
    @p_WeightPerPackage DECIMAL(18, 2) = NULL,
    @p_ReceivingLocation NVARCHAR(100) = NULL,
    @p_PartType NVARCHAR(50) = NULL,
    @p_IsNonPO BIT = 0,
    @p_IsAutoFilled BIT = 0,
    @p_AutoFillSource INT = NULL,
    @p_CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    
    BEGIN TRY
        -- Validation
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
        
        IF @p_LineNumber <= 0
        BEGIN
            SELECT 0 AS IsSuccess, 'LineNumber must be greater than zero' AS ErrorMessage;
            RETURN;
        END
        
        IF @p_LoadNumber <= 0
        BEGIN
            SELECT 0 AS IsSuccess, 'LoadNumber must be greater than zero' AS ErrorMessage;
            RETURN;
        END
        
        BEGIN TRANSACTION;
        
        INSERT INTO [dbo].[tbl_Receiving_Line] (
            [LineId], [TransactionId], [LineNumber], [PONumber], [PartNumber],
            [LoadNumber], [Quantity], [Weight], [HeatLot], [PackageType],
            [PackagesPerLoad], [WeightPerPackage], [ReceivingLocation], [PartType],
            [IsNonPO], [IsAutoFilled], [AutoFillSource],
            [IsActive], [IsDeleted], [CreatedBy], [CreatedDate]
        )
        VALUES (
            @p_LineId, @p_TransactionId, @p_LineNumber, @p_PONumber, @p_PartNumber,
            @p_LoadNumber, @p_Quantity, @p_Weight, @p_HeatLot, @p_PackageType,
            @p_PackagesPerLoad, @p_WeightPerPackage, @p_ReceivingLocation, @p_PartType,
            @p_IsNonPO, @p_IsAutoFilled, @p_AutoFillSource,
            1, 0, @p_CreatedBy, GETUTCDATE()
        );
        
        -- Log to audit trail
        INSERT INTO [dbo].[tbl_Receiving_AuditLog] (
            [AuditId], [TableName], [RecordId], [Action], [TransactionId],
            [PerformedBy], [PerformedDate], [Details]
        )
        VALUES (
            NEWID(), 'tbl_Receiving_Line', @p_LineId, 'INSERT', @p_TransactionId,
            @p_CreatedBy, GETUTCDATE(), 
            CONCAT('Created line: Load=', @p_LoadNumber, ', Part=', @p_PartNumber)
        );
        
        COMMIT TRANSACTION;
        
        SELECT 1 AS IsSuccess, '' AS ErrorMessage, @p_LineId AS LineId;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        
        SELECT 0 AS IsSuccess, ERROR_MESSAGE() AS ErrorMessage, NULL AS LineId;
    END CATCH
END
GO

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Inserts a new receiving line. Returns IsSuccess, ErrorMessage, and LineId. Logs to audit trail.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'PROCEDURE', @level1name = N'sp_Receiving_Line_Insert';
GO
