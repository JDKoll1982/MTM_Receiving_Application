USE mtm_receiving_application_test;

CREATE OR REPLACE VIEW view_receiving_scanner_active_sessions AS
SELECT
    s.id,
    s.user_id,
    s.session_name,
    s.status,
    s.stop_requested,
    s.sent_count,
    s.failed_count,
    s.waiting_count,
    s.last_message,
    s.profile_id,
    p.profile_name,
    p.app_window_title,
    s.updated_at
FROM receiving_scanner_session s
LEFT JOIN receiving_scanner_profile p
    ON p.id = s.profile_id
WHERE s.status IN ('Draft', 'Ready', 'Running', 'Stopped');