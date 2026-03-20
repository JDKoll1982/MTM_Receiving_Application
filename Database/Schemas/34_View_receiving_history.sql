-- ============================================================================
-- View: view_receiving_history
-- Module: Reporting
-- Purpose: Unified view combining receiving_history and receiving_label_data
--          for reporting across all receiving transactions.
-- ============================================================================

CREATE OR REPLACE VIEW view_receiving_history AS
SELECT
    id,
    po_number,
    po_line_number,
    part_id AS part_number,
    part_description,
    quantity,
    CAST(NULL AS DECIMAL(18,2)) AS weight_lbs,
    heat AS heat_lot_number,
    DATE(COALESCE(transaction_date, created_at)) AS created_date,
    employee_number,
    NULL AS load_number,
    initial_location,
    NULL AS packages_per_load,
    NULL AS package_type_name,
    coils_on_skid,
    label_number,
    vendor_name,
    is_non_po_item,
    NULL AS notes,
    'Receiving' AS source_module
FROM receiving_history

UNION ALL

SELECT
    id,
    po_number,
    po_line_number,
    part_id AS part_number,
    part_description,
    quantity,
    weight_quantity AS weight_lbs,
    heat AS heat_lot_number,
    DATE(COALESCE(transaction_date, created_at)) AS created_date,
    employee_number,
    load_number,
    initial_location,
    packages_per_load,
    package_type_name,
    coils_on_skid,
    label_number,
    vendor_name,
    is_non_po_item,
    NULL AS notes,
    'Receiving' AS source_module
FROM receiving_label_data

ORDER BY created_date DESC, id DESC;

-- ============================================================================
