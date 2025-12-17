-- Phase 1 Infrastructure: Stored Procedure
-- Procedure: receiving_line_Insert
-- Purpose: Insert a new receiving line record with error handling

DELIMITER $$

DROP PROCEDURE IF EXISTS receiving_line_Insert $$

CREATE PROCEDURE receiving_line_Insert(
    IN p_Quantity INT,
    IN p_PartID VARCHAR(50),
    IN p_PONumber INT,
    IN p_EmployeeNumber INT,
    IN p_Heat VARCHAR(100),
    IN p_Date DATE,
    IN p_InitialLocation VARCHAR(50),
    IN p_CoilsOnSkid INT,
    IN p_VendorName VARCHAR(255),
    IN p_PartDescription VARCHAR(500),
    OUT p_Status INT,
    OUT p_ErrorMsg VARCHAR(500)
)
BEGIN
    -- Error handler for SQL exceptions
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1 p_ErrorMsg = MESSAGE_TEXT;
        SET p_Status = 1;
        ROLLBACK;
    END;

    -- Start transaction
    START TRANSACTION;

    -- Validation: Check required fields
    IF p_Quantity <= 0 THEN
        SET p_Status = 1;
        SET p_ErrorMsg = 'Quantity must be greater than 0';
        ROLLBACK;
    ELSEIF p_PartID IS NULL OR p_PartID = '' THEN
        SET p_Status = 1;
        SET p_ErrorMsg = 'Part ID is required';
        ROLLBACK;
    ELSEIF p_PONumber <= 0 THEN
        SET p_Status = 1;
        SET p_ErrorMsg = 'PO Number must be greater than 0';
        ROLLBACK;
    ELSEIF p_EmployeeNumber <= 0 THEN
        SET p_Status = 1;
        SET p_ErrorMsg = 'Employee Number must be greater than 0';
        ROLLBACK;
    ELSE
        -- Insert the receiving line record
        INSERT INTO label_table_receiving (
            quantity,
            part_id,
            po_number,
            employee_number,
            heat,
            transaction_date,
            initial_location,
            coils_on_skid,
            vendor_name,
            part_description,
            label_number
        ) VALUES (
            p_Quantity,
            p_PartID,
            p_PONumber,
            p_EmployeeNumber,
            p_Heat,
            p_Date,
            p_InitialLocation,
            p_CoilsOnSkid,
            p_VendorName,
            p_PartDescription,
            1  -- Default label_number
        );

        -- Success
        SET p_Status = 0;
        SET p_ErrorMsg = '';
        COMMIT;
    END IF;
END $$

DELIMITER ;
