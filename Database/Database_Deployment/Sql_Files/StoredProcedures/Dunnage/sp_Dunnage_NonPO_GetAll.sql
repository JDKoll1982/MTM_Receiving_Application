-- ============================================================
-- Procedure: sp_Dunnage_NonPO_GetAll
-- Purpose:   Returns all saved non-PO reference entries,
--            ordered by most-used first, then newest first.
-- ============================================================

DROP PROCEDURE IF EXISTS `sp_Dunnage_NonPO_GetAll`;

DELIMITER $$
CREATE PROCEDURE `sp_Dunnage_NonPO_GetAll`()
BEGIN
    SELECT
        `id`,
        `value`,
        `created_by`,
        `created_at`,
        `use_count`
    FROM   `dunnage_non_po_entries`
    ORDER  BY `use_count` DESC, `created_at` DESC;
END $$
DELIMITER ;
