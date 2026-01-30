-- ==============================================================================
-- Stored Procedure: sp_Receiving_Line_SelectByTransaction
-- Purpose: Retrieve all lines for a transaction
-- Module: Module_Receiving
-- Created: 2026-01-25
-- ==============================================================================

CREATE PROCEDURE [dbo].[sp_Receiving_Line_SelectByTransaction]
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
        WHERE [TransactionId] = @p_TransactionId
          AND [IsActive] = 1 
          AND [IsDeleted] = 0
        ORDER BY [LineNumber] ASC;
        
        SELECT 1 AS IsSuccess, '' AS ErrorMessage;
    END TRY
    BEGIN CATCH
        SELECT 0 AS IsSuccess, ERROR_MESSAGE() AS ErrorMessage;
    END CATCH
END
GO

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Retrieves all lines for a specific transaction, ordered by line number.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'PROCEDURE', @level1name = N'sp_Receiving_Line_SelectByTransaction';
GO
