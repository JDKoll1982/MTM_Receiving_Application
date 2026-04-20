-- Stored Procedure: sp_Volvo_LabelData_ClearToHistory
-- Description: Atomically moves all shipment headers currently in the active queue
--              from volvo_label_data to volvo_label_history, along with all associated
--              line items from volvo_line_data to volvo_line_history, then deletes the
--              moved rows from the active tables.
--              All records moved in a single call share the same archive_batch_id.
-- Lifecycle: Clear Label Data -> Active Queue to History + Active Queue Delete
-- Note: This clears the full active Volvo queue, matching the Receiving and Dunnage workflows.

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
    DECLARE v_headers_count   INT DEFAULT 0;
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

    -- Count active headers to be moved.
    SELECT COUNT(*) INTO v_headers_count
    FROM volvo_label_data
    ;

    IF v_headers_count = 0 THEN
        -- Nothing to archive — succeed silently.
        COMMIT;
    ELSE
        -- Count associated lines for all active shipments.
        SELECT COUNT(*) INTO v_lines_count
        FROM volvo_line_data vld
        WHERE vld.shipment_id IN (
            SELECT id FROM volvo_label_data
        );

        -- ----------------------------------------------------------------
        -- Step 1: Archive shipment headers into volvo_label_history.
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
        ;

        -- ----------------------------------------------------------------
        -- Step 2: Archive line items into volvo_line_history with the final
        --         shipment_history_id resolved from the archived headers.
        -- ----------------------------------------------------------------
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
            vld.id             AS original_id,
            vlh.id             AS shipment_history_id,
            vld.shipment_id    AS original_shipment_id,
            vld.part_number,
            COALESCE(NULLIF(TRIM(vld.po_status), ''), 'Pending'),
            vld.location,
            vld.quantity_per_skid,
            vld.received_skid_count,
            vld.calculated_piece_count,
            vld.has_discrepancy,
            vld.expected_skid_count,
            vld.discrepancy_note,
            NOW()              AS archived_at,
            p_archived_by      AS archived_by,
            p_archive_batch_id AS archive_batch_id
        FROM volvo_line_data vld
        INNER JOIN volvo_label_history vlh
            ON vlh.original_id = vld.shipment_id
           AND vlh.archive_batch_id = p_archive_batch_id
        CROSS JOIN (
            SELECT 'Pending' AS po_status
        ) VolvoLineStatus;

        -- ----------------------------------------------------------------
        -- Step 3: Delete moved line items from volvo_line_data.
        -- ----------------------------------------------------------------
        DELETE FROM volvo_line_data
        WHERE shipment_id IN (
            SELECT id FROM volvo_label_data
        );

        -- ----------------------------------------------------------------
        -- Step 4: Delete moved headers from volvo_label_data.
        -- ----------------------------------------------------------------
        DELETE FROM volvo_label_data
        ;

        SET p_headers_moved = v_headers_count;
        SET p_lines_moved   = v_lines_count;

        COMMIT;
    END IF;

END $$

DELIMITER ;
-- ============================================================================
