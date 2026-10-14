-- Stored Procedure: sp_Receiving_UnmarkedLotMarkings_Apply
-- Purpose:  Writes one lot's resolved marking onto every unmarked receiving row that belongs to that
--           lot, in both the queue and receiving history. This is the only way a marking is ever
--           written for a receipt that was taken before its part was enabled.
-- Parameters:
--   p_part_id          VARCHAR(50)  - Part whose rows are being filled in.
--   p_heat             VARCHAR(100) - The heat value exactly as the preview returned it.
--   p_supplier_name    VARCHAR(255) - The supplier value exactly as the preview returned it.
--   p_heat_shape_code  VARCHAR(32)  - Shape emoji to write.
--   p_heat_color_code  VARCHAR(32)  - Color code to write.
-- Returns:  queue_rows_updated, history_rows_updated.
-- Errors:   SQLSTATE 45000 when the part is not on the enabled list, when the heat value is blank,
--           or when either code is blank. A fill-in never invents a marking, so it refuses rather
--           than writing an empty one.
-- Notes:    Only rows with a null heat_shape_code are touched. A row that already carries a marking
--           keeps it, which is what stops this from ever changing a marking a handler has seen.
--           Writing the same lot twice is therefore a no-op the second time.
--           Every row matching the part, heat and supplier is written in one statement, so all rows
--           of a lot always end up with identical codes.

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Receiving_UnmarkedLotMarkings_Apply`;

DELIMITER $$
CREATE PROCEDURE `sp_Receiving_UnmarkedLotMarkings_Apply`(
    IN p_part_id VARCHAR(50),
    IN p_heat VARCHAR(100),
    IN p_supplier_name VARCHAR(255),
    IN p_heat_shape_code VARCHAR(32),
    IN p_heat_color_code VARCHAR(32)
)
BEGIN
    DECLARE v_is_enabled INT DEFAULT 0;
    DECLARE v_queue_rows INT DEFAULT 0;
    DECLARE v_history_rows INT DEFAULT 0;

    SELECT COUNT(*)
    INTO v_is_enabled
    FROM `lot_marking_part_scope`
    WHERE `part_id` = p_part_id
      AND `is_enabled` = 1;

    IF v_is_enabled = 0 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'That part number is not on the enabled list, so its lots are not marked.';
    END IF;

    IF p_heat IS NULL OR TRIM(p_heat) = '' THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'A row with no heat number is not a lot and cannot be marked.';
    END IF;

    IF p_heat_shape_code IS NULL
       OR TRIM(p_heat_shape_code) = ''
       OR p_heat_color_code IS NULL
       OR TRIM(p_heat_color_code) = ''
    THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'A shape and a color are both required to mark a lot.';
    END IF;

    UPDATE `receiving_label_data`
    SET
        `heat_shape_code` = p_heat_shape_code,
        `heat_color_code` = p_heat_color_code
    WHERE `part_id` = p_part_id
      AND COALESCE(`heat`, '') = p_heat
      AND COALESCE(
            NULLIF(TRIM(`po_vendor`), ''),
            NULLIF(TRIM(`vendor_name`), ''),
            ''
          ) = COALESCE(p_supplier_name, '')
      AND `heat_shape_code` IS NULL;

    SET v_queue_rows = ROW_COUNT();

    UPDATE `receiving_history`
    SET
        `heat_shape_code` = p_heat_shape_code,
        `heat_color_code` = p_heat_color_code
    WHERE `part_id` = p_part_id
      AND COALESCE(`heat`, '') = p_heat
      AND COALESCE(
            NULLIF(TRIM(`po_vendor`), ''),
            NULLIF(TRIM(`vendor_name`), ''),
            ''
          ) = COALESCE(p_supplier_name, '')
      AND `heat_shape_code` IS NULL;

    SET v_history_rows = ROW_COUNT();

    SELECT
        v_queue_rows AS `queue_rows_updated`,
        v_history_rows AS `history_rows_updated`;
END $$
DELIMITER ;
