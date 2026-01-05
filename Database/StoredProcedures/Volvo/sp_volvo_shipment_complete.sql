-- =====================================================
-- Stored Procedure: sp_volvo_shipment_complete
-- =====================================================
-- Purpose: Mark shipment as completed with PO/Receiver numbers
-- Database: mtm_receiving_application
-- =====================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS sp_volvo_shipment_complete$$

CREATE PROCEDURE sp_volvo_shipment_complete(
  IN p_shipment_id INT,
  IN p_po_number VARCHAR(50),
  IN p_receiver_number VARCHAR(50)
)
BEGIN
  UPDATE volvo_shipments
  SET 
    po_number = p_po_number,
    receiver_number = p_receiver_number,
    status = 'completed',
    is_archived = 1,
    modified_date = CURRENT_TIMESTAMP
  WHERE id = p_shipment_id;
END$$

DELIMITER ;
