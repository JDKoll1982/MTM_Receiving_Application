CREATE
OR REPLACE VIEW view_dunnage_history AS
SELECT
    dl.load_uuid AS id,
    CAST(NULL AS CHAR(20)) AS po_number,
    COALESCE(dt.type_name, dl.dunnage_type_name, dl.type_name) AS dunnage_type,
    dp.part_id AS part_number,
    dl.udc1 AS udc1,
    dl.udc2 AS udc2,
    dl.udc3 AS udc3,
    dl.udc4 AS udc4,
    dl.udc5 AS udc5,
    dl.udc6 AS udc6,
    dl.udc7 AS udc7,
    dl.udc8 AS udc8,
    dl.udc9 AS udc9,
    dl.udc10 AS udc10,
    dl.quantity,
    DATE(dl.received_date) AS created_date,
    CAST(au.employee_number AS CHAR(20)) AS employee_number,
    dl.created_by AS created_by_username,
    dl.location AS location,
    CAST(NULL AS CHAR(255)) AS notes,
    'Dunnage' AS source_module
FROM
    dunnage_history dl
    LEFT JOIN dunnage_parts dp ON dl.part_id = dp.part_id
    LEFT JOIN dunnage_types dt ON dp.type_id = dt.id
    LEFT JOIN auth_users au ON au.windows_username = dl.created_by
    AND au.is_active = TRUE
ORDER BY
    dl.received_date DESC;