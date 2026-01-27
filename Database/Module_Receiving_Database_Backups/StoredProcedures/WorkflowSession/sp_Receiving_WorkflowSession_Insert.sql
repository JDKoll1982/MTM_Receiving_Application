-- ==============================================================================
-- Stored Procedure: sp_Receiving_WorkflowSession_Insert
-- Purpose: Create a new workflow session
-- Module: Module_Receiving
-- Created: 2026-01-25
-- ==============================================================================

CREATE PROCEDURE [dbo].[sp_Receiving_WorkflowSession_Insert]
    @p_SessionId CHAR(36),
    @p_WorkflowMode NVARCHAR(20),
    @p_PONumber NVARCHAR(50) = NULL,
    @p_PartNumber NVARCHAR(50) = NULL,
    @p_LoadCount INT = NULL,
    @p_IsNonPO BIT = 0,
    @p_CreatedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    
    BEGIN TRY
        IF @p_SessionId IS NULL OR @p_SessionId = ''
        BEGIN
            SELECT 0 AS IsSuccess, 'SessionId is required' AS ErrorMessage;
            RETURN;
        END
        
        IF @p_WorkflowMode NOT IN ('Wizard', 'Manual')
        BEGIN
            SELECT 0 AS IsSuccess, 'Invalid WorkflowMode. Must be Wizard or Manual' AS ErrorMessage;
            RETURN;
        END
        
        BEGIN TRANSACTION;
        
        INSERT INTO [dbo].[tbl_Receiving_WorkflowSession] (
            [SessionId], [WorkflowMode], [CurrentStep], [SessionStatus],
            [PONumber], [PartNumber], [LoadCount], [IsNonPO],
            [SessionStartDate], [LastActivityDate],
            [IsActive], [IsDeleted], [CreatedBy], [CreatedDate]
        )
        VALUES (
            @p_SessionId, @p_WorkflowMode, 1, 'Active',
            @p_PONumber, @p_PartNumber, @p_LoadCount, @p_IsNonPO,
            GETUTCDATE(), GETUTCDATE(),
            1, 0, @p_CreatedBy, GETUTCDATE()
        );
        
        COMMIT TRANSACTION;
        
        SELECT 1 AS IsSuccess, '' AS ErrorMessage, @p_SessionId AS SessionId;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        
        SELECT 0 AS IsSuccess, ERROR_MESSAGE() AS ErrorMessage, NULL AS SessionId;
    END CATCH
END
GO

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Creates a new workflow session. Returns SessionId on success.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'PROCEDURE', @level1name = N'sp_Receiving_WorkflowSession_Insert';
GO
