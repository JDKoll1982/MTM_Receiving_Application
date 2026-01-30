-- ==============================================================================
-- Stored Procedure: sp_Receiving_Location_SelectAll
-- Purpose: Retrieve all active receiving locations
-- Module: Module_Receiving
-- Created: 2026-01-25
-- ==============================================================================

CREATE PROCEDURE [dbo].[sp_Receiving_Location_SelectAll]
    @p_AllowReceivingOnly BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        SELECT 
            [LocationId],
            [LocationCode],
            [LocationName],
            [Description],
            [Warehouse],
            [Aisle],
            [Bay],
            [Level],
            [InforVisualLocation],
            [IsInforVisualSynced],
            [MaxCapacity],
            [CurrentLoad],
            [AllowReceiving],
            [IsSystemDefault]
        FROM [dbo].[tbl_Receiving_Location]
        WHERE [IsActive] = 1 
          AND [IsDeleted] = 0
          AND (@p_AllowReceivingOnly = 0 OR [AllowReceiving] = 1)
        ORDER BY [LocationCode] ASC;
        
        SELECT 1 AS IsSuccess, '' AS ErrorMessage;
    END TRY
    BEGIN CATCH
        SELECT 0 AS IsSuccess, ERROR_MESSAGE() AS ErrorMessage;
    END CATCH
END
GO

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Retrieves all active receiving locations. Optional filter for locations that allow receiving.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'PROCEDURE', @level1name = N'sp_Receiving_Location_SelectAll';
GO
