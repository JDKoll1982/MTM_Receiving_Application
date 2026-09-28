-- ============================================================================
-- Migration: 12_Migration_dunnage_json_to_udc
-- Module: Dunnage
-- Purpose:
--   Parse the existing dunnage JSON spec payloads and backfill the new
--   udc1..udc10 (User Defined Column) columns introduced by the JSON -> UDC
--   refactor. This is the one-time data migration that preserves existing data.
--
--   Sources -> targets:
--     dunnage_specs.spec_value       -> dunnage_custom_fields (+ choices rows)
--     dunnage_parts.spec_values      -> dunnage_parts.udc1..udc10
--     dunnage_history.specs_json     -> dunnage_history.udc1..udc10
--     dunnage_label_data.specs_json  -> dunnage_label_data.udc1..udc10
--
--   Slot mapping rule: dunnage_custom_fields.DisplayOrder (1..10) IS the UDC
--   slot, so a field with DisplayOrder N lives in column udc{N}. Field order
--   is derived from dunnage_specs.id (insertion order) within each type.
--   Custom display name = FieldName, populated from dunnage_specs.spec_key.
--
--   A spec key embedded in a part/history/queue JSON payload that has NO
--   matching dunnage_specs definition for its type is NOT migrated (there is
--   no slot to place it in); only the type's defined specs map to UDC slots.
--
-- Lifecycle:
--   Run AFTER the udc schema DDL (Schemas 05/06/31, extended custom fields,
--   choices table) and BEFORE dropping the JSON columns. Idempotent - safe to
--   run twice. Foreign-key checks are disabled during the write phases so the
--   backfill cannot be blocked by existing reference chains.
-- Prerequisite: MySQL 5.7 (no JSON_TABLE or window functions are used).
-- ============================================================================

DROP PROCEDURE IF EXISTS sp_mig12_add_column_if_missing;
DROP PROCEDURE IF EXISTS sp_mig12_add_udc_columns;
DROP PROCEDURE IF EXISTS sp_mig12_apply;

DELIMITER $$

CREATE PROCEDURE sp_mig12_add_column_if_missing(
    IN p_table_name  VARCHAR(64),
    IN p_column_name VARCHAR(64),
    IN p_ddl         TEXT
)
BEGIN
    DECLARE v_exists INT DEFAULT 0;

    SELECT COUNT(*)
    INTO v_exists
    FROM information_schema.columns
    WHERE table_schema = DATABASE()
      AND table_name    = p_table_name
      AND column_name   = p_column_name;

    IF v_exists = 0 THEN
        SET @ddl_sql = p_ddl;
        PREPARE stmt FROM @ddl_sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
END $$

