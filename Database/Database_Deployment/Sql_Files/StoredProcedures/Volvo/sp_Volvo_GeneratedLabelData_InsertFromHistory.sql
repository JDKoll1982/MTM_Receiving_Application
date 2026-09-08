-- Stored Procedure: sp_Volvo_GeneratedLabelData_InsertFromHistory
-- Description: Copies a single row from volvo_generated_label_history back into
--              volvo_generated_label_data so it can be re-printed. Sets is_reprint = 1.
--              The duplicate guard keys on the history row's original_id (the original
--              active-row id) matching an existing is_reprint = 1 generated label row.

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Volvo_GeneratedLabelData_InsertFromHistory`;

DELIMITER $$

CREATE PROCEDURE `sp_Volvo_GeneratedLabelData_InsertFromHistory`(
    IN p_history_id      INT,
    IN p_queued_by       VARCHAR(100),
    IN p_employee_number INT
)
BEGIN
    DECLARE v_already_queued INT DEFAULT 0;

    -- Guard: block re-queuing if a reprint row for this history original_id already exists.
    SELECT COUNT(*)
    INTO v_already_queued
    FROM volvo_generated_label_data vgld
    INNER JOIN volvo_generated_label_history vgh ON vgh.original_id = vgld.id
    WHERE vgh.id = p_history_id
      AND vgld.is_reprint = 1;

    IF v_already_queued > 0 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'This history record is already queued for reprint.';
    ELSE
        INSERT INTO volvo_generated_label_data (
            shipment_id,
            shipment_number,
            shipment_date,
            part_number,
            quantity,
            skid_number,
            total_skids,
            part_description,
            employee_number,
            is_reprint
        )
        SELECT
            vgh.shipment_id,
            vgh.shipment_number,
            vgh.shipment_date,
            vgh.part_number,
            vgh.quantity,
            vgh.skid_number,
            vgh.total_skids,
            vgh.part_description,
            COALESCE(p_employee_number, vgh.employee_number),
            1
        FROM volvo_generated_label_history vgh
        WHERE vgh.id = p_history_id;

        SELECT ROW_COUNT() AS rows_inserted;
    END IF;
END $$
