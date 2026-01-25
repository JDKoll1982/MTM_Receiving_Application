-- ==============================================================================
-- Stored Procedure: sp_Receiving_Settings_Upsert
-- Purpose: Insert or update a setting
-- Module: Module_Receiving
-- Created: 2026-01-25
-- ==============================================================================

CREATE PROCEDURE [dbo].[sp_Receiving_Settings_Upsert]
    @p_SettingKey NVARCHAR(100),
    @p_SettingValue NVARCHAR(500),
    @p_SettingType NVARCHAR(20),
    @p_Category NVARCHAR(50),
    @p_Description NVARCHAR(500) = NULL,
    @p_Scope NVARCHAR(20) = 'System',
    @p_ScopeUserId NVARCHAR(100) = NULL,
    @p_ValidValues NVARCHAR(500) = NULL,
    @p_MinValue DECIMAL(18, 2) = NULL,
    @p_MaxValue DECIMAL(18, 2) = NULL,
    @p_RequiresRestart BIT = 0,
    @p_ModifiedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    
    BEGIN TRY
        IF @p_SettingKey IS NULL OR @p_SettingKey = ''
        BEGIN
            SELECT 0 AS IsSuccess, 'SettingKey is required' AS ErrorMessage;
            RETURN;
        END
        
        IF @p_SettingType NOT IN ('String', 'Integer', 'Boolean', 'Decimal')
        BEGIN
            SELECT 0 AS IsSuccess, 'Invalid SettingType. Must be String, Integer, Boolean, or Decimal' AS ErrorMessage;
            RETURN;
        END
        
        BEGIN TRANSACTION;
        
        -- Check if setting exists
        IF EXISTS (
            SELECT 1 FROM [dbo].[tbl_Receiving_Settings]
            WHERE [SettingKey] = @p_SettingKey
              AND [Scope] = @p_Scope
              AND ((@p_ScopeUserId IS NULL AND [ScopeUserId] IS NULL) OR [ScopeUserId] = @p_ScopeUserId)
        )
        BEGIN
            -- Update existing
            UPDATE [dbo].[tbl_Receiving_Settings]
            SET
                [SettingValue] = @p_SettingValue,
                [SettingType] = @p_SettingType,
                [Category] = @p_Category,
                [Description] = @p_Description,
                [ValidValues] = @p_ValidValues,
                [MinValue] = @p_MinValue,
                [MaxValue] = @p_MaxValue,
                [RequiresRestart] = @p_RequiresRestart,
                [ModifiedBy] = @p_ModifiedBy,
                [ModifiedDate] = GETUTCDATE()
            WHERE [SettingKey] = @p_SettingKey
              AND [Scope] = @p_Scope
              AND ((@p_ScopeUserId IS NULL AND [ScopeUserId] IS NULL) OR [ScopeUserId] = @p_ScopeUserId);
        END
        ELSE
        BEGIN
            -- Insert new
            INSERT INTO [dbo].[tbl_Receiving_Settings] (
                [SettingKey], [SettingValue], [SettingType], [Category], [Description],
                [Scope], [ScopeUserId], [ValidValues], [MinValue], [MaxValue],
                [RequiresRestart], [IsActive], [IsDeleted], [CreatedBy], [CreatedDate]
            )
            VALUES (
                @p_SettingKey, @p_SettingValue, @p_SettingType, @p_Category, @p_Description,
                @p_Scope, @p_ScopeUserId, @p_ValidValues, @p_MinValue, @p_MaxValue,
                @p_RequiresRestart, 1, 0, @p_ModifiedBy, GETUTCDATE()
            );
        END
        
        COMMIT TRANSACTION;
        
        SELECT 1 AS IsSuccess, '' AS ErrorMessage;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        
        SELECT 0 AS IsSuccess, ERROR_MESSAGE() AS ErrorMessage;
    END CATCH
END
GO

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Inserts or updates a setting. If setting exists, updates it; otherwise creates new.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'PROCEDURE', @level1name = N'sp_Receiving_Settings_Upsert';
GO
