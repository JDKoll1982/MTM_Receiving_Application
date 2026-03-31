DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Dunnage_Loads_Update`$$

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
    IN p_specs_json JSON,
    IN p_user VARCHAR(50)
)
BEGIN
    UPDATE dunnage_history
    SET
        part_id = p_part_id,
        quantity = p_quantity,
        po_number = p_po_number,
        type_id = p_type_id,
        type_name = p_type_name,
        type_icon = p_type_icon,
        location = p_location,
        label_number = p_label_number,
        specs_json = p_specs_json,
        modified_by = p_user,
        modified_date = NOW()
    WHERE load_uuid = p_load_uuid;
END $$

DELIMITER ;
