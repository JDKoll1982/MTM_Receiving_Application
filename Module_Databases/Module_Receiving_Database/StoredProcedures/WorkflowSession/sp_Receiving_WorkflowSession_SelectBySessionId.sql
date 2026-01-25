-- =============================================
-- Stored Procedure: sp_Receiving_WorkflowSession_SelectBySessionId
-- Description: Retrieve workflow session by session ID
-- Used by: Session recovery and step navigation
-- =============================================
CREATE PROCEDURE [dbo].[sp_Receiving_WorkflowSession_SelectBySessionId]
    @SessionId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        session_id,
        user_id,
        current_step,
        workflow_mode,
        is_non_po,
        po_number,
        part_id,
        load_count,
        receiving_location_override,
        load_details_json,
        created_at,
        updated_at,
        expires_at
    FROM workflow_sessions
    WHERE session_id = @SessionId
      AND expires_at > GETDATE(); -- Only return non-expired sessions
END
GO
