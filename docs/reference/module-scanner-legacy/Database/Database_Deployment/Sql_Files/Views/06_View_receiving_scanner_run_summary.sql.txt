USE mtm_receiving_application;

CREATE OR REPLACE VIEW view_receiving_scanner_run_summary AS
SELECT
    r.id,
    r.session_id,
    r.user_id,
    r.profile_id,
    p.profile_name,
    r.run_status,
    r.stop_reason,
    r.sent_count,
    r.failed_count,
    r.waiting_count,
    r.started_at,
    r.ended_at,
    r.summary_message,
    TIMESTAMPDIFF(SECOND, r.started_at, COALESCE(r.ended_at, NOW())) AS duration_seconds
FROM receiving_scanner_run r
LEFT JOIN receiving_scanner_profile p
    ON p.id = r.profile_id;