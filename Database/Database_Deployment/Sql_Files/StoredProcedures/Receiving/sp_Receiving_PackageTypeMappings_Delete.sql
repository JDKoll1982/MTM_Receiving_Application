-- =============================================
-- Stored Procedure: sp_Receiving_PackageTypeMappings_Delete
-- Purpose: Soft-delete a mapping. Prevent deleting the default mapping.
-- =============================================

DELIMITER $$

DROP PROCEDURE IF EXISTS sp_Receiving_PackageTypeMappings_Delete$$
CREATE PROCEDURE sp_Receiving_PackageTypeMappings_Delete(
    IN p_id INT
)
proc_body: BEGIN
    DECLARE v_exists INT DEFAULT 0;
    DECLARE v_is_default TINYINT DEFAULT 0;

    SELECT COUNT(*) INTO v_exists
    FROM receiving_package_type_mapping
    WHERE id = p_id;

    IF v_exists = 0 THEN
        SELECT 0 AS affected_rows, 'NotFound' AS status, 'Mapping not found' AS message;
        LEAVE proc_body;
    END IF;

    SELECT IF(is_default,1,0) INTO v_is_default
    FROM receiving_package_type_mapping
    WHERE id = p_id
    LIMIT 1;

    IF v_is_default = 1 THEN
        SELECT 0 AS affected_rows, 'Forbidden' AS status, 'Cannot delete default mapping' AS message;
        LEAVE proc_body;
    END IF;

    UPDATE receiving_package_type_mapping
    SET is_active = FALSE,
        updated_at = CURRENT_TIMESTAMP
    WHERE id = p_id;

    SELECT ROW_COUNT() AS affected_rows, 'OK' AS status, NULL AS message;
END proc_body$$

DELIMITER ;
