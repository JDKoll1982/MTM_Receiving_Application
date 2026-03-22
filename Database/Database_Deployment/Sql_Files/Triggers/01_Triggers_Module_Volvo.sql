DROP TRIGGER IF EXISTS trigger_volvo_shipment_prevent_duplicate_pending;

DELIMITER //
CREATE TRIGGER trigger_volvo_shipment_prevent_duplicate_pending
BEFORE INSERT ON volvo_label_data
FOR EACH ROW
BEGIN
    DECLARE pending_count INT;

    IF NEW.status = 'pending_po'
        AND NEW.is_archived = 0 THEN
        SELECT
            COUNT(*) INTO pending_count
        FROM
            volvo_label_data
        WHERE
            status = 'pending_po'
            AND is_archived = 0;

        IF pending_count > 0 THEN
            SIGNAL SQLSTATE '45000'
                SET MESSAGE_TEXT = 'Only one pending shipment allowed at a time. Complete existing pending shipment first.';
        END IF;

    END IF;

END//
DELIMITER ;

DROP TRIGGER IF EXISTS trigger_volvo_shipment_prevent_duplicate_pending_update;

DELIMITER //
CREATE TRIGGER trigger_volvo_shipment_prevent_duplicate_pending_update
BEFORE UPDATE ON volvo_label_data
FOR EACH ROW
BEGIN
    DECLARE pending_count INT;

    IF NEW.status = 'pending_po'
        AND NEW.is_archived = 0
        AND (
            OLD.status != 'pending_po'
            OR OLD.is_archived = 1
        ) THEN
        SELECT
            COUNT(*) INTO pending_count
        FROM
            volvo_label_data
        WHERE
            status = 'pending_po'
            AND is_archived = 0
            AND id != NEW.id;

        IF pending_count > 0 THEN
            SIGNAL SQLSTATE '45000'
                SET MESSAGE_TEXT = 'Only one pending shipment allowed at a time. Complete existing pending shipment first.';
        END IF;

    END IF;

END//
DELIMITER ;
