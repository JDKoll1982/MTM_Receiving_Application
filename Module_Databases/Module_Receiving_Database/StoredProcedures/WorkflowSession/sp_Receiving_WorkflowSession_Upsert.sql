-- =============================================
-- Stored Procedure: sp_Receiving_WorkflowSession_Upsert
-- Description: Insert or update workflow session state
-- Used by: Wizard Mode navigation between steps
-- =============================================
CREATE PROCEDURE [dbo].[sp_Receiving_WorkflowSession_Upsert]
    @SessionId UNIQUEIDENTIFIER,
    @UserId INT,
    @CurrentStep INT,
    @WorkflowMode NVARCHAR(20),
    @IsNonPO BIT,
    @PONumber NVARCHAR(50) = NULL,
    @PartId NVARCHAR(50) = NULL,
    @LoadCount INT = 0,
    @ReceivingLocationOverride NVARCHAR(20) = NULL,
    @LoadDetailsJson NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ExpiresAt DATETIME2 = DATEADD(HOUR, 24, GETDATE());
    
    -- Check if session exists
    IF EXISTS (SELECT 1 FROM workflow_sessions WHERE session_id = @SessionId)
    BEGIN
        -- Update existing session
        UPDATE workflow_sessions
        SET user_id = @UserId,
            current_step = @CurrentStep,
            workflow_mode = @WorkflowMode,
            is_non_po = @IsNonPO,
            po_number = @PONumber,
            part_id = @PartId,
            load_count = @LoadCount,
            receiving_location_override = @ReceivingLocationOverride,
            load_details_json = @LoadDetailsJson,
            updated_at = GETDATE(),
            expires_at = @ExpiresAt
        WHERE session_id = @SessionId;
        
        SELECT 'UPDATED' AS Result, @SessionId AS SessionId;
    END
    ELSE
    BEGIN
        -- Insert new session
        INSERT INTO workflow_sessions (
            session_id, user_id, current_step, workflow_mode, is_non_po,
            po_number, part_id, load_count, receiving_location_override,
            load_details_json, created_at, updated_at, expires_at
        )
        VALUES (
            @SessionId, @UserId, @CurrentStep, @WorkflowMode, @IsNonPO,
            @PONumber, @PartId, @LoadCount, @ReceivingLocationOverride,
            @LoadDetailsJson, GETDATE(), GETDATE(), @ExpiresAt
        );
        
        SELECT 'INSERTED' AS Result, @SessionId AS SessionId;
    END
END
GO
