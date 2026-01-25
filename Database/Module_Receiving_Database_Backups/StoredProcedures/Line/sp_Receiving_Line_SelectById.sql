-- ==============================================================================
-- Stored Procedure: sp_Receiving_Line_SelectById
-- Purpose: Retrieve a single receiving line by ID
-- Module: Module_Receiving
-- Created: 2026-01-25
-- ==============================================================================

CREATE PROCEDURE [dbo].[sp_Receiving_Line_SelectById]
    @p_LineId CHAR(36)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        IF @p_LineId IS NULL OR @p_LineId = ''
        BEGIN
            SELECT 0 AS IsSuccess, 'LineId is required' AS ErrorMessage;
            RETURN;
        END
        
        SELECT 
            [LineId],
            [TransactionId],
            [LineNumber],
            [PONumber],
            [PartNumber],
            [LoadNumber],
            [Quantity],
            [Weight],
            [HeatLot],
            [PackageType],
            [PackagesPerLoad],
            [WeightPerPackage],
            [ReceivingLocation],
            [PartType],
            [IsNonPO],
            [IsAutoFilled],
            [AutoFillSource],
            [OnQualityHold],
            [QualityHoldReason],
            [CreatedBy],
            [CreatedDate],
            [ModifiedBy],
            [ModifiedDate]
        FROM [dbo].[tbl_Receiving_Line]
        WHERE [LineId] = @p_LineId
          AND [IsActive] = 1 
          AND [IsDeleted] = 0;
        
        IF @@ROWCOUNT = 0
        BEGIN
            SELECT 0 AS IsSuccess, 'Line not found' AS ErrorMessage;
        END
        ELSE
        BEGIN
            SELECT 1 AS IsSuccess, '' AS ErrorMessage;
        END
    END TRY
    BEGIN CATCH
        SELECT 0 AS IsSuccess, ERROR_MESSAGE() AS ErrorMessage;
    END CATCH
END
GO

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Retrieves a single receiving line by LineId.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'PROCEDURE', @level1name = N'sp_Receiving_Line_SelectById';
GO
