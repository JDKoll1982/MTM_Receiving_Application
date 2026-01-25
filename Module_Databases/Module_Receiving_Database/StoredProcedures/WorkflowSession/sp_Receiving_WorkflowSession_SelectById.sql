-- ==============================================================================
-- Stored Procedure: sp_Receiving_WorkflowSession_SelectById
-- Purpose: Retrieve workflow session by ID
-- Module: Module_Receiving
-- Created: 2026-01-25
-- ==============================================================================

CREATE PROCEDURE [dbo].[sp_Receiving_WorkflowSession_SelectById]
    @p_SessionId CHAR(36)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        IF @p_SessionId IS NULL OR @p_SessionId = ''
        BEGIN
            SELECT 0 AS IsSuccess, 'SessionId is required' AS ErrorMessage;
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
            [DefaultReceivingLocation],
            [DefaultPackageType],
            [DefaultPackagesPerLoad],
            [LoadDetailsJson],
            [Step1Valid],
            [Step2Valid],
            [AllStepsValid],
            [SessionStartDate],
            [SessionEndDate],
            [LastActivityDate],
            [CreatedBy],
            [CreatedDate],
            [ModifiedBy],
            [ModifiedDate]
        FROM [dbo].[tbl_Receiving_WorkflowSession]
        WHERE [SessionId] = @p_SessionId
          AND [IsActive] = 1 
          AND [IsDeleted] = 0;
        
        IF @@ROWCOUNT = 0
        BEGIN
            SELECT 0 AS IsSuccess, 'Session not found' AS ErrorMessage;
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
    @value = N'Retrieves workflow session by SessionId.',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'PROCEDURE', @level1name = N'sp_Receiving_WorkflowSession_SelectById';
GO
