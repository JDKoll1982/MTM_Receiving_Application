-- ============================================================================
-- Stored Procedure: sp_Dunnage_NoLabelParts_GetAll
-- Description: Returns the part numbers that need no printed label. The list is
--   small and read once per save, so the whole set comes back rather than being
--   asked about one part at a time.
-- Feature: Dunnage Module
-- MySQL Version: 5.7 compatible
-- ============================================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_NoLabelParts_GetAll` $$

CREATE PROCEDURE `sp_Dunnage_NoLabelParts_GetAll`()
BEGIN
    SELECT
        part_id,
        created_by,
        created_date,
        modified_by,
        modified_date
    FROM dunnage_no_label_parts
    ORDER BY part_id;
END $$

DELIMITER ;
