-- Phase 1 Infrastructure: Stored Procedure
-- Procedure: sp_Receiving_Line_Insert
-- Purpose: Insert a new receiving line record with error handling

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Receiving_Line_Insert` $$

CREATE PROCEDURE `sp_Receiving_Line_Insert`(
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
    DECLARE v_old_foreign_key_checks INT DEFAULT @@FOREIGN_KEY_CHECKS;
    SET FOREIGN_KEY_CHECKS = 0;
    -- This SP is deprecated - use receiving_history and receiving_label_data tables
    SET p_Status = 1;
    SET p_ErrorMsg = 'sp_Receiving_Line_Insert is deprecated - use receiving_history table';
    SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
END $$

DELIMITER ;
