-- ============================================================================
-- Stored Procedure: sp_Dunnage_Types_Delete
-- Description: Soft delete a dunnage type by marking it inactive
-- Parameters:
--   @p_id: The ID of the dunnage type to delete
-- Feature: Dunnage Module
-- MySQL Version: 5.7 compatible
-- ============================================================================

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Dunnage_Types_Delete`;

DELIMITER $$

CREATE PROCEDURE `sp_Dunnage_Types_Delete`(
    IN p_id INT,
    IN p_modified_by VARCHAR(50),
    OUT p_status INT,
    OUT p_error_msg VARCHAR(500)
)
BEGIN
    DECLARE v_exists INT DEFAULT 0;
    DECLARE v_parts_count INT DEFAULT 0;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1 p_error_msg = MESSAGE_TEXT;
        SET p_status = -1;
        ROLLBACK;
    END;

    START TRANSACTION;

    -- Check if dunnage type exists
    SELECT COUNT(*) INTO v_exists
    FROM dunnage_types
    WHERE id = p_id;

    IF v_exists = 0 THEN
        SET p_status = 0;
        SET p_error_msg = 'Dunnage type not found';
        ROLLBACK;
    ELSE
        -- Check if dunnage type is in use by any parts
        SELECT COUNT(*) INTO v_parts_count
        FROM dunnage_parts
        WHERE dunnage_type_id = p_id;

        IF v_parts_count > 0 THEN
            SET p_status = -1;
            SET p_error_msg = CONCAT('Cannot delete dunnage type that is in use by ', v_parts_count, ' part(s)');
            ROLLBACK;
        ELSE
            -- Delete the dunnage type (hard delete since no soft delete columns in schema)
            DELETE FROM dunnage_types
            WHERE id = p_id;

            SET p_status = 1;
            SET p_error_msg = 'Dunnage type deleted successfully';
            COMMIT;
        END IF;
    END IF;
END $$

DELIMITER ;
