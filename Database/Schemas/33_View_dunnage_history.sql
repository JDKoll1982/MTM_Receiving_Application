-- ============================================================================
-- View: view_dunnage_history
-- Module: Reporting
-- Purpose: Flattened view of dunnage loads for reporting
-- ============================================================================

CREATE OR REPLACE VIEW view_dunnage_history AS
SELECT
    dl.load_uuid AS id,
    COALESCE(dl.type_name, dt.type_name) AS dunnage_type,
    dp.part_id AS part_number,
    COALESCE(
        NULLIF(
            REPLACE(
                REPLACE(
                    REPLACE(
                        REPLACE(CAST(dl.specs_json AS CHAR(1000)), '{', ''),
                        '}', ''),
                    '"', ''),
                ',', ', '),
            ''),
        GROUP_CONCAT(
            CONCAT(ds.spec_key, ':', COALESCE(JSON_UNQUOTE(ds.spec_value), ''))
            ORDER BY ds.spec_key
            SEPARATOR ', '
        )) AS specs_combined,
    dl.quantity,
    DATE(dl.received_date) AS created_date,
    CAST(au.employee_number AS CHAR(20)) AS employee_number,
    dl.created_by AS created_by_username,
    'Dunnage' AS source_module
FROM dunnage_history dl
INNER JOIN dunnage_parts dp ON dl.part_id = dp.part_id
INNER JOIN dunnage_types dt ON dp.type_id = dt.id
LEFT JOIN dunnage_specs ds ON dp.type_id = ds.type_id
LEFT JOIN auth_users au ON au.windows_username = dl.created_by AND au.is_active = TRUE
GROUP BY dl.load_uuid, dl.type_name, dt.type_name, dp.part_id, dl.specs_json, dl.quantity, dl.received_date, dl.created_by, au.employee_number
ORDER BY dl.received_date DESC;

-- ============================================================================
