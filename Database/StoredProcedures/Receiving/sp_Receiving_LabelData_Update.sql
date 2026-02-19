-- Stored Procedure: sp_Receiving_LabelData_Update
-- Description: Updates one row in receiving_label_data (active print queue)

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Receiving_LabelData_Update`;

DELIMITER $$

CREATE PROCEDURE `sp_Receiving_LabelData_Update`(
    IN p_load_id CHAR(36),
    IN p_load_number INT,
    IN p_quantity INT,
    IN p_weight_quantity DECIMAL(18,2),
    IN p_part_id VARCHAR(50),
    IN p_part_description VARCHAR(500),
    IN p_part_type VARCHAR(50),
    IN p_po_number VARCHAR(20),
    IN p_po_line_number VARCHAR(10),
    IN p_po_vendor VARCHAR(255),
    IN p_po_status VARCHAR(100),
    IN p_po_due_date DATE,
    IN p_qty_ordered DECIMAL(18,2),
    IN p_unit_of_measure VARCHAR(20),
    IN p_remaining_quantity INT,
    IN p_employee_number INT,
    IN p_user_id VARCHAR(100),
    IN p_heat VARCHAR(100),
    IN p_received_date DATETIME,
    IN p_transaction_date DATE,
    IN p_initial_location VARCHAR(50),
    IN p_packages_per_load INT,
    IN p_package_type_name VARCHAR(50),
    IN p_weight_per_package DECIMAL(18,2),
    IN p_coils_on_skid INT,
    IN p_label_number INT,
    IN p_vendor_name VARCHAR(255),
    IN p_is_non_po_item TINYINT(1),
    IN p_is_quality_hold_required TINYINT(1),
    IN p_is_quality_hold_acknowledged TINYINT(1),
    IN p_quality_hold_restriction_type VARCHAR(255)
)
BEGIN
    UPDATE receiving_label_data
    SET
        load_number = p_load_number,
        quantity = p_quantity,
        weight_quantity = p_weight_quantity,
        part_id = p_part_id,
        part_description = p_part_description,
        part_type = p_part_type,
        po_number = p_po_number,
        po_line_number = p_po_line_number,
        po_vendor = p_po_vendor,
        po_status = p_po_status,
        po_due_date = p_po_due_date,
        qty_ordered = p_qty_ordered,
        unit_of_measure = p_unit_of_measure,
        remaining_quantity = p_remaining_quantity,
        employee_number = p_employee_number,
        user_id = p_user_id,
        heat = p_heat,
        received_date = p_received_date,
        transaction_date = p_transaction_date,
        initial_location = p_initial_location,
        packages_per_load = p_packages_per_load,
        package_type_name = p_package_type_name,
        weight_per_package = p_weight_per_package,
        coils_on_skid = p_coils_on_skid,
        label_number = p_label_number,
        vendor_name = p_vendor_name,
        is_non_po_item = p_is_non_po_item,
        is_quality_hold_required = p_is_quality_hold_required,
        is_quality_hold_acknowledged = p_is_quality_hold_acknowledged,
        quality_hold_restriction_type = p_quality_hold_restriction_type
    WHERE load_id = p_load_id;
END$$

DELIMITER ;
