-- ============================================================================
-- Procedure: sp_Receiving_History_Get
-- Purpose: Retrieve receiving history records with flexible filtering
-- All parameters are optional (pass NULL to skip that filter)
-- ============================================================================

DELIMITER //

DROP PROCEDURE IF EXISTS `sp_Receiving_History_Get` //

CREATE PROCEDURE `sp_Receiving_History_Get`(
    IN p_PartID VARCHAR(50),
    IN p_StartDate DATE,
    IN p_EndDate DATE,
    IN p_PONumber VARCHAR(20),
    IN p_EmployeeNumber INT
)
BEGIN
    SELECT
        id,
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
        created_at
    FROM receiving_history
    WHERE
        (p_PartID IS NULL       OR part_id = p_PartID)
        AND (p_PONumber IS NULL     OR po_number = p_PONumber)
        AND (p_EmployeeNumber IS NULL OR employee_number = p_EmployeeNumber)
        AND (p_StartDate IS NULL    OR transaction_date >= p_StartDate)
        AND (p_EndDate IS NULL      OR transaction_date <= p_EndDate)
    ORDER BY transaction_date DESC, id DESC;
END //

DELIMITER ;
