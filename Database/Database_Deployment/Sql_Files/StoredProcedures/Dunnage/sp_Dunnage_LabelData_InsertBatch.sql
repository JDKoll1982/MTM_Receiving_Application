-- Stored Procedure: sp_Dunnage_LabelData_InsertBatch
-- Description: Iterates a JSON array and inserts each element into dunnage_label_data.
--              Each element must carry all fixed fields plus specs_json.
-- Lifecycle: Workflow Complete -> Batch Queue Insert
-- JSON element shape:
--   {
--     "load_uuid":         "<CHAR(36)>",
--     "part_id":           "<VARCHAR(50)>",
--     "dunnage_type_id":   <INT|null>,
--     "dunnage_type_name": "<VARCHAR(100)|null>",
--     "dunnage_type_icon": "<VARCHAR(100)|null>",
--     "quantity":          <DECIMAL>,
--     "po_number":         "<VARCHAR(50)|null>",
--     "received_date":     "<DATETIME>",
--     "user_id":           "<VARCHAR(100)>",
--     "location":          "<VARCHAR(100)|null>",
--     "label_number":      "<VARCHAR(50)|null>",
--     "specs_json":        {<key>:<value>, ...} or null
--   }

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS `sp_Dunnage_LabelData_InsertBatch`;

DELIMITER $$

CREATE PROCEDURE `sp_Dunnage_LabelData_InsertBatch`(
    IN p_load_data JSON,
    IN p_user      VARCHAR(100)
)
BEGIN
    DECLARE i     INT DEFAULT 0;
    DECLARE cnt   INT DEFAULT 0;
    DECLARE v_load_uuid         CHAR(36);
    DECLARE v_part_id           VARCHAR(50);
    DECLARE v_dunnage_type_id   INT;
    DECLARE v_dunnage_type_name VARCHAR(100);
    DECLARE v_dunnage_type_icon VARCHAR(100);
    DECLARE v_quantity          DECIMAL(10,2);
    DECLARE v_po_number         VARCHAR(50);
    DECLARE v_received_date     DATETIME;
    DECLARE v_location          VARCHAR(100);
    DECLARE v_label_number      VARCHAR(50);
    DECLARE v_specs_json        JSON;

    SET cnt = JSON_LENGTH(p_load_data);

    WHILE i < cnt DO
        SET v_load_uuid         = JSON_UNQUOTE(JSON_EXTRACT(p_load_data, CONCAT('$[', i, '].load_uuid')));
        SET v_part_id           = JSON_UNQUOTE(JSON_EXTRACT(p_load_data, CONCAT('$[', i, '].part_id')));
        SET v_dunnage_type_id   = JSON_EXTRACT(p_load_data,             CONCAT('$[', i, '].dunnage_type_id'));
        SET v_dunnage_type_name = JSON_UNQUOTE(JSON_EXTRACT(p_load_data, CONCAT('$[', i, '].dunnage_type_name')));
        SET v_dunnage_type_icon = JSON_UNQUOTE(JSON_EXTRACT(p_load_data, CONCAT('$[', i, '].dunnage_type_icon')));
        SET v_quantity          = JSON_EXTRACT(p_load_data,             CONCAT('$[', i, '].quantity'));
        SET v_po_number         = JSON_UNQUOTE(JSON_EXTRACT(p_load_data, CONCAT('$[', i, '].po_number')));
        SET v_received_date     = JSON_UNQUOTE(JSON_EXTRACT(p_load_data, CONCAT('$[', i, '].received_date')));
        SET v_location          = JSON_UNQUOTE(JSON_EXTRACT(p_load_data, CONCAT('$[', i, '].location')));
        SET v_label_number      = JSON_UNQUOTE(JSON_EXTRACT(p_load_data, CONCAT('$[', i, '].label_number')));
        SET v_specs_json        = JSON_EXTRACT(p_load_data,             CONCAT('$[', i, '].specs_json'));

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
            v_load_uuid,
            v_part_id,
            v_dunnage_type_id,
            v_dunnage_type_name,
            v_dunnage_type_icon,
            v_quantity,
            NULLIF(v_po_number, 'null'),
            COALESCE(v_received_date, NOW()),
            p_user,
            NULLIF(v_location, 'null'),
            NULLIF(v_label_number, 'null'),
            v_specs_json
        );

        SET i = i + 1;
    END WHILE;
END $$

DELIMITER ;

-- ============================================================================
