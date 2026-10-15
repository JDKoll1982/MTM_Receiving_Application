-- Stored Procedure: sp_Receiving_LotMarking_ClearRows
-- Purpose:  Removes the marking from every receiving row that carries one and matches one raw heat
--           and supplier, across both the queue and history. This is how a lot number that was
--           corrected to nothing is dealt with: a line with no lot number is not a lot and must not
--           carry a lot's marking.
-- Parameters:
--   p_heat             VARCHAR(100) - The heat value exactly as the report returned it. Empty when the
--                                     number was cleared, which is the ordinary case here.
--   p_supplier_name    VARCHAR(255) - The supplier value exactly as the report returned it.
-- Returns:  queue_rows_cleared, history_rows_cleared.
-- Errors:   none. Clearing a marking that is already absent is a normal answer, not a failure.
-- Notes:    Only rows that carry a marking are touched, so the row count reported is the number of
--           markings actually removed.
--           The supplier is matched with the same expression the preview used, so a supplier that the
--           receipt left empty matches the empty value the preview reported.
--           The work is wrapped in a transaction so the two tables cannot end up disagreeing.

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Receiving_LotMarking_ClearRows`;

DELIMITER $$
CREATE PROCEDURE `sp_Receiving_LotMarking_ClearRows`(
    IN p_heat VARCHAR(100),
    IN p_supplier_name VARCHAR(255)
)
BEGIN
    DECLARE v_queue_rows INT DEFAULT 0;
    DECLARE v_history_rows INT DEFAULT 0;

    START TRANSACTION;

    UPDATE `receiving_label_data`
    SET
        `heat_shape_code` = NULL,
        `heat_color_code` = NULL
    WHERE COALESCE(`heat`, '') = COALESCE(p_heat, '')
      AND COALESCE(
            NULLIF(TRIM(`po_vendor`), ''),
            NULLIF(TRIM(`vendor_name`), ''),
            ''
          ) = COALESCE(p_supplier_name, '')
      AND `heat_shape_code` IS NOT NULL;

    SET v_queue_rows = ROW_COUNT();

    UPDATE `receiving_history`
    SET
        `heat_shape_code` = NULL,
        `heat_color_code` = NULL
    WHERE COALESCE(`heat`, '') = COALESCE(p_heat, '')
      AND COALESCE(
            NULLIF(TRIM(`po_vendor`), ''),
            NULLIF(TRIM(`vendor_name`), ''),
            ''
          ) = COALESCE(p_supplier_name, '')
      AND `heat_shape_code` IS NOT NULL;

    SET v_history_rows = ROW_COUNT();

    COMMIT;

    SELECT
        v_queue_rows AS `queue_rows_cleared`,
        v_history_rows AS `history_rows_cleared`;
END $$
DELIMITER ;
