-- =====================================================
-- Stored Procedure: sp_volvo_shipment_complete
-- =====================================================
-- Purpose: Complete a shipment and move its active rows to history
-- Database: mtm_receiving_application
-- =====================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Volvo_Shipment_Complete` $$

CREATE PROCEDURE `sp_Volvo_Shipment_Complete`(
  IN p_shipment_id INT,
  IN p_po_number VARCHAR(50),
  IN p_receiver_number VARCHAR(50),
  IN p_archived_by VARCHAR(100)
)
BEGIN
  DECLARE v_history_id INT;
  DECLARE v_archive_batch_id CHAR(36);

  DECLARE v_old_foreign_key_checks INT DEFAULT @@FOREIGN_KEY_CHECKS;
  DECLARE EXIT HANDLER FOR SQLEXCEPTION
  BEGIN
        SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
    ROLLBACK;
    RESIGNAL;
  END;

  SET FOREIGN_KEY_CHECKS = 0;

  SET v_archive_batch_id = UUID();

  START TRANSACTION;

  INSERT INTO volvo_label_history (
    original_id,
    shipment_date,
    shipment_number,
    po_number,
    receiver_number,
    employee_number,
    notes,
    status,
    created_date,
    modified_date,
    archived_at,
    archived_by,
    archive_batch_id
  )
  SELECT
    vld.id,
    vld.shipment_date,
    vld.shipment_number,
    NULLIF(TRIM(p_po_number), ''),
    NULLIF(TRIM(p_receiver_number), ''),
    vld.employee_number,
    vld.notes,
    'completed',
    vld.created_date,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP,
    NULLIF(TRIM(p_archived_by), ''),
    v_archive_batch_id
  FROM volvo_label_data vld
  WHERE vld.id = p_shipment_id;

  SET v_history_id = LAST_INSERT_ID();

  INSERT INTO volvo_line_history (
    original_id,
    shipment_history_id,
    original_shipment_id,
    part_number,
    po_status,
    location,
    quantity_per_skid,
    received_skid_count,
    calculated_piece_count,
    has_discrepancy,
    expected_skid_count,
    discrepancy_note,
    archived_at,
    archived_by,
    archive_batch_id
  )
  SELECT
    vln.id,
    v_history_id,
    vln.shipment_id,
    vln.part_number,
    'Received',
    vln.location,
    vln.quantity_per_skid,
    vln.received_skid_count,
    vln.calculated_piece_count,
    vln.has_discrepancy,
    vln.expected_skid_count,
    vln.discrepancy_note,
    CURRENT_TIMESTAMP,
    NULLIF(TRIM(p_archived_by), ''),
    v_archive_batch_id
  FROM volvo_line_data vln
  WHERE vln.shipment_id = p_shipment_id;

  DELETE FROM volvo_line_data
  WHERE shipment_id = p_shipment_id;

  DELETE FROM volvo_label_data
  WHERE id = p_shipment_id;

  COMMIT;
    SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
END $$

DELIMITER ;