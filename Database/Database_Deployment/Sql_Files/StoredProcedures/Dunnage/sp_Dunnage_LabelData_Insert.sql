-- Stored Procedure: sp_Dunnage_LabelData_Insert
-- Description: Inserts one row into dunnage_label_data (active print queue for LabelView2022)
-- Lifecycle: Workflow Complete -> Queue Insert

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Dunnage_LabelData_Insert`;

DELIMITER $$

CREATE PROCEDURE `sp_Dunnage_LabelData_Insert`(
    IN p_load_uuid          CHAR(36),
    IN p_part_id            VARCHAR(50),
    IN p_dunnage_type_id    INT,
    IN p_dunnage_type_name  VARCHAR(100),
    IN p_dunnage_type_icon  VARCHAR(100),
    IN p_quantity           DECIMAL(10,2),
    IN p_quantity_type      VARCHAR(100),
    IN p_po_number          VARCHAR(50),
    IN p_received_date      DATETIME,
    IN p_user_id            VARCHAR(100),
    IN p_employee_number    INT,
    IN p_location           VARCHAR(100),
    IN p_label_number       VARCHAR(50),
    IN p_part_skid_sequence INT,
    IN p_part_skid_total    INT,
    IN p_specs_json         JSON
)
BEGIN
    SET FOREIGN_KEY_CHECKS = 0;
    INSERT INTO dunnage_label_data
    (
        load_uuid,
        part_id,
        dunnage_type_id,
        dunnage_type_name,
        dunnage_type_icon,
        quantity,
        quantity_type,
        po_number,
        received_date,
        user_id,
        employee_number,
        location,
        label_number,
        part_skid_sequence,
        part_skid_total,
        specs_json
    )
    VALUES
    (
        p_load_uuid,
        p_part_id,
        p_dunnage_type_id,
        p_dunnage_type_name,
        p_dunnage_type_icon,
        p_quantity,
        COALESCE(NULLIF(TRIM(p_quantity_type), ''), 'Quantity'),
        p_po_number,
        COALESCE(p_received_date, NOW()),
        p_user_id,
        p_employee_number,
        p_location,
        p_label_number,
        p_part_skid_sequence,
        p_part_skid_total,
        p_specs_json
    );
    SET FOREIGN_KEY_CHECKS = 1;
END $$

DELIMITER ;

-- ============================================================================
