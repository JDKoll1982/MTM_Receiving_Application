-- =============================================
-- Stored Procedure: sp_Receiving_WorkflowSession_SelectByUserId
-- Description: Retrieve active workflow session for a user
-- Used by: Session recovery on app startup
-- =============================================
CREATE PROCEDURE [dbo].[sp_Receiving_WorkflowSession_SelectByUserId]
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP 1
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
    WHERE user_id = @UserId
      AND expires_at > GETDATE()
    ORDER BY updated_at DESC; -- Most recently updated session
END
GO
