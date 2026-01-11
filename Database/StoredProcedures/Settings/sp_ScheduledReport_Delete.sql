-- =============================================
-- Stored Procedure: sp_ScheduledReport_Delete
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Delete Scheduled Report
-- =============================================
DROP PROCEDURE IF EXISTS sp_ScheduledReport_Delete$$
CREATE PROCEDURE sp_ScheduledReport_Delete(
    IN p_id INT
)
BEGIN
    -- Soft delete
    UPDATE scheduled_reports
    SET is_active = FALSE,
        updated_at = CURRENT_TIMESTAMP
    WHERE id = p_id;
    
    SELECT ROW_COUNT() AS affected_rows;
END$$



DELIMITER ;
