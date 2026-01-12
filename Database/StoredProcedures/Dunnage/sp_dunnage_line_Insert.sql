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
    -- This SP is deprecated - table structure has changed
    -- Use dunnage_history table directly through appropriate service layer
    SET p_Status = 1;
    SET p_ErrorMsg = 'sp_Dunnage_Line_Insert is deprecated - use dunnage_history table';
END $$

DELIMITER ;
