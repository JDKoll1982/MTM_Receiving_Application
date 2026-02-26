-- Stored Procedure: sp_Receiving_Load_Update
-- Description: Updates an existing receiving load record by GUID
-- Schema: Aligned with receiving_label_data column structure

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Receiving_Load_Update` $$

CREATE PROCEDURE `sp_Receiving_Load_Update`(
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
    UPDATE receiving_history
    SET
        quantity         = p_Quantity,
        part_id          = p_PartID,
        po_number        = p_PONumber,
        employee_number  = p_EmployeeNumber,
        heat             = p_Heat,
        transaction_date = p_TransactionDate,
        initial_location = p_InitialLocation,
        coils_on_skid    = p_CoilsOnSkid,
        label_number     = IFNULL(p_LabelNumber, 1),
        vendor_name      = p_VendorName,
        part_description = p_PartDescription
    WHERE load_guid = p_LoadGuid;
END$$

DELIMITER ;
