-- ==============================================================================
-- Stored Procedure: sp_Receiving_Location_SelectByCode
-- Purpose: Retrieve a specific location by code
-- Module: Module_Receiving
-- Created: 2026-01-25
-- ==============================================================================

CREATE PROCEDURE [dbo].[sp_Receiving_Location_SelectByCode]
    @p_LocationCode NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        IF @p_LocationCode IS NULL OR @p_LocationCode = ''
        BEGIN
            SELECT 0 AS IsSuccess, 'LocationCode is required' AS ErrorMessage;
            RETURN;
        END
        
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
        WHERE [LocationCode] = @p_LocationCode
          AND [IsActive] = 1 
          AND [IsDeleted] = 0;
        
        IF @@ROWCOUNT = 0
        BEGIN
            SELECT 0 AS IsSuccess, 'Location not found' AS ErrorMessage;
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
    @value = N'Retrieves a specific location by location code.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'PROCEDURE', @level1name = N'sp_Receiving_Location_SelectByCode';
GO
