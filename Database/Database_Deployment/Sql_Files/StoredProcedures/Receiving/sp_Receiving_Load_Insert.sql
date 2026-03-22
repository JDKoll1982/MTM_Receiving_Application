-- ============================================================================
-- Procedure: sp_Receiving_Load_Insert
-- Purpose: Insert a receiving transaction into receiving_history
-- Schema: Aligned with receiving_label_data column structure
-- ============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS `sp_Receiving_Load_Insert` //

CREATE PROCEDURE `sp_Receiving_Load_Insert`(
    IN p_LoadGuid CHAR(36),
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
    IN p_PartDescription VARCHAR(500)
)
BEGIN
    INSERT INTO receiving_history
    (
        load_guid,
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
        part_description
    )
    VALUES
    (
        p_LoadGuid,
        p_Quantity,
        p_PartID,
        p_PONumber,
        p_EmployeeNumber,
        p_Heat,
        p_TransactionDate,
        p_InitialLocation,
        p_CoilsOnSkid,
        IFNULL(p_LabelNumber, 1),
        p_VendorName,
        p_PartDescription
    );

    SELECT LAST_INSERT_ID() AS new_id;
END //

DELIMITER ;
