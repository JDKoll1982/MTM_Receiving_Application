-- Stored Procedure: sp_Receiving_LabelData_Insert
-- Description: Inserts one row into receiving_label_data (active print queue for LabelView2022)
-- Lifecycle: Workflow Complete -> Queue Insert

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Receiving_LabelData_Insert`;

DELIMITER $$

CREATE PROCEDURE `sp_Receiving_LabelData_Insert`(
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
    INSERT INTO receiving_label_data
    (
        load_id,
        load_number,
        quantity,
        weight_quantity,
        part_id,
        part_description,
        part_type,
        po_number,
        po_line_number,
        po_vendor,
        po_status,
        po_due_date,
        qty_ordered,
        unit_of_measure,
        remaining_quantity,
        employee_number,
        user_id,
        heat,
        received_date,
        transaction_date,
        initial_location,
        packages_per_load,
        package_type_name,
        weight_per_package,
        coils_on_skid,
        label_number,
        vendor_name,
        is_non_po_item,
        is_quality_hold_required,
        is_quality_hold_acknowledged,
        quality_hold_restriction_type
    )
    VALUES
    (
        p_load_id,
        p_load_number,
        p_quantity,
        p_weight_quantity,
        p_part_id,
        p_part_description,
        p_part_type,
        p_po_number,
        p_po_line_number,
        p_po_vendor,
        p_po_status,
        p_po_due_date,
        p_qty_ordered,
        p_unit_of_measure,
        p_remaining_quantity,
        p_employee_number,
        p_user_id,
        p_heat,
        p_received_date,
        p_transaction_date,
        p_initial_location,
        p_packages_per_load,
        p_package_type_name,
        p_weight_per_package,
        p_coils_on_skid,
        p_label_number,
        p_vendor_name,
        p_is_non_po_item,
        p_is_quality_hold_required,
        p_is_quality_hold_acknowledged,
        p_quality_hold_restriction_type
    );
END$$

DELIMITER ;
