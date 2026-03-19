-- ============================================================================
-- View: view_volvo_history
-- Module: Reporting
-- Purpose: Flattened view of Volvo shipments for reporting
-- ============================================================================

CREATE OR REPLACE VIEW view_volvo_history AS
SELECT
    CAST(vsl.id AS CHAR(20)) AS id,
    vs.po_number,
    vsl.part_number,
    CAST(vsl.calculated_piece_count AS DECIMAL(10,2)) AS quantity,
    DATE(vs.created_date) AS created_date,
    vs.employee_number,
    COALESCE(vsl.discrepancy_note, vs.notes) AS notes,
    vs.shipment_number,
    vs.receiver_number,
    vs.status,
    1 AS part_count,
    vsl.location,
    vsl.quantity_per_skid,
    vsl.received_skid_count,
    'Volvo' AS source_module
FROM volvo_label_data vs
INNER JOIN volvo_line_data vsl ON vs.id = vsl.shipment_id

UNION ALL

SELECT
    CAST(vlnh.id AS CHAR(20)) AS id,
    vlh.po_number,
    vlnh.part_number,
    CAST(vlnh.calculated_piece_count AS DECIMAL(10,2)) AS quantity,
    DATE(vlh.created_date) AS created_date,
    vlh.employee_number,
    COALESCE(vlnh.discrepancy_note, vlh.notes) AS notes,
    vlh.shipment_number,
    vlh.receiver_number,
    vlh.status,
    1 AS part_count,
    vlnh.location,
    vlnh.quantity_per_skid,
    vlnh.received_skid_count,
    'Volvo' AS source_module
FROM volvo_label_history vlh
INNER JOIN volvo_line_history vlnh ON vlh.id = vlnh.shipment_history_id

ORDER BY created_date DESC;

-- ============================================================================
