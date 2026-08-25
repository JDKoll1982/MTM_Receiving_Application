-- =====================================================
-- Stored Procedure: sp_Volvo_ShipmentHistory_GetById
-- =====================================================
-- Purpose: Get a specific archived shipment header by history ID
-- Database: mtm_receiving_application_test
-- =====================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Volvo_ShipmentHistory_GetById`$$

CREATE PROCEDURE `sp_Volvo_ShipmentHistory_GetById`(
  IN p_id INT
)
BEGIN
  SELECT
    id,
    shipment_date,
    shipment_number,
    po_number,
    receiver_number,
    employee_number,
    notes,
    status,
    created_date,
    modified_date,
    1 AS is_archived
  FROM volvo_label_history
  WHERE id = p_id
  LIMIT 1;
END $$

DELIMITER ;