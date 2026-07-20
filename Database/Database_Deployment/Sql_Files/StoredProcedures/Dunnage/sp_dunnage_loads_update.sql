DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Loads_Update` $$

CREATE PROCEDURE `sp_Dunnage_Loads_Update`(
    IN p_load_uuid CHAR(36),
    IN p_part_id VARCHAR(50),
    IN p_quantity DECIMAL(10,2),
    IN p_po_number VARCHAR(50),
    IN p_type_id INT,
    IN p_type_name VARCHAR(100),
    IN p_type_icon VARCHAR(100),
    IN p_location VARCHAR(100),
    IN p_label_number VARCHAR(50),
    IN p_part_skid_sequence INT,
    IN p_part_skid_total INT,
    IN p_specs_json JSON,
    IN p_user VARCHAR(50)
)
BEGIN
    DECLARE v_old_foreign_key_checks INT DEFAULT @@FOREIGN_KEY_CHECKS;

    SET FOREIGN_KEY_CHECKS = 0;
    UPDATE dunnage_history
    SET
        part_id = p_part_id,
        quantity = p_quantity,
        po_number = p_po_number,
        user_id = p_user,
        type_id = p_type_id,
        type_name = p_type_name,
        type_icon = p_type_icon,
        dunnage_type_id = p_type_id,
        dunnage_type_name = p_type_name,
        dunnage_type_icon = p_type_icon,
        location = p_location,
        label_number = p_label_number,
        part_skid_sequence = p_part_skid_sequence,
        part_skid_total = p_part_skid_total,
        specs_json = p_specs_json,
        modified_by = p_user,
        modified_date = NOW()
    WHERE load_uuid = p_load_uuid;

    UPDATE dunnage_label_data
    SET
        part_id = p_part_id,
        quantity = p_quantity,
        po_number = p_po_number,
        dunnage_type_id = p_type_id,
        dunnage_type_name = p_type_name,
        dunnage_type_icon = p_type_icon,
        location = p_location,
        label_number = p_label_number,
        part_skid_sequence = p_part_skid_sequence,
        part_skid_total = p_part_skid_total,
        specs_json = p_specs_json,
        user_id = p_user
    WHERE load_uuid = p_load_uuid;
    SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;
END $$

DELIMITER;