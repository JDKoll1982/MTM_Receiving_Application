-- ============================================================================
-- View: view_volvo_shipments_history
-- Module: Volvo
-- Purpose: Flattened view for reporting with part details
-- ============================================================================

CREATE OR REPLACE VIEW view_volvo_shipments_history AS
SELECT
  s.id as shipment_id,
  s.shipment_date,
  s.shipment_number,
  s.po_number,
  s.receiver_number,
  s.status,
  l.part_number,
  l.received_skid_count,
  l.calculated_piece_count,
  l.has_discrepancy,
  l.expected_skid_count,
  l.discrepancy_note
FROM volvo_shipments s
LEFT JOIN volvo_shipment_lines l ON s.id = l.shipment_id
ORDER BY s.shipment_date DESC, s.shipment_number DESC;

-- ============================================================================
