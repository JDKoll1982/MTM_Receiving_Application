-- Stored Procedure: sp_Receiving_ScannerRunItem_Insert
-- Description: Writes one per-item run outcome row.

USE mtm_receiving_application_test;

DROP PROCEDURE IF EXISTS `sp_Receiving_ScannerRunItem_Insert`;

DELIMITER $$

CREATE PROCEDURE `sp_Receiving_ScannerRunItem_Insert`(
    IN p_run_id CHAR(36),
    IN p_session_item_id BIGINT,
    IN p_item_order INT,
    IN p_part_id VARCHAR(50),
    IN p_from_warehouse_id VARCHAR(10),
    IN p_from_location_id VARCHAR(50),
    IN p_to_warehouse_id VARCHAR(10),
    IN p_to_location_id VARCHAR(50),
    IN p_quantity DECIMAL(18,2),
    IN p_result_status VARCHAR(24),
    IN p_attempt_count INT,
    IN p_failure_code VARCHAR(120),
    IN p_failure_message VARCHAR(500)
)
BEGIN
    INSERT INTO receiving_scanner_run_item
    (
        run_id,
        session_item_id,
        item_order,
        part_id,
        from_warehouse_id,
        from_location_id,
        to_warehouse_id,
        to_location_id,
        quantity,
        result_status,
        attempt_count,
        failure_code,
        failure_message,
        processed_at
    )
    VALUES
    (
        p_run_id,
        p_session_item_id,
        p_item_order,
        p_part_id,
        p_from_warehouse_id,
        p_from_location_id,
        p_to_warehouse_id,
        p_to_location_id,
        p_quantity,
        p_result_status,
        COALESCE(p_attempt_count, 1),
        p_failure_code,
        p_failure_message,
        NOW()
    );
END $$

DELIMITER ;