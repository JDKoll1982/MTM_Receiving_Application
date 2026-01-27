-- ==============================================================================
-- Stored Procedure: sp_Receiving_Line_SelectByPO
-- Purpose: Retrieve all lines for a PO Number
-- Module: Module_Receiving
-- Created: 2026-01-25
-- ==============================================================================

CREATE PROCEDURE [dbo].[sp_Receiving_Line_SelectByPO]
    @p_PONumber NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        IF @p_PONumber IS NULL OR @p_PONumber = ''
        BEGIN
            SELECT 0 AS IsSuccess, 'PONumber is required' AS ErrorMessage;
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
            [CreatedBy],
            [CreatedDate]
        FROM [dbo].[tbl_Receiving_Line]
        WHERE [PONumber] = @p_PONumber
          AND [IsActive] = 1 
          AND [IsDeleted] = 0
        ORDER BY [CreatedDate] DESC, [LineNumber] ASC;
        
        SELECT 1 AS IsSuccess, '' AS ErrorMessage;
    END TRY
    BEGIN CATCH
        SELECT 0 AS IsSuccess, ERROR_MESSAGE() AS ErrorMessage;
    END CATCH
END
GO

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Retrieves all lines for a specific PO Number, ordered by date and line number.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'PROCEDURE', @level1name = N'sp_Receiving_Line_SelectByPO';
GO
