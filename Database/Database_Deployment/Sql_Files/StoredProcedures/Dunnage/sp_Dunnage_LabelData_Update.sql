-- Stored Procedure: sp_Dunnage_LabelData_Update
-- Description: Updates one row in dunnage_label_data (active print queue)

USE mtm_receiving_application_test;

DROP PROCEDURE IF EXISTS `sp_Dunnage_LabelData_Update`;

DELIMITER $$

CREATE PROCEDURE `sp_Dunnage_LabelData_Update`(
    IN p_load_uuid CHAR(36),
    IN p_part_id VARCHAR(50),
    IN p_dunnage_type_id INT,
    IN p_dunnage_type_name VARCHAR(100),
    IN p_dunnage_type_icon VARCHAR(100),
    IN p_quantity DECIMAL(10,2),
    IN p_quantity_type VARCHAR(100),
    IN p_po_number VARCHAR(50),
    IN p_received_date DATETIME,
    IN p_user_id VARCHAR(100),
    IN p_employee_number INT,
    IN p_location VARCHAR(100),
    IN p_label_number VARCHAR(50),
    IN p_part_skid_sequence INT,
    IN p_part_skid_total INT,
    IN p_udc1 VARCHAR(255),
    IN p_udc2 VARCHAR(255),
    IN p_udc3 VARCHAR(255),
    IN p_udc4 VARCHAR(255),
    IN p_udc5 VARCHAR(255),
    IN p_udc6 VARCHAR(255),
    IN p_udc7 VARCHAR(255),
    IN p_udc8 VARCHAR(255),
    IN p_udc9 VARCHAR(255),
    IN p_udc10 VARCHAR(255)
)
BEGIN
    DECLARE v_old_foreign_key_checks INT DEFAULT @@FOREIGN_KEY_CHECKS;

    SET FOREIGN_KEY_CHECKS = 0;
    UPDATE dunnage_label_data
    SET
        part_id = p_part_id,
        dunnage_type_id = p_dunnage_type_id,
        dunnage_type_name = p_dunnage_type_name,
        dunnage_type_icon = p_dunnage_type_icon,
        quantity = p_quantity,
        po_number = p_po_number,
        received_date = COALESCE(p_received_date, received_date),
        user_id = p_user_id,
        employee_number = p_employee_number,
        location = p_location,
        label_number = p_label_number,
        part_skid_sequence = p_part_skid_sequence,
        part_skid_total = p_part_skid_total,
        specs_json = p_specs_json
    WHERE load_uuid = p_load_uuid;

    UPDATE dunnage_history
    SET
        part_id = p_part_id,
        quantity = p_quantity,
        quantity_type = COALESCE(NULLIF(TRIM(p_quantity_type), ''), 'Quantity'),
        received_date = COALESCE(p_received_date, received_date),
        user_id = p_user_id,
        employee_number = p_employee_number,
        modified_by = p_user_id,
        modified_date = NOW(),
        po_number = p_po_number,
        type_id = p_dunnage_type_id,
        type_name = p_dunnage_type_name,
        type_icon = p_dunnage_type_icon,
        dunnage_type_id = p_dunnage_type_id,
        dunnage_type_name = p_dunnage_type_name,
        dunnage_type_icon = p_dunnage_type_icon,
        location = p_location,
        label_number = p_label_number,
        part_skid_sequence = p_part_skid_sequence,
        part_skid_total = p_part_skid_total,
        udc1 = p_udc1,
        udc2 = p_udc2,
        udc3 = p_udc3,
        udc4 = p_udc4,
        udc5 = p_udc5,
        udc6 = p_udc6,
        udc7 = p_udc7,
        udc8 = p_udc8,
        udc9 = p_udc9,
        udc10 = p_udc10
    WHERE load_uuid = p_load_uuid;
    SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
END $$

DELIMITER ;