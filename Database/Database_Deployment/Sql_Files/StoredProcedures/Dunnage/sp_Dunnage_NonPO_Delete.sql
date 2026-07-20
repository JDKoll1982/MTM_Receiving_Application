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
    SET FOREIGN_KEY_CHECKS = 0;
    DELETE FROM `dunnage_non_po_entries` WHERE `id` = p_id;
    SET FOREIGN_KEY_CHECKS = 1;
END $$
DELIMITER ;
