-- ============================================================================
-- View: view_dunnage_history
-- Module: Reporting
-- Purpose: Flattened view of dunnage loads for reporting
-- ============================================================================

CREATE OR REPLACE VIEW view_dunnage_history AS
SELECT
    dl.load_uuid AS id,
    dt.type_name AS dunnage_type,
    dp.part_id AS part_number,
    GROUP_CONCAT(
        CONCAT(ds.spec_key, ':', COALESCE(JSON_UNQUOTE(ds.spec_value), ''))
        ORDER BY ds.spec_key
        SEPARATOR ', '
    ) AS specs_combined,
    dl.quantity,
    DATE(dl.received_date) AS created_date,
    dl.created_by AS employee_number,
    'Dunnage' AS source_module
FROM dunnage_history dl
INNER JOIN dunnage_parts dp ON dl.part_id = dp.part_id
INNER JOIN dunnage_types dt ON dp.type_id = dt.id
LEFT JOIN dunnage_specs ds ON dp.type_id = ds.type_id
GROUP BY dl.load_uuid, dt.type_name, dp.part_id, dl.quantity, dl.received_date, dl.created_by
ORDER BY dl.received_date DESC;

-- ============================================================================
