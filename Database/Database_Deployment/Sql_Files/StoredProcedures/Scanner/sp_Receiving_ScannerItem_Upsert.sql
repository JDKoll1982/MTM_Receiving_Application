-- Stored Procedure: sp_Receiving_ScannerItem_Upsert
-- Description: Adds or updates one ordered scanner item for a session, including the
-- execution result fields so send results persist through the same procedure.

USE mtm_receiving_application_test;

DROP PROCEDURE IF EXISTS `sp_Receiving_ScannerItem_Upsert`;

DELIMITER $$

CREATE PROCEDURE `sp_Receiving_ScannerItem_Upsert`(
    IN p_session_id CHAR(36),
    IN p_item_order INT,
    IN p_part_id VARCHAR(50),
    IN p_from_warehouse_id VARCHAR(10),
    IN p_from_location_id VARCHAR(50),
    IN p_to_warehouse_id VARCHAR(10),
    IN p_to_location_id VARCHAR(50),
    IN p_quantity DECIMAL(18,2),
    IN p_payload_json JSON,
    IN p_validation_state VARCHAR(24),
    IN p_validation_notes VARCHAR(500),
    IN p_status VARCHAR(24),
    IN p_failure_code VARCHAR(120),
    IN p_failure_message VARCHAR(500)
)
BEGIN
    INSERT INTO receiving_scanner_item
    (
        session_id,
        item_order,
        part_id,
        from_warehouse_id,
        from_location_id,
        to_warehouse_id,
        to_location_id,
        quantity,
        payload_json,
        validation_state,
        validation_notes,
        status,
        failure_code,
        failure_message
    )
    VALUES
    (
        p_session_id,
        p_item_order,
        p_part_id,
        p_from_warehouse_id,
        p_from_location_id,
        p_to_warehouse_id,
        p_to_location_id,
        p_quantity,
        p_payload_json,
        p_validation_state,
        p_validation_notes,
        COALESCE(p_status, 'Waiting'),
        p_failure_code,
        p_failure_message
    )
    ON DUPLICATE KEY UPDATE
        part_id = VALUES(part_id),
        from_warehouse_id = VALUES(from_warehouse_id),
        from_location_id = VALUES(from_location_id),
        to_warehouse_id = VALUES(to_warehouse_id),
        to_location_id = VALUES(to_location_id),
        quantity = VALUES(quantity),
        payload_json = VALUES(payload_json),
        validation_state = VALUES(validation_state),
        validation_notes = VALUES(validation_notes),
        status = VALUES(status),
        failure_code = VALUES(failure_code),
        failure_message = VALUES(failure_message),
        updated_at = CURRENT_TIMESTAMP;
END $$

DELIMITER ;
