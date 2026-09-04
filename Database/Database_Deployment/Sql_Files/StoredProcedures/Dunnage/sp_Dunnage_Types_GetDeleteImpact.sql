-- ============================================================================
-- Stored Procedure: sp_Dunnage_Types_GetDeleteImpact
-- Description: Returns the number of parts, label-data queue rows, and archived
--   history rows that a dunnage-type delete would cascade-remove. Used to show
--   a delete-impact warning before deleting a type.
-- Parameters:
--   p_type_id: The dunnage type id (dunnage_types.id)
-- Returns: Single row (part_count, label_data_count, history_count)
-- Feature: Dunnage Module
-- MySQL Version: 5.7 compatible
-- ============================================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Types_GetDeleteImpact`$$

CREATE PROCEDURE `sp_Dunnage_Types_GetDeleteImpact`(
    IN p_type_id INT
)
BEGIN
    SELECT
        (SELECT COUNT(*) FROM dunnage_parts WHERE type_id = p_type_id) AS part_count,
        (SELECT COUNT(*) FROM dunnage_label_data
          WHERE dunnage_type_id = p_type_id
             OR part_id IN (SELECT part_id FROM dunnage_parts WHERE type_id = p_type_id)) AS label_data_count,
        (SELECT COUNT(*) FROM dunnage_history
          WHERE dunnage_type_id = p_type_id
             OR type_id = p_type_id
             OR part_id IN (SELECT part_id FROM dunnage_parts WHERE type_id = p_type_id)) AS history_count;
END $$

DELIMITER ;
