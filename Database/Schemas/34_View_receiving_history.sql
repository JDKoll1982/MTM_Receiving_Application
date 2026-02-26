-- ============================================================================
-- View: view_receiving_history
-- Module: Reporting
-- Purpose: Unified view combining receiving_history and receiving_label_data
--          for reporting across all receiving transactions.
-- ============================================================================

CREATE OR REPLACE VIEW view_receiving_history AS
SELECT
    id,
    quantity,
    part_id,
    po_number,
    employee_number,
    heat,
    transaction_date,
    initial_location,
    coils_on_skid,
    label_number,
    vendor_name,
    part_description,
    created_at,
    'history' AS source
FROM receiving_history

UNION ALL

SELECT
    id,
    quantity,
    part_id,
    CAST(po_number AS CHAR) AS po_number,
    employee_number,
    heat,
    transaction_date,
    initial_location,
    coils_on_skid,
    label_number,
    vendor_name,
    part_description,
    created_at,
    'label_data' AS source
FROM receiving_label_data

ORDER BY transaction_date DESC, id DESC;

-- ============================================================================
