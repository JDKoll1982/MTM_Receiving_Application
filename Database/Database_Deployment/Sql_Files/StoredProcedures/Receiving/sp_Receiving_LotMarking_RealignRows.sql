-- Stored Procedure: sp_Receiving_LotMarking_RealignRows
-- Purpose:  Puts one shape and color onto every receiving row that already carries a marking and that
--           matches one raw heat and supplier, across both the queue and history. This is how a lot
--           whose number was corrected is given the marking of the number it now carries.
-- Parameters:
--   p_heat             VARCHAR(100) - The heat value exactly as the report returned it.
--   p_supplier_name    VARCHAR(255) - The supplier value exactly as the report returned it.
--   p_heat_shape_code  VARCHAR(32)  - Shape emoji to write.
--   p_heat_color_code  VARCHAR(32)  - Color code to write.
-- Returns:  queue_rows_updated, history_rows_updated.
-- Errors:   SQLSTATE 45000 when either code is blank. A realignment never writes an empty marking.
--           The work is wrapped in a transaction so the two tables cannot end up disagreeing.
-- Notes:    Only rows that already carry a marking are touched. A row with no marking is left alone,
--           because putting markings right is this action's job and introducing new ones belongs to
--           the fill-in action. That predicate is also what stops this from ever marking a receipt
--           that was never marked in the first place.
--           The caller supplies the raw heat and supplier rather than a key, because the matching key
--           is produced by the application and the stored row holds what was typed.
--           Rows are matched on the supplier the way the preview reported it, so the same expression
--           is used on both sides.

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Receiving_LotMarking_RealignRows`;

DELIMITER $$
CREATE PROCEDURE `sp_Receiving_LotMarking_RealignRows`(
    IN p_heat VARCHAR(100),
    IN p_supplier_name VARCHAR(255),
    IN p_heat_shape_code VARCHAR(32),
    IN p_heat_color_code VARCHAR(32)
)
BEGIN
    DECLARE v_queue_rows INT DEFAULT 0;
    DECLARE v_history_rows INT DEFAULT 0;

    IF p_heat_shape_code IS NULL
       OR TRIM(p_heat_shape_code) = ''
       OR p_heat_color_code IS NULL
       OR TRIM(p_heat_color_code) = ''
    THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'A shape and a color are both required to put a marking right.';
    END IF;

    START TRANSACTION;

    UPDATE `receiving_label_data`
    SET
        `heat_shape_code` = p_heat_shape_code,
        `heat_color_code` = p_heat_color_code
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
        `heat_shape_code` = p_heat_shape_code,
        `heat_color_code` = p_heat_color_code
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
        v_queue_rows AS `queue_rows_updated`,
        v_history_rows AS `history_rows_updated`;
END $$
DELIMITER ;
