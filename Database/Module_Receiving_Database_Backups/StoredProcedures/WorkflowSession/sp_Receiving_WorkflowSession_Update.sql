-- ==============================================================================
-- Stored Procedure: sp_Receiving_WorkflowSession_Update
-- Purpose: Update workflow session state
-- Module: Module_Receiving
-- Created: 2026-01-25
-- ==============================================================================

CREATE PROCEDURE [dbo].[sp_Receiving_WorkflowSession_Update]
    @p_SessionId CHAR(36),
    @p_CurrentStep INT = NULL,
    @p_SessionStatus NVARCHAR(20) = NULL,
    @p_PONumber NVARCHAR(50) = NULL,
    @p_PartNumber NVARCHAR(50) = NULL,
    @p_LoadCount INT = NULL,
    @p_LoadDetailsJson NVARCHAR(MAX) = NULL,
    @p_Step1Valid BIT = NULL,
    @p_Step2Valid BIT = NULL,
    @p_AllStepsValid BIT = NULL,
    @p_ModifiedBy NVARCHAR(100)
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
        
        IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_Receiving_WorkflowSession] WHERE [SessionId] = @p_SessionId AND [IsActive] = 1 AND [IsDeleted] = 0)
        BEGIN
            SELECT 0 AS IsSuccess, 'Session not found' AS ErrorMessage;
            RETURN;
        END
        
        BEGIN TRANSACTION;
        
        UPDATE [dbo].[tbl_Receiving_WorkflowSession]
        SET
            [CurrentStep] = ISNULL(@p_CurrentStep, [CurrentStep]),
            [SessionStatus] = ISNULL(@p_SessionStatus, [SessionStatus]),
            [PONumber] = ISNULL(@p_PONumber, [PONumber]),
            [PartNumber] = ISNULL(@p_PartNumber, [PartNumber]),
            [LoadCount] = ISNULL(@p_LoadCount, [LoadCount]),
            [LoadDetailsJson] = ISNULL(@p_LoadDetailsJson, [LoadDetailsJson]),
            [Step1Valid] = ISNULL(@p_Step1Valid, [Step1Valid]),
            [Step2Valid] = ISNULL(@p_Step2Valid, [Step2Valid]),
            [AllStepsValid] = ISNULL(@p_AllStepsValid, [AllStepsValid]),
            [LastActivityDate] = GETUTCDATE(),
            [SessionEndDate] = CASE WHEN @p_SessionStatus IN ('Completed', 'Abandoned') THEN GETUTCDATE() ELSE [SessionEndDate] END,
            [ModifiedBy] = @p_ModifiedBy,
            [ModifiedDate] = GETUTCDATE()
        WHERE [SessionId] = @p_SessionId;
        
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
    @value = N'Updates workflow session state. Only updates provided fields.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'PROCEDURE', @level1name = N'sp_Receiving_WorkflowSession_Update';
GO
