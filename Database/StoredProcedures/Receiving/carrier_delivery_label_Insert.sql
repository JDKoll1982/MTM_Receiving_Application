-- Phase 1 Infrastructure: Stored Procedure
-- Procedure: carrier_delivery_label_Insert
-- Purpose: Insert a new carrier delivery label record (UPS/FedEx/USPS shipping info) with error handling

DELIMITER $$

DROP PROCEDURE IF EXISTS carrier_delivery_label_Insert $$

CREATE PROCEDURE carrier_delivery_label_Insert(
    IN p_DeliverTo VARCHAR(255),
    IN p_Department VARCHAR(100),
    IN p_PackageDescription VARCHAR(500),
    IN p_PONumber INT,
    IN p_WorkOrderNumber VARCHAR(50),
    IN p_EmployeeNumber INT,
    IN p_Date DATE,
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
    IF p_DeliverTo IS NULL OR p_DeliverTo = '' THEN
        SET p_Status = 1;
        SET p_ErrorMsg = 'DeliverTo is required';
        ROLLBACK;
    ELSEIF p_EmployeeNumber <= 0 THEN
        SET p_Status = 1;
        SET p_ErrorMsg = 'Employee Number must be greater than 0';
        ROLLBACK;
    ELSE
        -- Insert the carrier delivery label record
        INSERT INTO carrier_delivery_lines (
            deliver_to,
            department,
            package_description,
            po_number,
            work_order_number,
            employee_number,
            label_number,
            transaction_date
        ) VALUES (
            p_DeliverTo,
            p_Department,
            p_PackageDescription,
            p_PONumber,
            p_WorkOrderNumber,
            p_EmployeeNumber,
            1,  -- Default label_number
            p_Date
        );

        -- Success
        SET p_Status = 0;
        SET p_ErrorMsg = '';
        COMMIT;
    END IF;
END $$

DELIMITER ;
