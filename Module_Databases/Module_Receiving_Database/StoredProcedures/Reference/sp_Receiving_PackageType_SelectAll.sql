-- ==============================================================================
-- Stored Procedure: sp_Receiving_PackageType_SelectAll
-- Purpose: Retrieve all active package types
-- Module: Module_Receiving
-- Created: 2026-01-25
-- ==============================================================================

CREATE PROCEDURE [dbo].[sp_Receiving_PackageType_SelectAll]
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        SELECT 
            [PackageTypeId],
            [PackageTypeName],
            [PackageTypeCode],
            [Description],
            [DefaultPackagesPerLoad],
            [SortOrder],
            [IsSystemDefault]
        FROM [dbo].[tbl_Receiving_PackageType]
        WHERE [IsActive] = 1 
          AND [IsDeleted] = 0
        ORDER BY [SortOrder] ASC, [PackageTypeName] ASC;
        
        SELECT 1 AS IsSuccess, '' AS ErrorMessage;
    END TRY
    BEGIN CATCH
        SELECT 0 AS IsSuccess, ERROR_MESSAGE() AS ErrorMessage;
    END CATCH
END
GO

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Retrieves all active package types, ordered by sort order.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'PROCEDURE', @level1name = N'sp_Receiving_PackageType_SelectAll';
GO
