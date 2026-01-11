-- =====================================================
-- Stored Procedure: sp_volvo_shipment_get_pending
-- =====================================================
-- Purpose: Get pending shipment if one exists
-- Database: mtm_receiving_application
-- =====================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS sp_volvo_shipment_get_pending$$

CREATE PROCEDURE sp_volvo_shipment_get_pending()
BEGIN
  SELECT
    id, shipment_date, shipment_number, po_number, receiver_number,
    employee_number, notes, status, created_date, modified_date, is_archived
  FROM volvo_label_data
  WHERE status = 'pending_po' AND is_archived = 0
  LIMIT 1;
END$$

DELIMITER ;
