-- ============================================================
-- Procedure: sp_Dunnage_NonPO_Delete
-- Purpose:   Deletes a saved non-PO reference entry by ID.
-- ============================================================

DROP PROCEDURE IF EXISTS `sp_Dunnage_NonPO_Delete`;

DELIMITER $$
CREATE PROCEDURE `sp_Dunnage_NonPO_Delete`(
    IN p_id INT UNSIGNED
)
BEGIN
    DELETE FROM `dunnage_non_po_entries` WHERE `id` = p_id;
END $$
DELIMITER ;
