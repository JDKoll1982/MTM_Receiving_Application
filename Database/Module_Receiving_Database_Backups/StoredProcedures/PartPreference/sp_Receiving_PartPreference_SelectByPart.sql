-- ==============================================================================
-- Stored Procedure: sp_Receiving_PartPreference_SelectByPart
-- Purpose: Retrieve part preferences by part number
-- Module: Module_Receiving
-- Created: 2026-01-25
-- ==============================================================================

CREATE PROCEDURE [dbo].[sp_Receiving_PartPreference_SelectByPart]
    @p_PartNumber NVARCHAR(50),
    @p_Scope NVARCHAR(20) = 'System',
    @p_ScopeUserId NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        IF @p_PartNumber IS NULL OR @p_PartNumber = ''
        BEGIN
            SELECT 0 AS IsSuccess, 'PartNumber is required' AS ErrorMessage;
            RETURN;
        END
        
        SELECT 
            pp.[PartPreferenceId],
            pp.[PartNumber],
            pp.[PartTypeId],
            pt.[PartTypeName],
            pt.[PartTypeCode],
            pp.[DefaultReceivingLocation],
            pp.[DefaultPackageType],
            pp.[DefaultPackagesPerLoad],
            pp.[RequiresQualityHold],
            pp.[QualityHoldProcedure],
            pp.[Scope],
            pp.[ScopeUserId]
        FROM [dbo].[tbl_Receiving_PartPreference] pp
        LEFT JOIN [dbo].[tbl_Receiving_PartType] pt ON pp.[PartTypeId] = pt.[PartTypeId]
        WHERE pp.[PartNumber] = @p_PartNumber
          AND pp.[Scope] = @p_Scope
          AND ((@p_ScopeUserId IS NULL AND pp.[ScopeUserId] IS NULL) OR pp.[ScopeUserId] = @p_ScopeUserId)
          AND pp.[IsActive] = 1 
          AND pp.[IsDeleted] = 0;
        
        IF @@ROWCOUNT = 0
        BEGIN
            SELECT 0 AS IsSuccess, 'Part preference not found' AS ErrorMessage;
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
    @value = N'Retrieves part preferences for a specific part number, including part type details.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'PROCEDURE', @level1name = N'sp_Receiving_PartPreference_SelectByPart';
GO
