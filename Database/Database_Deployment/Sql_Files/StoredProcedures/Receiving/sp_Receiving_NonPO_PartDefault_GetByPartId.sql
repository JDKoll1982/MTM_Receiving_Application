-- ============================================================
-- Procedure: sp_Receiving_NonPO_PartDefault_GetByPartId
-- Purpose:   Returns the saved per-part default non-PO reference
--            for the supplied Receiving part ID.
-- ============================================================

DROP PROCEDURE IF EXISTS `sp_Receiving_NonPO_PartDefault_GetByPartId`;

DELIMITER $$
CREATE PROCEDURE `sp_Receiving_NonPO_PartDefault_GetByPartId`(
    IN p_part_id VARCHAR(100)
)
BEGIN
    SELECT `value`
    FROM `receiving_non_po_part_defaults`
    WHERE `part_id` = p_part_id
    LIMIT 1;
END $$
DELIMITER ;