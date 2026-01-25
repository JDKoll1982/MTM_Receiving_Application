-- ==============================================================================
-- Stored Procedure: sp_Receiving_WorkflowSession_SelectByUser
-- Purpose: Retrieve active sessions for a user
-- Module: Module_Receiving
-- Created: 2026-01-25
-- ==============================================================================

CREATE PROCEDURE [dbo].[sp_Receiving_WorkflowSession_SelectByUser]
    @p_Username NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        IF @p_Username IS NULL OR @p_Username = ''
        BEGIN
            SELECT 0 AS IsSuccess, 'Username is required' AS ErrorMessage;
            RETURN;
        END
        
        SELECT 
            [SessionId],
            [WorkflowMode],
            [CurrentStep],
            [SessionStatus],
            [PONumber],
            [PartNumber],
            [LoadCount],
            [IsNonPO],
            [Step1Valid],
            [Step2Valid],
            [AllStepsValid],
            [SessionStartDate],
            [LastActivityDate],
            [CreatedBy],
            [CreatedDate]
        FROM [dbo].[tbl_Receiving_WorkflowSession]
        WHERE [CreatedBy] = @p_Username
          AND [SessionStatus] = 'Active'
          AND [IsActive] = 1 
          AND [IsDeleted] = 0
        ORDER BY [LastActivityDate] DESC;
        
        SELECT 1 AS IsSuccess, '' AS ErrorMessage;
    END TRY
    BEGIN CATCH
        SELECT 0 AS IsSuccess, ERROR_MESSAGE() AS ErrorMessage;
    END CATCH
END
GO

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Retrieves all active sessions for a user, ordered by last activity.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'PROCEDURE', @level1name = N'sp_Receiving_WorkflowSession_SelectByUser';
GO
