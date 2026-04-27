-- ============================================================
-- Procedure: sp_Receiving_NonPO_GetAll
-- Purpose:   Returns all saved non-PO reference entries for
--            Receiving, ordered by most-used first.
-- ============================================================

DROP PROCEDURE IF EXISTS `sp_Receiving_NonPO_GetAll`;

DELIMITER $$
CREATE PROCEDURE `sp_Receiving_NonPO_GetAll`()
BEGIN
    SELECT
        `id`,
        `value`,
        `created_by`,
        `created_at`,
        `use_count`
    FROM `receiving_non_po_entries`
    ORDER BY `use_count` DESC, `created_at` DESC;
END $$
DELIMITER ;