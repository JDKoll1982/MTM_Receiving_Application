-- Stored Procedure: sp_Receiving_Load_GetAll
-- Description: Retrieves all receiving loads within a date range for Edit Mode
-- Schema: Aligned with receiving_label_data column structure

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Receiving_Load_GetAll` $$

CREATE PROCEDURE `sp_Receiving_Load_GetAll`(
    IN p_StartDate DATE,
    IN p_EndDate DATE
)
BEGIN
    SELECT
        id,
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
        part_description,
        created_at
    FROM receiving_history
    WHERE transaction_date >= p_StartDate
      AND transaction_date <= p_EndDate
    ORDER BY transaction_date DESC, id ASC;
END$$

DELIMITER ;
