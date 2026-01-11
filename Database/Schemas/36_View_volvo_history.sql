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
ORDER BY vs.created_date DESC;

-- ============================================================================
