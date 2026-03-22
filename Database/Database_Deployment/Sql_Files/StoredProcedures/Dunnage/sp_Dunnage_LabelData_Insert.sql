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
    IN p_po_number          VARCHAR(50),
    IN p_received_date      DATETIME,
    IN p_user_id            VARCHAR(100),
    IN p_location           VARCHAR(100),
    IN p_label_number       VARCHAR(50),
    IN p_specs_json         JSON
)
BEGIN
    INSERT INTO dunnage_label_data
    (
        load_uuid,
        part_id,
        dunnage_type_id,
        dunnage_type_name,
        dunnage_type_icon,
        quantity,
        po_number,
        received_date,
        user_id,
        location,
        label_number,
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
        p_po_number,
        COALESCE(p_received_date, NOW()),
        p_user_id,
        p_location,
        p_label_number,
        p_specs_json
    );
END $$

DELIMITER ;

-- ============================================================================
