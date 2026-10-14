-- Stored Procedure: sp_LotMarkings_Insert
-- Purpose:  Records a marking for a lot that has never been seen. A lot that already has
--           a row is refused, because an existing marking must never be replaced by a
--           freshly computed one.
-- Parameters:
--   p_heat_number            VARCHAR(100) - Heat number as captured.
--   p_heat_number_normalized VARCHAR(100) - Matching key.
--   p_supplier_id            VARCHAR(15)  - Supplier group, or 'NONPO'.
--   p_supplier_name          VARCHAR(255) - Supplier name as seen on the receipt.
--   p_shape_code             VARCHAR(32)  - Assigned shape code.
--   p_color_code             VARCHAR(32)  - Assigned color code.
--   p_marking_source         VARCHAR(16)  - Auto, Manual or Import.
--   p_first_seen_part_number VARCHAR(50)  - Part that first created the lot.
--   p_resolved_by            VARCHAR(50)  - Acting user, or null for an automatic assignment.
-- Returns:  new_id - the id of the inserted row.
-- Errors:   SQLSTATE 45000 when a marking already exists for this lot and supplier.

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_LotMarkings_Insert`;

DELIMITER $$
CREATE PROCEDURE `sp_LotMarkings_Insert`(
    IN p_heat_number VARCHAR(100),
    IN p_heat_number_normalized VARCHAR(100),
    IN p_supplier_id VARCHAR(15),
    IN p_supplier_name VARCHAR(255),
    IN p_shape_code VARCHAR(32),
    IN p_color_code VARCHAR(32),
    IN p_marking_source VARCHAR(16),
    IN p_first_seen_part_number VARCHAR(50),
    IN p_resolved_by VARCHAR(50)
)
BEGIN
    DECLARE v_exists INT DEFAULT 0;

    SELECT COUNT(*)
    INTO v_exists
    FROM `lot_markings`
    WHERE `heat_number_normalized` = p_heat_number_normalized
      AND `supplier_id` = p_supplier_id;

    IF v_exists > 0 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'This lot already has a marking. Use the override to change it.';
    END IF;

    INSERT INTO `lot_markings` (
        `heat_number`,
        `heat_number_normalized`,
        `supplier_id`,
        `supplier_name`,
        `shape_code`,
        `color_code`,
        `marking_source`,
        `is_locked`,
        `first_seen_part_number`,
        `resolved_by`,
        `resolved_at`
    )
    VALUES (
        p_heat_number,
        p_heat_number_normalized,
        p_supplier_id,
        p_supplier_name,
        p_shape_code,
        p_color_code,
        COALESCE(NULLIF(p_marking_source, ''), 'Auto'),
        0,
        p_first_seen_part_number,
        p_resolved_by,
        CURRENT_TIMESTAMP
    );

    SELECT LAST_INSERT_ID() AS new_id;
END $$
DELIMITER ;
