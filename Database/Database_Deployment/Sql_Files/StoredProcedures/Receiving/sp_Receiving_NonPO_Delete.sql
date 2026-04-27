-- ============================================================
-- Procedure: sp_Receiving_NonPO_Delete
-- Purpose:   Deletes a saved Receiving non-PO entry by ID.
-- ============================================================

DROP PROCEDURE IF EXISTS `sp_Receiving_NonPO_Delete`;

DELIMITER $$
CREATE PROCEDURE `sp_Receiving_NonPO_Delete`(
    IN p_id INT UNSIGNED
)
BEGIN
    DELETE FROM `receiving_non_po_entries` WHERE `id` = p_id;
END $$
DELIMITER ;