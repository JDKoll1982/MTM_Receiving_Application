-- =====================================================
-- Stored Procedure: sp_Volvo_Shipment_GetPending
-- =====================================================
-- Purpose: Get pending shipment if one exists
-- Database: mtm_receiving_application
-- =====================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Volvo_Shipment_GetPending`$$

CREATE PROCEDURE `sp_Volvo_Shipment_GetPending`()
BEGIN
  SELECT
    id, shipment_date, shipment_number, po_number, receiver_number,
    employee_number, notes, status, created_date, modified_date, is_archived
  FROM volvo_label_data
  WHERE status = 'pending_po' AND is_archived = 0
  LIMIT 1;
END$$

DELIMITER ;
