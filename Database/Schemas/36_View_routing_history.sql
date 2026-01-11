-- ============================================================================
-- View: view_routing_history
-- Module: Reporting
-- Purpose: Flattened view of routing labels for reporting
-- ============================================================================

CREATE OR REPLACE VIEW view_routing_history AS
SELECT
    rl.id,
    rl.po_number,
    rl.line_number,
    rl.description AS package_description,
    rr.name AS deliver_to,
    rr.department,
    rr.location,
    rl.quantity,
    rl.created_by AS employee_number,
    rl.created_date,
    CASE
        WHEN rl.other_reason_id IS NOT NULL THEN ror.description
        ELSE NULL
    END AS other_reason,
    'Routing' AS source_module
FROM routing_label_data rl
INNER JOIN routing_recipients rr ON rl.recipient_id = rr.id
LEFT JOIN routing_po_alternatives ror ON rl.other_reason_id = ror.id
WHERE rl.is_active = 1
ORDER BY rl.created_date DESC, rl.id DESC;

-- ============================================================================
