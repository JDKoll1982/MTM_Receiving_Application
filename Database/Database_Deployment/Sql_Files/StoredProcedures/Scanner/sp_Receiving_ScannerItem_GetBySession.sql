-- Stored Procedure: sp_Receiving_ScannerItem_GetBySession
-- Description: Returns ordered scanner items for a current-list session.

USE mtm_receiving_application_test;

DROP PROCEDURE IF EXISTS `sp_Receiving_ScannerItem_GetBySession`;

DELIMITER $$

CREATE PROCEDURE `sp_Receiving_ScannerItem_GetBySession`(
    IN p_session_id CHAR(36)
)
BEGIN
    SELECT
        id,
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
        failure_message,
        created_at,
        updated_at
    FROM receiving_scanner_item
    WHERE session_id = p_session_id
    ORDER BY item_order ASC;
END $$

DELIMITER ;
