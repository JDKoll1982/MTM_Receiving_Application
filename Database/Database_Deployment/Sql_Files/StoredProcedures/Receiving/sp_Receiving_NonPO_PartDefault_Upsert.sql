-- ============================================================
-- Procedure: sp_Receiving_NonPO_PartDefault_Upsert
-- Purpose:   Saves or replaces the per-part default non-PO
--            reference for a Receiving part.
-- ============================================================

DROP PROCEDURE IF EXISTS `sp_Receiving_NonPO_PartDefault_Upsert`;

DELIMITER $$
CREATE PROCEDURE `sp_Receiving_NonPO_PartDefault_Upsert`(
    IN p_part_id VARCHAR(100),
    IN p_value VARCHAR(100),
    IN p_updated_by VARCHAR(100)
)
BEGIN
    DECLARE v_old_foreign_key_checks INT DEFAULT @@FOREIGN_KEY_CHECKS;
    SET FOREIGN_KEY_CHECKS = 0;
    INSERT INTO `receiving_non_po_part_defaults` (`part_id`, `value`, `updated_by`)
    VALUES (p_part_id, p_value, p_updated_by)
    ON DUPLICATE KEY UPDATE
        `value` = VALUES(`value`),
        `updated_by` = VALUES(`updated_by`),
        `updated_at` = CURRENT_TIMESTAMP;
    SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
END $$
DELIMITER ;
