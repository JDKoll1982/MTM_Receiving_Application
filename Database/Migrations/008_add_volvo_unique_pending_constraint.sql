-- Migration: Add unique constraint for pending shipments
-- Purpose: Prevent race condition - only one pending shipment at a time
-- Issue #8: Race Condition in Pending Shipment Check
-- MySQL 5.7 compatible version using triggers

-- Drop trigger if exists (for re-running migration)
DROP TRIGGER IF EXISTS trg_volvo_shipment_prevent_duplicate_pending;

DELIMITER //

CREATE TRIGGER trg_volvo_shipment_prevent_duplicate_pending
BEFORE INSERT ON volvo_shipments
FOR EACH ROW
BEGIN
    DECLARE pending_count INT;
    
    -- Only check if new shipment is pending and not archived
    IF NEW.status = 'pending_po' AND NEW.is_archived = 0 THEN
        -- Count existing pending, non-archived shipments
        SELECT COUNT(*) INTO pending_count
        FROM volvo_shipments
        WHERE status = 'pending_po' 
          AND is_archived = 0;
        
        -- If one already exists, prevent insert
        IF pending_count > 0 THEN
            SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Only one pending shipment allowed at a time. Complete existing pending shipment first.';
        END IF;
    END IF;
END//

DELIMITER ;

-- Also create trigger for UPDATE to prevent changing status to pending when one exists
DROP TRIGGER IF EXISTS trg_volvo_shipment_prevent_duplicate_pending_update;

DELIMITER //

CREATE TRIGGER trg_volvo_shipment_prevent_duplicate_pending_update
BEFORE UPDATE ON volvo_shipments
FOR EACH ROW
BEGIN
    DECLARE pending_count INT;
    
    -- Only check if updating TO pending status (and not archived)
    IF NEW.status = 'pending_po' AND NEW.is_archived = 0 
       AND (OLD.status != 'pending_po' OR OLD.is_archived = 1) THEN
        
        -- Count existing pending, non-archived shipments (excluding current row)
        SELECT COUNT(*) INTO pending_count
        FROM volvo_shipments
        WHERE status = 'pending_po' 
          AND is_archived = 0
          AND id != NEW.id;
        
        -- If one already exists, prevent update
        IF pending_count > 0 THEN
            SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Only one pending shipment allowed at a time. Complete existing pending shipment first.';
        END IF;
    END IF;
END//

DELIMITER ;
