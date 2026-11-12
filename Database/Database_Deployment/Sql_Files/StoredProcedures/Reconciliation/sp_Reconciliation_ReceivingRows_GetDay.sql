-- =============================================================================
-- Procedure : sp_Reconciliation_ReceivingRows_GetDay
-- Module    : Reconciliation (Receiving Validator)
-- Purpose   : Returns every receiving row for one calendar day, from both row
--             sources, so the validator can build one day's findings.
-- Parameters:
--   p_transaction_date DATE  in   the calendar day to load
-- Returns   : one row set
--             row_source VARCHAR(16)  'CurrentLabel' or 'History'
--             record_id INT           the row's own id in its table
--             load_id CHAR(36)        nullable
--             load_number INT         nullable, the allocation tie-break
--             part_id, part_description, po_number, po_line_number
--             transaction_date DATE, received_date DATETIME
--             quantity INT            the quantity proof's field
--             weight_quantity DECIMAL nullable, display only
--             unit_of_measure, initial_location, vendor_name
--             is_non_po_item TINYINT(1), is_reprint TINYINT(1)
-- Notes     : Reprint rows are returned rather than filtered out so the caller
--             can exclude and count them in one place (FR-016).
--             Explicit column lists only, and the filter sits on the indexed
--             transaction_date column with no function applied to it (R-SQL-02).
-- =============================================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Reconciliation_ReceivingRows_GetDay` $$

CREATE PROCEDURE `sp_Reconciliation_ReceivingRows_GetDay`(
    IN p_transaction_date DATE
)
BEGIN
    SELECT
        'CurrentLabel' AS row_source,
        ld.id AS record_id,
        ld.load_id AS load_id,
        ld.load_number AS load_number,
        ld.part_id AS part_id,
        COALESCE(ld.part_description, '') AS part_description,
        COALESCE(ld.po_number, '') AS po_number,
        COALESCE(ld.po_line_number, '') AS po_line_number,
        ld.transaction_date AS transaction_date,
        ld.received_date AS received_date,
        ld.quantity AS quantity,
        ld.weight_quantity AS weight_quantity,
        COALESCE(ld.unit_of_measure, '') AS unit_of_measure,
        COALESCE(ld.initial_location, '') AS initial_location,
        COALESCE(NULLIF(ld.vendor_name, ''), ld.po_vendor, '') AS vendor_name,
        ld.is_non_po_item AS is_non_po_item,
        ld.is_reprint AS is_reprint
    FROM receiving_label_data ld
    WHERE ld.transaction_date = p_transaction_date

    UNION ALL

    SELECT
        'History' AS row_source,
        h.id AS record_id,
        h.load_id AS load_id,
        h.load_number AS load_number,
        h.part_id AS part_id,
        COALESCE(h.part_description, '') AS part_description,
        COALESCE(h.po_number, '') AS po_number,
        COALESCE(h.po_line_number, '') AS po_line_number,
        h.transaction_date AS transaction_date,
        h.received_date AS received_date,
        h.quantity AS quantity,
        h.weight_quantity AS weight_quantity,
        COALESCE(h.unit_of_measure, '') AS unit_of_measure,
        COALESCE(h.initial_location, '') AS initial_location,
        COALESCE(NULLIF(h.vendor_name, ''), h.po_vendor, '') AS vendor_name,
        h.is_non_po_item AS is_non_po_item,
        h.is_reprint AS is_reprint
    FROM receiving_history h
    WHERE h.transaction_date = p_transaction_date

    ORDER BY
        part_id ASC,
        transaction_date ASC,
        load_number ASC,
        load_id ASC,
        record_id ASC;
END $$

DELIMITER ;

-- TEST BLOCK START: Sample day
-- CALL sp_Reconciliation_ReceivingRows_GetDay('2026-11-12');
-- TEST BLOCK END: Sample day
