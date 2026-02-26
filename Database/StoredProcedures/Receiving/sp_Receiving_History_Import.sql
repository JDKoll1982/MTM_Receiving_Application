-- ============================================================================
-- Procedure: sp_Receiving_History_Import
-- Purpose: Import a single receiving history record from an external source
--          (e.g., Google Sheets CSV). Designed for bulk historical import.
-- Notes:
--   - po_number stored as-is (e.g., 'PO-066914') - no stripping
--   - heat 'NONE' is preserved as-is; callers may convert to NULL if desired
--   - Returns the new auto-increment id via SELECT
-- ============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS `sp_Receiving_History_Import` //

CREATE PROCEDURE `sp_Receiving_History_Import`(
    IN p_Quantity INT,
    IN p_PartID VARCHAR(50),
    IN p_PONumber VARCHAR(20),
    IN p_EmployeeNumber INT,
    IN p_Heat VARCHAR(100),
    IN p_TransactionDate DATE,
    IN p_InitialLocation VARCHAR(50),
    IN p_CoilsOnSkid INT,
    IN p_LabelNumber INT,
    IN p_VendorName VARCHAR(255),
    IN p_PartDescription VARCHAR(500),
    IN p_IsNonPOItem TINYINT(1)
)
BEGIN
    INSERT INTO receiving_history
    (
        quantity,
        part_id,
        po_number,
        employee_number,
        heat,
        transaction_date,
        initial_location,
        coils_on_skid,
        label_number,
        vendor_name,
        part_description,
        is_non_po_item
    )
    VALUES
    (
        p_Quantity,
        p_PartID,
        NULLIF(TRIM(p_PONumber), ''),
        p_EmployeeNumber,
        NULLIF(TRIM(p_Heat), ''),
        p_TransactionDate,
        NULLIF(TRIM(p_InitialLocation), ''),
        p_CoilsOnSkid,
        IFNULL(p_LabelNumber, 1),
        NULLIF(TRIM(p_VendorName), ''),
        NULLIF(TRIM(p_PartDescription), ''),
        IFNULL(p_IsNonPOItem, 0)
    );

    -- Assign to a session variable so no result-set is returned (avoids
    -- flooding stdout during bulk imports via mysql.exe batch mode).
    SELECT LAST_INSERT_ID() INTO @last_import_id;
END //

DELIMITER ;