CREATE PROCEDURE sp_mig12_add_udc_columns(
    IN p_table_name VARCHAR(64)
)
BEGIN
    DECLARE v_i INT DEFAULT 1;
    DECLARE v_col VARCHAR(16);
    DECLARE v_exists INT DEFAULT 0;

    SET @tbl = p_table_name;

    WHILE v_i <= 10 DO
        SET v_col = CONCAT('udc', v_i);

        SELECT COUNT(*)
        INTO v_exists
        FROM information_schema.columns
        WHERE table_schema = DATABASE()
          AND table_name    = @tbl
          AND column_name   = v_col;

        IF v_exists = 0 THEN
            SET @ddl_sql = CONCAT(
                'ALTER TABLE `', @tbl, '` ADD COLUMN `', v_col,
                '` VARCHAR(255) NULL COMMENT ''User Defined Column slot ', v_i, ''''
            );
            PREPARE stmt FROM @ddl_sql;
            EXECUTE stmt;
            DEALLOCATE PREPARE stmt;
        END IF;

        SET v_i = v_i + 1;
    END WHILE;
END $$

CREATE PROCEDURE sp_mig12_apply()
BEGIN
    DECLARE v_done          INT DEFAULT 0;
    DECLARE v_type_id       INT DEFAULT 0;
    DECLARE v_slot          INT DEFAULT 0;
    DECLARE v_field_name    VARCHAR(100) DEFAULT '';
    DECLARE v_col           VARCHAR(16) DEFAULT '';
    DECLARE v_path          TEXT;
    DECLARE v_updates       INT DEFAULT 0;
    DECLARE v_cf_created    INT DEFAULT 0;
    DECLARE v_choices_made  INT DEFAULT 0;
    DECLARE v_image_paths   INT DEFAULT 0;
    DECLARE v_has_dbc       INT DEFAULT 0;
    DECLARE v_sql           TEXT;

    DECLARE cur_fields CURSOR FOR
        SELECT DunnageTypeID, DisplayOrder, FieldName
        FROM dunnage_custom_fields
        WHERE DisplayOrder BETWEEN 1 AND 10
        ORDER BY DunnageTypeID, DisplayOrder;

    DECLARE CONTINUE HANDLER FOR NOT FOUND SET v_done = 1;

    -- ============================================================
    -- 1) Ensure the target columns and tables exist (idempotent)
    -- ============================================================
    CALL sp_mig12_add_udc_columns('dunnage_parts');
    CALL sp_mig12_add_udc_columns('dunnage_history');
    CALL sp_mig12_add_udc_columns('dunnage_label_data');

    CALL sp_mig12_add_column_if_missing('dunnage_custom_fields', 'Unit',
        'ALTER TABLE dunnage_custom_fields ADD COLUMN `Unit` VARCHAR(50) NULL COMMENT ''Unit of measure shown on the UI for this field''');
    CALL sp_mig12_add_column_if_missing('dunnage_custom_fields', 'MinValue',
        'ALTER TABLE dunnage_custom_fields ADD COLUMN `MinValue` DECIMAL(18,4) NULL COMMENT ''Minimum value enforced when the field is a Number''');
    CALL sp_mig12_add_column_if_missing('dunnage_custom_fields', 'MaxValue',
        'ALTER TABLE dunnage_custom_fields ADD COLUMN `MaxValue` DECIMAL(18,4) NULL COMMENT ''Maximum value enforced when the field is a Number''');
    CALL sp_mig12_add_column_if_missing('dunnage_custom_fields', 'DefaultValue',
        'ALTER TABLE dunnage_custom_fields ADD COLUMN `DefaultValue` VARCHAR(255) NULL COMMENT ''Default value pre-filled when the field has no stored value''');

    CREATE TABLE IF NOT EXISTS dunnage_custom_field_choices (
        ID INT AUTO_INCREMENT PRIMARY KEY COMMENT 'Unique identifier for each choice',
        CustomFieldID INT NOT NULL COMMENT 'References dunnage_custom_fields.ID',
        Choice VARCHAR(255) NOT NULL COMMENT 'Display value of the choice',
        SortOrder INT NOT NULL COMMENT 'Display order of the choice within the field',
        UNIQUE KEY IDX_UDC_CHOICES_001 (CustomFieldID, Choice),
        KEY IDX_UDC_CHOICES_002 (CustomFieldID),
        CONSTRAINT FK_UDC_CHOICES_FIELD FOREIGN KEY (CustomFieldID)
            REFERENCES dunnage_custom_fields (ID) ON DELETE CASCADE
    ) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COMMENT = 'Choice list options for dunnage custom fields of type Choices';

    -- ============================================================
    -- 2) Backfill dunnage_custom_fields from dunnage_specs
    --    (only for types that do not already have custom fields)
    --    DatabaseColumnName is optionally populated (legacy schema) as
    --    udc{slot} so the migration runs before or after it is retired.
    -- ============================================================
    SELECT COUNT(*)
    INTO v_has_dbc
    FROM information_schema.columns
    WHERE table_schema = DATABASE()
      AND table_name   = 'dunnage_custom_fields'
      AND column_name  = 'DatabaseColumnName';

    -- MySQL 5.7 cannot implicitly cast a JSON value to DECIMAL, so min/max are
    -- extracted to a string (JSON_UNQUOTE), guarded as numeric, then CAST.
    SET @min_expr = CONCAT(
        'CASE WHEN JSON_EXTRACT(ds.spec_value, ''$.minValue'') IS NULL THEN NULL ',
        'WHEN JSON_UNQUOTE(JSON_EXTRACT(ds.spec_value, ''$.minValue'')) REGEXP ''^-?[0-9]+([.][0-9]+)?$'' ',
        'THEN CAST(JSON_UNQUOTE(JSON_EXTRACT(ds.spec_value, ''$.minValue'')) AS DECIMAL(18,4)) ',
        'ELSE NULL END'
    );
    SET @max_expr = CONCAT(
        'CASE WHEN JSON_EXTRACT(ds.spec_value, ''$.maxValue'') IS NULL THEN NULL ',
        'WHEN JSON_UNQUOTE(JSON_EXTRACT(ds.spec_value, ''$.maxValue'')) REGEXP ''^-?[0-9]+([.][0-9]+)?$'' ',
        'THEN CAST(JSON_UNQUOTE(JSON_EXTRACT(ds.spec_value, ''$.maxValue'')) AS DECIMAL(18,4)) ',
        'ELSE NULL END'
    );

    SET @sql = CONCAT(
        'INSERT INTO dunnage_custom_fields (',
        IF(v_has_dbc = 1, 'DatabaseColumnName, ', ''),
        'DunnageTypeID, FieldName, FieldType, DisplayOrder, IsRequired, ',
        '`Unit`, `MinValue`, `MaxValue`, `DefaultValue`, CreatedDate, CreatedBy) ',
        'SELECT ',
        IF(v_has_dbc = 1, 'x.dbc, ', ''),
        'x.type_id, x.spec_key, x.field_type, x.slot, x.is_required, ',
        'x.unit, x.min_value, x.max_value, x.default_value, NOW(), ''migration'' ',
        'FROM (',
        'SELECT ds.type_id, ds.spec_key, ',
        'COALESCE(',
        '  NULLIF(JSON_UNQUOTE(JSON_EXTRACT(ds.spec_value, ''$.dataType'')), ''''), ',
        '  NULLIF(JSON_UNQUOTE(JSON_EXTRACT(ds.spec_value, ''$.type'')), ''''), ',
        '  ''Text'' ',
        ') AS field_type, ',
        '(SELECT COUNT(*) FROM dunnage_specs d2 WHERE d2.type_id = ds.type_id AND d2.id <= ds.id) AS slot, ',
        'IFNULL(JSON_EXTRACT(ds.spec_value, ''$.required''), false) = true AS is_required, ',
        'NULLIF(JSON_UNQUOTE(JSON_EXTRACT(ds.spec_value, ''$.unit'')), '''') AS unit, ',
        @min_expr, ' AS min_value, ',
        @max_expr, ' AS max_value, ',
        'NULLIF(JSON_UNQUOTE(JSON_EXTRACT(ds.spec_value, ''$.defaultValue'')), '''') AS default_value, ',
        'CONCAT(''udc'', (SELECT COUNT(*) FROM dunnage_specs d2 WHERE d2.type_id = ds.type_id AND d2.id <= ds.id)) AS dbc ',
        'FROM dunnage_specs ds ',
        'WHERE (SELECT COUNT(*) FROM dunnage_specs d2 WHERE d2.type_id = ds.type_id AND d2.id <= ds.id) <= 10 ',
        'AND NOT EXISTS (SELECT 1 FROM dunnage_custom_fields cf WHERE cf.DunnageTypeID = ds.type_id) ',
        ') x'
    );
    PREPARE stmt FROM @sql;
    EXECUTE stmt;
    SET v_cf_created = ROW_COUNT();
    DEALLOCATE PREPARE stmt;

    -- ============================================================
    -- 3) Backfill dunnage_custom_field_choices from spec_value.choices
    --    Tally 0..49 covers up to 50 choices per field.
    -- ============================================================
    INSERT INTO dunnage_custom_field_choices (CustomFieldID, Choice, SortOrder)
    SELECT cf.ID,
           JSON_UNQUOTE(JSON_EXTRACT(ds.spec_value, CONCAT('$.choices[', t.n, ']'))),
           t.n + 1
    FROM dunnage_custom_fields cf
    JOIN dunnage_specs ds
        ON ds.type_id  = cf.DunnageTypeID
       AND ds.spec_key = cf.FieldName COLLATE utf8mb4_unicode_ci
    JOIN (
        SELECT a.n + b.n * 10 AS n
        FROM (SELECT 0 n UNION ALL SELECT 1 UNION ALL SELECT 2 UNION ALL SELECT 3
              UNION ALL SELECT 4 UNION ALL SELECT 5 UNION ALL SELECT 6
              UNION ALL SELECT 7 UNION ALL SELECT 8 UNION ALL SELECT 9) a
        CROSS JOIN (SELECT 0 n UNION ALL SELECT 1 UNION ALL SELECT 2 UNION ALL SELECT 3
              UNION ALL SELECT 4) b
    ) t
        ON JSON_EXTRACT(ds.spec_value, CONCAT('$.choices[', t.n, ']')) IS NOT NULL
    WHERE cf.FieldType = 'Choices'
      AND NOT EXISTS (
          SELECT 1 FROM dunnage_custom_field_choices c WHERE c.CustomFieldID = cf.ID
      );
    SET v_choices_made = ROW_COUNT();

    -- ============================================================
    -- 4) Backfill udc1..udc10 on parts, queue, and history
    --    One UPDATE per custom field; the JSON key path and type id are
    --    bound as parameters (never interpolated), only the udc column name
    --    is dynamic. Field names are JSON_QUOTE'd so keys with special
    --    characters resolve to a valid JSON path.
    -- ============================================================
    SET FOREIGN_KEY_CHECKS = 0;

    OPEN cur_fields;
    read_loop: LOOP
        FETCH cur_fields INTO v_type_id, v_slot, v_field_name;
        IF v_done = 1 THEN
            LEAVE read_loop;
        END IF;

        SET v_col  = CONCAT('udc', v_slot);
        SET v_path = CONCAT('$.', JSON_QUOTE(v_field_name));

        SET @path = v_path;
        SET @type = v_type_id;

        SET @sql = CONCAT(
            'UPDATE dunnage_parts p ',
            'SET p.`', v_col, '` = JSON_UNQUOTE(JSON_EXTRACT(p.spec_values, ?)) ',
            'WHERE p.type_id = ? AND JSON_CONTAINS_PATH(p.spec_values, ''one'', ?) = 1'
        );
        PREPARE stmt FROM @sql;
        EXECUTE stmt USING @path, @type, @path;
        SET v_updates = v_updates + ROW_COUNT();
        DEALLOCATE PREPARE stmt;

        SET @sql = CONCAT(
            'UPDATE dunnage_label_data d ',
            'JOIN dunnage_parts p ON p.part_id = d.part_id ',
            'SET d.`', v_col, '` = JSON_UNQUOTE(JSON_EXTRACT(d.specs_json, ?)) ',
            'WHERE p.type_id = ? AND JSON_CONTAINS_PATH(d.specs_json, ''one'', ?) = 1'
        );
        PREPARE stmt FROM @sql;
        EXECUTE stmt USING @path, @type, @path;
        SET v_updates = v_updates + ROW_COUNT();
        DEALLOCATE PREPARE stmt;

        SET @sql = CONCAT(
            'UPDATE dunnage_history h ',
            'JOIN dunnage_parts p ON p.part_id = h.part_id ',
            'SET h.`', v_col, '` = JSON_UNQUOTE(JSON_EXTRACT(h.specs_json, ?)) ',
            'WHERE p.type_id = ? AND JSON_CONTAINS_PATH(h.specs_json, ''one'', ?) = 1'
        );
        PREPARE stmt FROM @sql;
        EXECUTE stmt USING @path, @type, @path;
        SET v_updates = v_updates + ROW_COUNT();
        DEALLOCATE PREPARE stmt;
    END LOOP;
    CLOSE cur_fields;

    -- ============================================================
    -- 5) image_path is a dedicated column, not a UDC slot - lift it out
    --    of spec_values only when the column is not already populated.
    -- ============================================================
    UPDATE dunnage_parts p
    SET p.image_path = JSON_UNQUOTE(JSON_EXTRACT(p.spec_values, '$.image_path'))
    WHERE JSON_CONTAINS_PATH(p.spec_values, 'one', '$.image_path') = 1
      AND COALESCE(p.image_path, '') = '';
    SET v_image_paths = ROW_COUNT();

    SET FOREIGN_KEY_CHECKS = 1;

    -- ============================================================
    -- 6) Summary
    -- ============================================================
    SELECT '12_Migration_dunnage_json_to_udc'  AS migration,
           v_cf_created                         AS custom_fields_created,
           v_choices_made                      AS choice_rows_created,
           v_updates                           AS udc_cells_updated,
           v_image_paths                       AS image_paths_backfilled;
END $$

DELIMITER ;

CALL sp_mig12_apply();
