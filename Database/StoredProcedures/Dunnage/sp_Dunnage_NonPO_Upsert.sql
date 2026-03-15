-- ============================================================
-- Procedure: sp_Dunnage_NonPO_Upsert
-- Purpose:   Inserts a new non-PO reference entry, or increments
--            use_count when the same value already exists.
-- ============================================================

DROP PROCEDURE IF EXISTS `sp_Dunnage_NonPO_Upsert`;

DELIMITER $$
CREATE PROCEDURE `sp_Dunnage_NonPO_Upsert`(
    IN  p_value       VARCHAR(100),
    IN  p_created_by  VARCHAR(100)
)
BEGIN
    INSERT INTO `dunnage_non_po_entries` (`value`, `created_by`, `use_count`)
    VALUES (p_value, p_created_by, 1)
    ON DUPLICATE KEY UPDATE `use_count` = `use_count` + 1;
END$$
DELIMITER ;
