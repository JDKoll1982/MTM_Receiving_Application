-- =====================================================
-- Stored Procedure: sp_volvo_shipment_history_get
-- =====================================================
-- Purpose: Get shipment history with filtering
-- Database: mtm_receiving_application
-- =====================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS sp_volvo_shipment_history_get$$

CREATE PROCEDURE sp_volvo_shipment_history_get(
  IN p_start_date DATE,
  IN p_end_date DATE,
  IN p_status VARCHAR(20) -- 'pending_po', 'completed', or 'all'
)
BEGIN
  SELECT
    s.id, s.shipment_date, s.shipment_number, s.po_number, s.receiver_number,
    s.employee_number, s.notes, s.status, s.created_date, s.modified_date,
    s.is_archived,
    COUNT(l.id) as part_count
  FROM volvo_label_data s
  LEFT JOIN volvo_line_data l ON s.id = l.shipment_id
  WHERE s.shipment_date BETWEEN p_start_date AND p_end_date
    AND (p_status = 'all' OR s.status = p_status)
  GROUP BY s.id
  ORDER BY s.shipment_date DESC, s.shipment_number DESC;
END$$

DELIMITER ;
