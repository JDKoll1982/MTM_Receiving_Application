-- =====================================================
-- Stored Procedure: sp_volvo_shipment_insert
-- =====================================================
-- Purpose: Insert new shipment header and auto-generate shipment_number
-- Database: mtm_receiving_application
-- =====================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS sp_volvo_shipment_insert$$

CREATE PROCEDURE sp_volvo_shipment_insert(
  IN p_shipment_date DATE,
  IN p_employee_number VARCHAR(20),
  IN p_notes TEXT,
  OUT p_new_id INT,
  OUT p_shipment_number INT
)
BEGIN
  DECLARE v_next_number INT;

  -- Calculate next shipment number for this date
  SELECT COALESCE(MAX(shipment_number), 0) + 1
  INTO v_next_number
  FROM volvo_label_data
  WHERE shipment_date = p_shipment_date;

  -- Insert shipment
  INSERT INTO volvo_label_data (
    shipment_date, shipment_number, employee_number, notes, status
  ) VALUES (
    p_shipment_date, v_next_number, p_employee_number, p_notes, 'pending_po'
  );

  SET p_new_id = LAST_INSERT_ID();
  SET p_shipment_number = v_next_number;
END$$

DELIMITER ;
