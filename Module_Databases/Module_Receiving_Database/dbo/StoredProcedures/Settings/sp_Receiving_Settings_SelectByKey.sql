-- ==============================================================================
-- Stored Procedure: sp_Receiving_Settings_SelectByKey
-- Purpose: Retrieve a setting value by key
-- Module: Module_Receiving
-- Created: 2026-01-25
-- ==============================================================================

CREATE PROCEDURE [dbo].[sp_Receiving_Settings_SelectByKey]
    @p_SettingKey NVARCHAR(100),
    @p_Scope NVARCHAR(20) = 'System',
    @p_ScopeUserId NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        IF @p_SettingKey IS NULL OR @p_SettingKey = ''
        BEGIN
            SELECT 0 AS IsSuccess, 'SettingKey is required' AS ErrorMessage;
            RETURN;
        END
        
        SELECT 
            [SettingId],
            [SettingKey],
            [SettingValue],
            [SettingType],
            [Category],
            [Description],
            [Scope],
            [ScopeUserId],
            [ValidValues],
            [MinValue],
            [MaxValue],
            [RequiresRestart]
        FROM [dbo].[tbl_Receiving_Settings]
        WHERE [SettingKey] = @p_SettingKey
          AND [Scope] = @p_Scope
          AND ((@p_ScopeUserId IS NULL AND [ScopeUserId] IS NULL) OR [ScopeUserId] = @p_ScopeUserId)
          AND [IsActive] = 1 
          AND [IsDeleted] = 0;
        
        IF @@ROWCOUNT = 0
        BEGIN
            SELECT 0 AS IsSuccess, 'Setting not found' AS ErrorMessage;
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
    @value = N'Retrieves a setting value by key, scope, and optional user ID.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'PROCEDURE', @level1name = N'sp_Receiving_Settings_SelectByKey';
GO
