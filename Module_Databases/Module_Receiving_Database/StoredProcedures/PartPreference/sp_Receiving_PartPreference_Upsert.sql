-- ==============================================================================
-- Stored Procedure: sp_Receiving_PartPreference_Upsert
-- Purpose: Insert or update part preferences
-- Module: Module_Receiving
-- Created: 2026-01-25
-- ==============================================================================

CREATE PROCEDURE [dbo].[sp_Receiving_PartPreference_Upsert]
    @p_PartNumber NVARCHAR(50),
    @p_PartTypeId INT = NULL,
    @p_DefaultReceivingLocation NVARCHAR(100) = NULL,
    @p_DefaultPackageType NVARCHAR(50) = NULL,
    @p_DefaultPackagesPerLoad INT = NULL,
    @p_RequiresQualityHold BIT = 0,
    @p_QualityHoldProcedure NVARCHAR(MAX) = NULL,
    @p_Scope NVARCHAR(20) = 'System',
    @p_ScopeUserId NVARCHAR(100) = NULL,
    @p_ModifiedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    
    BEGIN TRY
        IF @p_PartNumber IS NULL OR @p_PartNumber = ''
        BEGIN
            SELECT 0 AS IsSuccess, 'PartNumber is required' AS ErrorMessage;
            RETURN;
        END
        
        IF @p_Scope NOT IN ('System', 'User')
        BEGIN
            SELECT 0 AS IsSuccess, 'Invalid Scope. Must be System or User' AS ErrorMessage;
            RETURN;
        END
        
        BEGIN TRANSACTION;
        
        -- Check if preference exists
        IF EXISTS (
            SELECT 1 FROM [dbo].[tbl_Receiving_PartPreference]
            WHERE [PartNumber] = @p_PartNumber
              AND [Scope] = @p_Scope
              AND ((@p_ScopeUserId IS NULL AND [ScopeUserId] IS NULL) OR [ScopeUserId] = @p_ScopeUserId)
        )
        BEGIN
            -- Update existing
            UPDATE [dbo].[tbl_Receiving_PartPreference]
            SET
                [PartTypeId] = @p_PartTypeId,
                [DefaultReceivingLocation] = @p_DefaultReceivingLocation,
                [DefaultPackageType] = @p_DefaultPackageType,
                [DefaultPackagesPerLoad] = @p_DefaultPackagesPerLoad,
                [RequiresQualityHold] = @p_RequiresQualityHold,
                [QualityHoldProcedure] = @p_QualityHoldProcedure,
                [ModifiedBy] = @p_ModifiedBy,
                [ModifiedDate] = GETUTCDATE()
            WHERE [PartNumber] = @p_PartNumber
              AND [Scope] = @p_Scope
              AND ((@p_ScopeUserId IS NULL AND [ScopeUserId] IS NULL) OR [ScopeUserId] = @p_ScopeUserId);
        END
        ELSE
        BEGIN
            -- Insert new
            INSERT INTO [dbo].[tbl_Receiving_PartPreference] (
                [PartNumber], [PartTypeId], [DefaultReceivingLocation], [DefaultPackageType],
                [DefaultPackagesPerLoad], [RequiresQualityHold], [QualityHoldProcedure],
                [Scope], [ScopeUserId], [IsActive], [IsDeleted], [CreatedBy], [CreatedDate]
            )
            VALUES (
                @p_PartNumber, @p_PartTypeId, @p_DefaultReceivingLocation, @p_DefaultPackageType,
                @p_DefaultPackagesPerLoad, @p_RequiresQualityHold, @p_QualityHoldProcedure,
                @p_Scope, @p_ScopeUserId, 1, 0, @p_ModifiedBy, GETUTCDATE()
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
    @value = N'Inserts or updates part preferences. If preference exists, updates it; otherwise creates new.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'PROCEDURE', @level1name = N'sp_Receiving_PartPreference_Upsert';
GO
