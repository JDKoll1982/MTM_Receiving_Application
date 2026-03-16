-- ============================================================================
-- View: view_volvo_history
-- Module: Reporting
-- Purpose: Flattened view of Volvo shipments for reporting
-- ============================================================================

CREATE OR REPLACE VIEW view_volvo_history AS
SELECT
    vs.id,
    vs.shipment_number,
    vs.shipment_date,
    vs.po_number,
    vs.receiver_number,
    vs.status,
    vs.employee_number,
    vs.notes,
    COUNT(vsl.id) AS part_count,
    vs.created_date,
    'Volvo' AS source_module
FROM volvo_label_data vs
LEFT JOIN volvo_line_data vsl ON vs.id = vsl.shipment_id
GROUP BY vs.id, vs.shipment_number, vs.shipment_date, vs.po_number, vs.receiver_number, vs.status, vs.employee_number, vs.notes, vs.created_date

UNION ALL

SELECT
    vlh.id,
    vlh.shipment_number,
    vlh.shipment_date,
    vlh.po_number,
    vlh.receiver_number,
    vlh.status,
    vlh.employee_number,
    vlh.notes,
    COUNT(vlnh.id) AS part_count,
    vlh.created_date,
    'Volvo' AS source_module
FROM volvo_label_history vlh
LEFT JOIN volvo_line_history vlnh ON vlh.id = vlnh.shipment_history_id
GROUP BY vlh.id, vlh.shipment_number, vlh.shipment_date, vlh.po_number, vlh.receiver_number, vlh.status, vlh.employee_number, vlh.notes, vlh.created_date

ORDER BY created_date DESC;

-- ============================================================================
