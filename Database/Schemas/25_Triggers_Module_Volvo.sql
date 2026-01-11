-- ============================================================================
-- Triggers: Prevent duplicate pending shipments
-- Module: Volvo
-- Purpose: Ensure only one pending shipment exists at a time
-- Notes:
--   - Triggers ensure business rule: only one non-archived shipment may be in
--     'pending_po' status concurrently.
--   - Uses SIGNAL to abort offending INSERT/UPDATE with a clear message.
-- ============================================================================

-- Remove existing INSERT trigger if present
DROP TRIGGER IF EXISTS trigger_volvo_shipment_prevent_duplicate_pending;

-- Switch delimiter for trigger body
DELIMITER //

-- CREATE TRIGGER: BEFORE INSERT
-- Purpose: On insert, reject a new row that would create a second pending shipment.
CREATE TRIGGER trigger_volvo_shipment_prevent_duplicate_pending
BEFORE INSERT ON volvo_shipments
FOR EACH ROW
BEGIN
    -- Local variable to hold count of existing pending shipments
    DECLARE pending_count INT;

    -- Only enforce rule when the new row is intended to be a pending, non-archived shipment
    IF NEW.status = 'pending_po' AND NEW.is_archived = 0 THEN

        -- Count existing pending, non-archived shipments in the table
        -- (this counts the current table state before the INSERT is applied)
        SELECT COUNT(*) INTO pending_count
        FROM volvo_shipments
        WHERE status = 'pending_po'
          AND is_archived = 0;

        -- If any existing pending shipment exists, block the insert with a user error.
        -- SIGNAL SQLSTATE '45000' produces a generic user-defined error.
        IF pending_count > 0 THEN
            SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Only one pending shipment allowed at a time. Complete existing pending shipment first.';
        END IF;
    END IF;
END//

-- Restore standard delimiter
DELIMITER ;

-- Remove existing UPDATE trigger if present
DROP TRIGGER IF EXISTS trigger_volvo_shipment_prevent_duplicate_pending_update;

-- Switch delimiter for update trigger body
DELIMITER //

-- CREATE TRIGGER: BEFORE UPDATE
-- Purpose: Prevent changing an existing row TO pending_po (and non-archived) if
--          another pending, non-archived shipment exists (excluding the current row).
CREATE TRIGGER trigger_volvo_shipment_prevent_duplicate_pending_update
BEFORE UPDATE ON volvo_shipments
FOR EACH ROW
BEGIN
    -- Local variable to hold count of existing pending shipments (excluding current row)
    DECLARE pending_count INT;

    -- Only enforce when the updated state will be pending and non-archived,
    -- and the previous state was not already pending (or was archived).
    IF NEW.status = 'pending_po' AND NEW.is_archived = 0
       AND (OLD.status != 'pending_po' OR OLD.is_archived = 1) THEN

        -- Count existing pending, non-archived shipments excluding the row being updated
        SELECT COUNT(*) INTO pending_count
        FROM volvo_shipments
        WHERE status = 'pending_po'
          AND is_archived = 0
          AND id != NEW.id; -- Exclude current row to allow changing the current row itself

        -- If another pending shipment exists, block the update with a user error.
        IF pending_count > 0 THEN
            SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Only one pending shipment allowed at a time. Complete existing pending shipment first.';
        END IF;
    END IF;
END//

-- Restore standard delimiter
DELIMITER ;

-- ============================================================================
