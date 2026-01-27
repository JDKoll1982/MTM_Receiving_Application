-- ==============================================================================
-- Stored Procedure: sp_Receiving_Line_Update
-- Purpose: Update an existing receiving line
-- Module: Module_Receiving
-- Created: 2026-01-25
-- ==============================================================================

CREATE PROCEDURE [dbo].[sp_Receiving_Line_Update]
    @p_LineId CHAR(36),
    @p_PONumber NVARCHAR(50) = NULL,
    @p_PartNumber NVARCHAR(50) = NULL,
    @p_LoadNumber INT = NULL,
    @p_Quantity INT = NULL,
    @p_Weight DECIMAL(18, 2) = NULL,
    @p_HeatLot NVARCHAR(100) = NULL,
    @p_PackageType NVARCHAR(50) = NULL,
    @p_PackagesPerLoad INT = NULL,
    @p_WeightPerPackage DECIMAL(18, 2) = NULL,
    @p_ReceivingLocation NVARCHAR(100) = NULL,
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
            [PONumber] = ISNULL(@p_PONumber, [PONumber]),
            [PartNumber] = ISNULL(@p_PartNumber, [PartNumber]),
            [LoadNumber] = ISNULL(@p_LoadNumber, [LoadNumber]),
            [Quantity] = ISNULL(@p_Quantity, [Quantity]),
            [Weight] = ISNULL(@p_Weight, [Weight]),
            [HeatLot] = ISNULL(@p_HeatLot, [HeatLot]),
            [PackageType] = ISNULL(@p_PackageType, [PackageType]),
            [PackagesPerLoad] = ISNULL(@p_PackagesPerLoad, [PackagesPerLoad]),
            [WeightPerPackage] = ISNULL(@p_WeightPerPackage, [WeightPerPackage]),
            [ReceivingLocation] = ISNULL(@p_ReceivingLocation, [ReceivingLocation]),
            [ModifiedBy] = @p_ModifiedBy,
            [ModifiedDate] = GETUTCDATE()
        WHERE [LineId] = @p_LineId;
        
        -- Log to audit trail
        INSERT INTO [dbo].[tbl_Receiving_AuditLog] (
            [AuditId], [TableName], [RecordId], [Action], [TransactionId],
            [PerformedBy], [PerformedDate], [Details]
        )
        SELECT 
            NEWID(), 'tbl_Receiving_Line', @p_LineId, 'UPDATE', [TransactionId],
            @p_ModifiedBy, GETUTCDATE(), 
            'Updated receiving line'
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
    @value = N'Updates an existing receiving line. Only updates fields that are provided (not NULL).',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'PROCEDURE', @level1name = N'sp_Receiving_Line_Update';
GO
