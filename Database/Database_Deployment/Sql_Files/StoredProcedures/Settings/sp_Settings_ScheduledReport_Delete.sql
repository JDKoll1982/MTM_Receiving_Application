-- =============================================
-- Stored Procedure: sp_Settings_ScheduledReport_Delete
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Delete Scheduled Report
-- =============================================
DROP PROCEDURE IF EXISTS `sp_Settings_ScheduledReport_Delete` $$
CREATE PROCEDURE `sp_Settings_ScheduledReport_Delete`(
    IN p_id INT
)
BEGIN
    -- Soft delete (use 0/1 and NOW() for MySQL 5.7 compatibility)
    UPDATE reporting_scheduled_reports
    SET is_active = 0,
        updated_at = NOW()
    WHERE id = p_id;

    SELECT ROW_COUNT() AS affected_rows;
END $$
DELIMITER ;
