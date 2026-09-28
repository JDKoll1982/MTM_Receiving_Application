-- Stored Procedure: sp_Dunnage_LabelData_InsertFromHistory
-- Description: Copies a single row from dunnage_history back into dunnage_label_data
--              so it can be re-printed. Sets is_reprint = 1 so the row is visually
--              identified in the Reprint page and skipped when ClearToHistory runs.
--              Uses the history row's load_uuid (the dunnage_history primary key) so the
--              PRIMARY KEY on dunnage_label_data.load_uuid plus the duplicate guard below
--              prevent double-queuing.

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Dunnage_LabelData_InsertFromHistory`;

DELIMITER $$

CREATE PROCEDURE `sp_Dunnage_LabelData_InsertFromHistory`(
    IN p_load_uuid       CHAR(36),
    IN p_queued_by       VARCHAR(100),
    IN p_employee_number INT
)
BEGIN
    DECLARE v_already_queued INT DEFAULT 0;

    -- Guard: block re-queuing if a reprint row for this history load_uuid already exists.
    SELECT COUNT(*)
    INTO v_already_queued
    FROM dunnage_label_data dld
    INNER JOIN dunnage_history dh ON dh.load_uuid = dld.load_uuid
    WHERE dh.load_uuid = p_load_uuid
      AND dld.is_reprint = 1;

    IF v_already_queued > 0 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'This history record is already queued for reprint.';
    ELSE
        INSERT INTO dunnage_label_data (
            load_uuid,
            part_id,
            dunnage_type_id,
            dunnage_type_name,
            dunnage_type_icon,
            quantity,
            quantity_type,
            po_number,
            received_date,
            user_id,
            employee_number,
            location,
            label_number,
            part_skid_sequence,
            part_skid_total,
            udc1,
            udc2,
            udc3,
            udc4,
            udc5,
            udc6,
            udc7,
            udc8,
            udc9,
            udc10,
            is_reprint
        )
        SELECT
            dh.load_uuid,
            dh.part_id,
            COALESCE(dh.dunnage_type_id, dh.type_id),
            COALESCE(NULLIF(dh.dunnage_type_name, ''), dh.type_name),
            COALESCE(NULLIF(dh.dunnage_type_icon, ''), dh.type_icon),
            dh.quantity,
            COALESCE(NULLIF(dh.quantity_type, ''), 'Quantity'),
            dh.po_number,
            COALESCE(dh.received_date, dh.created_date, NOW()),
            COALESCE(p_queued_by, dh.user_id),
            COALESCE(p_employee_number, dh.employee_number),
            dh.location,
            dh.label_number,
            dh.part_skid_sequence,
            dh.part_skid_total,
            dh.udc1,
            dh.udc2,
            dh.udc3,
            dh.udc4,
            dh.udc5,
            dh.udc6,
            dh.udc7,
            dh.udc8,
            dh.udc9,
            dh.udc10,
            1
        FROM dunnage_history dh
        WHERE dh.load_uuid = p_load_uuid;

        SELECT ROW_COUNT() AS rows_inserted;
    END IF;
END $$
