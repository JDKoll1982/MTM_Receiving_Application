-- Stored Procedure: sp_Volvo_LabelData_ClearToHistory
-- Description: Atomically moves all completed (status='completed') shipment headers
--              from volvo_label_data to volvo_label_history, along with all associated
--              line items from volvo_line_data to volvo_line_history, then deletes the
--              moved rows from the active tables.
--              All records moved in a single call share the same archive_batch_id.
-- Lifecycle: Clear Label Data -> Active Queue to History + Active Queue Delete
-- Note: Only rows with status='completed' are moved. Pending shipments are left untouched.

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Volvo_LabelData_ClearToHistory`;

DELIMITER $$

CREATE PROCEDURE `sp_Volvo_LabelData_ClearToHistory`(
    IN  p_archived_by       VARCHAR(100),
    OUT p_headers_moved     INT,
    OUT p_lines_moved       INT,
    OUT p_archive_batch_id  CHAR(36),
    OUT p_status            INT,
    OUT p_error_message     VARCHAR(1000)
)
BEGIN
    DECLARE v_completed_count INT DEFAULT 0;
    DECLARE v_lines_count     INT DEFAULT 0;

    -- Roll back and surface the error if anything inside the transaction fails.
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        SET p_headers_moved    = 0;
        SET p_lines_moved      = 0;
        SET p_status           = 1;
        SET p_error_message    = 'Clear Label Data failed. Transaction rolled back.';
    END;

    SET p_headers_moved    = 0;
    SET p_lines_moved      = 0;
    SET p_status           = 0;
    SET p_error_message    = NULL;
    SET p_archive_batch_id = UUID();

    START TRANSACTION;

    -- Count completed headers to be moved.
    SELECT COUNT(*) INTO v_completed_count
    FROM volvo_label_data
    WHERE status = 'completed';

    IF v_completed_count = 0 THEN
        -- Nothing to archive — succeed silently.
        COMMIT;
    ELSE
        -- Count associated lines for the completed shipments.
        SELECT COUNT(*) INTO v_lines_count
        FROM volvo_line_data vld
        WHERE vld.shipment_id IN (
            SELECT id FROM volvo_label_data WHERE status = 'completed'
        );

        -- ----------------------------------------------------------------
        -- Step 1: Archive line items into volvo_line_history.
        --         Must happen before archiving headers (FK insert-select).
        -- ----------------------------------------------------------------
        INSERT INTO volvo_line_history (
            original_id,
            shipment_history_id,
            original_shipment_id,
            part_number,
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
            vld.id                          AS original_id,
            -- shipment_history_id resolved via correlated sub-select after header insert;
            -- we insert headers first and use LAST_INSERT_ID range mapping via a temp approach.
            -- Simpler: insert in two passes — headers first, then lines joined by original_id.
            -- Re-order: headers first (Step 2), then lines (Step 3).
            0,                              -- placeholder; overwritten in Step 3
            vld.shipment_id                 AS original_shipment_id,
            vld.part_number,
            vld.location,
            vld.quantity_per_skid,
            vld.received_skid_count,
            vld.calculated_piece_count,
            vld.has_discrepancy,
            vld.expected_skid_count,
            vld.discrepancy_note,
            NOW()                           AS archived_at,
            p_archived_by                   AS archived_by,
            p_archive_batch_id              AS archive_batch_id
        FROM volvo_line_data vld
        WHERE vld.shipment_id IN (
            SELECT id FROM volvo_label_data WHERE status = 'completed'
        );
        -- Note: shipment_history_id is updated in Step 3 via a JOIN.

        -- ----------------------------------------------------------------
        -- Step 2: Archive shipment headers into volvo_label_history.
        -- ----------------------------------------------------------------
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
            vl.id               AS original_id,
            vl.shipment_date,
            vl.shipment_number,
            vl.po_number,
            vl.receiver_number,
            vl.employee_number,
            vl.notes,
            vl.status,
            vl.created_date,
            vl.modified_date,
            NOW()               AS archived_at,
            p_archived_by       AS archived_by,
            p_archive_batch_id  AS archive_batch_id
        FROM volvo_label_data vl
        WHERE vl.status = 'completed';

        -- ----------------------------------------------------------------
        -- Step 3: Back-fill shipment_history_id on the newly inserted lines
        --         using original_id → volvo_label_history.original_id join.
        -- ----------------------------------------------------------------
        UPDATE volvo_line_history vlh
        INNER JOIN volvo_label_history vlhdr
            ON  vlhdr.original_id      = vlh.original_shipment_id
            AND vlhdr.archive_batch_id = p_archive_batch_id
        SET vlh.shipment_history_id = vlhdr.id
        WHERE vlh.archive_batch_id = p_archive_batch_id
          AND vlh.shipment_history_id = 0;

        -- ----------------------------------------------------------------
        -- Step 4: Delete moved line items from volvo_line_data.
        -- ----------------------------------------------------------------
        DELETE FROM volvo_line_data
        WHERE shipment_id IN (
            SELECT id FROM volvo_label_data WHERE status = 'completed'
        );

        -- ----------------------------------------------------------------
        -- Step 5: Delete moved headers from volvo_label_data.
        -- ----------------------------------------------------------------
        DELETE FROM volvo_label_data
        WHERE status = 'completed';

        SET p_headers_moved = v_completed_count;
        SET p_lines_moved   = v_lines_count;

        COMMIT;
    END IF;

END$$

DELIMITER ;
-- ============================================================================
