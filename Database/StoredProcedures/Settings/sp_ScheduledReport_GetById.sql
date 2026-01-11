-- Stored Procedure: sp_ScheduledReport_GetById
-- Compatible with MySQL 5.7
-- Run in the mtm_receiving_application database (e.g., USE mtm_receiving_application;)

DELIMITER $$
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
    WHERE id = p_id
    LIMIT 1;
END$$
DELIMITER ;
