-- =====================================================
-- Stored Procedure: sp_Volvo_Shipment_GetById
-- =====================================================
-- Purpose: Get a specific shipment by ID
-- Database: mtm_receiving_application
-- =====================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Volvo_Shipment_GetById`$$

CREATE PROCEDURE `sp_Volvo_Shipment_GetById`(
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
    is_archived
  FROM volvo_label_data
  WHERE id = p_id
  LIMIT 1;
END $$

DELIMITER ;