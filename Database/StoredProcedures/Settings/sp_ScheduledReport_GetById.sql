-- =============================================
-- Stored Procedure: sp_ScheduledReport_GetById
-- =============================================

DELIMITER $$

-- =============================================
-- SP: Get Scheduled Report by ID
-- =============================================
DROP PROCEDURE IF EXISTS sp_ScheduledReport_GetById$$
CREATE PROCEDURE sp_ScheduledReport_GetById(
    IN p_id INT
)
BEGIN
    SELECT
        id,
        report_type,
        schedule,
        email_recipients,
        is_active,
        next_run_date,
        last_run_date,
        created_at,
        updated_at,
        created_by
    FROM reporting_scheduled_reports
    WHERE id = p_id;
END$$



DELIMITER ;
