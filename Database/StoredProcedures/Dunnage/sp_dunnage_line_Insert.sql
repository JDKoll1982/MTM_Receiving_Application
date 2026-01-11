-- Phase 1 Infrastructure: Stored Procedure
-- Procedure: sp_Dunnage_Line_Insert
-- Purpose: Insert a new dunnage line record with error handling

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Line_Insert` $$

CREATE PROCEDURE `sp_Dunnage_Line_Insert`(
    IN p_Line1 VARCHAR(255),
    IN p_Line2 VARCHAR(255),
    IN p_PONumber INT,
    IN p_Date DATE,
    IN p_EmployeeNumber INT,
    IN p_VendorName VARCHAR(255),
    IN p_Location VARCHAR(50),
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
    IF p_Line1 IS NULL OR p_Line1 = '' THEN
        SET p_Status = 1;
        SET p_ErrorMsg = 'Line1 is required';
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
        -- Insert the dunnage line record
        INSERT INTO label_table_dunnage (
            line1,
            line2,
            po_number,
            transaction_date,
            employee_number,
            vendor_name,
            location,
            label_number
        ) VALUES (
            p_Line1,
            p_Line2,
            p_PONumber,
            p_Date,
            p_EmployeeNumber,
            p_VendorName,
            p_Location,
            1  -- Default label_number
        );

        -- Success
        SET p_Status = 0;
        SET p_ErrorMsg = '';
        COMMIT;
    END IF;
END $$

DELIMITER ;
