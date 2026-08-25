-- =====================================================
-- Stored Procedure: sp_Volvo_Shipment_GetHistory
-- =====================================================
-- Purpose: Get shipment history with filtering
-- Database: mtm_receiving_application_test
-- =====================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Volvo_Shipment_GetHistory`$$

CREATE PROCEDURE `sp_Volvo_Shipment_GetHistory`(
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

  UNION ALL

  SELECT
    h.id, h.shipment_date, h.shipment_number, h.po_number, h.receiver_number,
    h.employee_number, h.notes, h.status, h.created_date, h.modified_date,
    1 AS is_archived,
    COUNT(lh.id) AS part_count
  FROM volvo_label_history h
  LEFT JOIN volvo_line_history lh ON h.id = lh.shipment_history_id
  WHERE h.shipment_date BETWEEN p_start_date AND p_end_date
    AND (p_status = 'all' OR h.status = p_status)
  GROUP BY h.id

  ORDER BY shipment_date DESC, shipment_number DESC;
END $$

DELIMITER ;
