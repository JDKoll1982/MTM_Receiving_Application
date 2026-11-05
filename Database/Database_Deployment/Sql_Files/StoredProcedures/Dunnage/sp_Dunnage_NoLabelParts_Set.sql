-- ============================================================================
-- Stored Procedure: sp_Dunnage_NoLabelParts_Set
-- Description: Turns the "needs no printed label" flag on or off for one part.
--   Setting it on adds the row (or refreshes who changed it); setting it off
--   removes the row, so the part goes back to being queued in
--   dunnage_label_data like every other part.
-- Parameters:
--   p_part_id: The part number the flag belongs to
--   p_needs_no_label: 1 to add the flag, 0 to remove it
--   p_user: The user making the change
-- Notes:
--   A blank part id changes nothing: the caller reports the missing part number
--   itself, and the flag has no meaning without a part to hang it on.
-- Feature: Dunnage Module
-- MySQL Version: 5.7 compatible
-- ============================================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_NoLabelParts_Set` $$

CREATE PROCEDURE `sp_Dunnage_NoLabelParts_Set`(
    IN p_part_id VARCHAR(50),
    IN p_needs_no_label TINYINT(1),
    IN p_user VARCHAR(50)
)
BEGIN
    IF p_part_id IS NOT NULL AND TRIM(p_part_id) <> '' THEN
        IF p_needs_no_label = 1 THEN
            INSERT INTO dunnage_no_label_parts (
                part_id,
                created_by,
                created_date
            ) VALUES (
                TRIM(p_part_id),
                p_user,
                NOW()
            )
            ON DUPLICATE KEY UPDATE
                modified_by = p_user,
                modified_date = NOW();
        ELSE
            DELETE FROM dunnage_no_label_parts
            WHERE part_id = TRIM(p_part_id);
        END IF;
    END IF;
END $$

DELIMITER ;
