-- ==============================================================================
-- Stored Procedure: sp_Receiving_PartType_SelectAll
-- Purpose: Retrieve all active part types
-- Module: Module_Receiving
-- Created: 2026-01-25
-- ==============================================================================

CREATE PROCEDURE [dbo].[sp_Receiving_PartType_SelectAll]
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        SELECT 
            [PartTypeId],
            [PartTypeName],
            [PartTypeCode],
            [Description],
            [PartPrefixes],
            [RequiresDiameter],
            [RequiresWidth],
            [RequiresLength],
            [RequiresThickness],
            [RequiresWeight],
            [SortOrder],
            [IsSystemDefault]
        FROM [dbo].[tbl_Receiving_PartType]
        WHERE [IsActive] = 1 
          AND [IsDeleted] = 0
        ORDER BY [SortOrder] ASC, [PartTypeName] ASC;
        
        SELECT 1 AS IsSuccess, '' AS ErrorMessage;
    END TRY
    BEGIN CATCH
        SELECT 0 AS IsSuccess, ERROR_MESSAGE() AS ErrorMessage;
    END CATCH
END
GO

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Retrieves all active part types with measurement requirements, ordered by sort order.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'PROCEDURE', @level1name = N'sp_Receiving_PartType_SelectAll';
GO
