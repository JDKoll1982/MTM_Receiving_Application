-- ============================================================================
-- Stored Procedure: sp_Dunnage_Parts_GetDeleteImpact
-- Description: Returns the number of label-data queue rows, archived history
--   rows, and inventory-tracking rows that reference a dunnage part. Used to
--   show a delete-impact warning before cascading a part delete.
-- Parameters:
--   p_part_id: The part identifier (dunnage_parts.part_id)
-- Returns: Single row (label_data_count, history_count, inventory_count)
-- Feature: Dunnage Module
-- MySQL Version: 5.7 compatible
-- ============================================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Parts_GetDeleteImpact`$$

CREATE PROCEDURE `sp_Dunnage_Parts_GetDeleteImpact`(
    IN p_part_id VARCHAR(50)
)
BEGIN
    SELECT
        (SELECT COUNT(*) FROM dunnage_label_data WHERE part_id = p_part_id) AS label_data_count,
        (SELECT COUNT(*) FROM dunnage_history WHERE part_id = p_part_id) AS history_count,
        (SELECT COUNT(*) FROM dunnage_requires_inventory WHERE part_id = p_part_id) AS inventory_count;
END $$

DELIMITER ;
