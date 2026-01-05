-- Stored Procedure: sp_volvo_part_check_references
-- Purpose: Check if a part has any active shipment line references before deactivation
-- Returns: Count of active references

DELIMITER //

DROP PROCEDURE IF EXISTS sp_volvo_part_check_references//

CREATE PROCEDURE sp_volvo_part_check_references(
    IN p_part_number VARCHAR(50),
    OUT p_active_reference_count INT
)
BEGIN
    -- Count shipment lines using this part where shipment is not completed/archived
    SELECT COUNT(*) INTO p_active_reference_count
    FROM volvo_shipment_lines vsl
    INNER JOIN volvo_shipments vs ON vsl.shipment_id = vs.id
    WHERE vsl.part_number = p_part_number
      AND vs.status NOT IN ('completed', 'archived')
      AND vs.is_archived = 0;
END//

DELIMITER ;
