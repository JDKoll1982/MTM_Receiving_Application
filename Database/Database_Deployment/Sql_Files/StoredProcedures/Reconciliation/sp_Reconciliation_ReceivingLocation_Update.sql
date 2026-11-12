-- =============================================================================
-- Procedure : sp_Reconciliation_ReceivingLocation_Update
-- Module    : Reconciliation (Receiving Validator)
-- Purpose   : The single reconciliation-owned write procedure. Corrects the
--             saved location on one receiving row, from whichever table the row
--             lives in. This procedure is the whole data surface between the
--             validator and Receiving (FR-005, FR-055, decision D-11).
-- Parameters:
--   p_row_source           VARCHAR(16)  in  'CurrentLabel' or 'History'
--   p_record_id            INT          in  the row's id in the named table
--   p_destination_location VARCHAR(50)  in  the location to write
--   p_operator_user_id     VARCHAR(100) in  who accepted the line
--   p_affected_rows        INT          out 1 on a successful update, 0 when
--                                            the row was absent
-- Notes     : Updates initial_location and nothing else. It does not touch
--             quantity, part, purchase order or any audit column, because this
--             slice adds no audit trail (FR-055).
--             No ownership predicate: the procedure corrects a row regardless of
--             the employee or Windows user who created it (FR-052, DS-06).
--             R-SQL-03 asks a modifying procedure for SET NOCOUNT ON, which is
--             SQL Server syntax this MySQL server does not accept. The intent is
--             met instead: the procedure emits no row-count result set and
--             reports the count through p_affected_rows only.
--             The role gate FR-054 leaves open would be enforced here, before the
--             UPDATE. Decision D-09 sets the working default of any authenticated
--             user, so no gate is applied yet.
-- =============================================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Reconciliation_ReceivingLocation_Update` $$

CREATE PROCEDURE `sp_Reconciliation_ReceivingLocation_Update`(
    IN p_row_source VARCHAR(16),
    IN p_record_id INT,
    IN p_destination_location VARCHAR(50),
    IN p_operator_user_id VARCHAR(100),
    OUT p_affected_rows INT
)
BEGIN
    DECLARE v_normalized_source VARCHAR(16);

    SET p_affected_rows = 0;

    START TRANSACTION;

    IF p_destination_location IS NULL OR TRIM(p_destination_location) = '' THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'A destination location is required.';
    END IF;

    IF p_operator_user_id IS NULL OR TRIM(p_operator_user_id) = '' THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'An operator user id is required.';
    END IF;

    IF p_record_id IS NULL OR p_record_id <= 0 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'A positive record id is required.';
    END IF;

    SET v_normalized_source = TRIM(p_row_source);

    IF v_normalized_source = 'CurrentLabel' THEN
        UPDATE receiving_label_data
        SET initial_location = TRIM(p_destination_location)
        WHERE id = p_record_id;
    ELSEIF v_normalized_source = 'History' THEN
        UPDATE receiving_history
        SET initial_location = TRIM(p_destination_location)
        WHERE id = p_record_id;
    ELSE
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'The row source must be CurrentLabel or History.';
    END IF;

    SET p_affected_rows = ROW_COUNT();

    COMMIT;
END $$

DELIMITER ;

-- TEST BLOCK START: Sample update
-- CALL sp_Reconciliation_ReceivingLocation_Update('CurrentLabel', 1, 'V-C0-01', 'test.user', @rows);
-- SELECT @rows;
-- TEST BLOCK END: Sample update
