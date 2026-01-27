-- ==============================================================================
-- Stored Procedure: sp_Receiving_Transaction_SelectByDateRange
-- Purpose: Retrieve transactions within a date range
-- Module: Module_Receiving
-- Created: 2026-01-25
-- ==============================================================================

CREATE PROCEDURE [dbo].[sp_Receiving_Transaction_SelectByDateRange]
    @p_StartDate DATETIME2,
    @p_EndDate DATETIME2,
    @p_Status NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        SELECT 
            [TransactionId],
            [PONumber],
            [PartNumber],
            [TotalLoads],
            [TotalWeight],
            [TotalQuantity],
            [WorkflowMode],
            [Status],
            [CompletedDate],
            [IsNonPO],
            [CreatedBy],
            [CreatedDate]
        FROM [dbo].[tbl_Receiving_Transaction]
        WHERE [CreatedDate] >= @p_StartDate
          AND [CreatedDate] <= @p_EndDate
          AND ([Status] = @p_Status OR @p_Status IS NULL)
          AND [IsActive] = 1 
          AND [IsDeleted] = 0
        ORDER BY [CreatedDate] DESC;
        
        SELECT 1 AS IsSuccess, '' AS ErrorMessage;
    END TRY
    BEGIN CATCH
        SELECT 0 AS IsSuccess, ERROR_MESSAGE() AS ErrorMessage;
    END CATCH
END
GO

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Retrieves transactions within a date range. Optional status filter.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'PROCEDURE', @level1name = N'sp_Receiving_Transaction_SelectByDateRange';
GO
