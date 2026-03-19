-- ============================================================================
-- View: view_volvo_label_data_history
-- Module: Volvo
-- Purpose: Flattened view for reporting with part details
-- ============================================================================

CREATE OR REPLACE VIEW view_volvo_label_data_history AS
SELECT
  s.id as shipment_id,
  s.shipment_date,
  s.shipment_number,
  s.po_number,
  s.receiver_number,
  s.status,
  l.part_number,
  l.location,
  l.received_skid_count,
  l.calculated_piece_count,
  l.has_discrepancy,
  l.expected_skid_count,
  l.discrepancy_note
FROM volvo_label_data s
LEFT JOIN volvo_line_data l ON s.id = l.shipment_id

UNION ALL

SELECT
  vlh.id as shipment_id,
  vlh.shipment_date,
  vlh.shipment_number,
  vlh.po_number,
  vlh.receiver_number,
  vlh.status,
  vlnh.part_number,
  vlnh.location,
  vlnh.received_skid_count,
  vlnh.calculated_piece_count,
  vlnh.has_discrepancy,
  vlnh.expected_skid_count,
  vlnh.discrepancy_note
FROM volvo_label_history vlh
LEFT JOIN volvo_line_history vlnh ON vlh.id = vlnh.shipment_history_id

ORDER BY shipment_date DESC, shipment_number DESC;

-- ============================================================================
