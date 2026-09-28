-- ============================================================================
-- Migration: 15_Migration_dunnage_spec_defaults_and_delete_cascade
-- Module: Dunnage
-- Purpose:
--   1) Backfill a default value on every Choices custom field that does not
--      currently have one (first choice by SortOrder becomes the default).
--   2) Backfill every existing dunnage_label_data / dunnage_history row whose
--      udc slot is blank/NULL for a field that now has a default.
--
-- Stored procedures changed by this feature are NOT re-created here; the
-- canonical definitions live under Sql_Files/StoredProcedures/Dunnage and are
-- (re)deployed by the deployment tool. This migration only fixes existing data:
--   - sp_Dunnage_CustomFields_Insert   now backfills blank slots when a new spec
--                                       with a default is added.
--   - sp_Dunnage_CustomFields_Update   now propagates a changed default to saved
--                                       rows that still hold the old default.
--   - sp_Dunnage_Parts_UpdateWithReferences now rewrites saved rows whose stored
--                                       location equals the part's old home
--                                       location when the home location changes.
--   - sp_Dunnage_Parts_Delete / sp_Dunnage_Types_Delete now cascade-remove
--                                       label data / history / inventory rows.
--   - sp_Dunnage_Parts_GetDeleteImpact / sp_Dunnage_Types_GetDeleteImpact are new.
-- Lifecycle: Run AFTER 13_Migration_dunnage_retire_json on existing DBs.
-- ============================================================================

DROP PROCEDURE IF EXISTS sp_mig15_backfill_spec_defaults;

DELIMITER $$

CREATE PROCEDURE sp_mig15_backfill_spec_defaults()
BEGIN
    DECLARE v_slot INT DEFAULT 1;

    WHILE v_slot <= 10 DO
        SET @slot := v_slot;

        -- 1) Choices fields without a default get their first choice (by sort order).
        SET @sql := CONCAT(
            'UPDATE dunnage_custom_fields cf ',
            'JOIN dunnage_custom_field_choices c ON c.CustomFieldID = cf.ID ',
            'SET cf.DefaultValue = c.Choice ',
            'WHERE cf.FieldType = ''Choices'' ',
            'AND (cf.DefaultValue IS NULL OR TRIM(cf.DefaultValue) = '''') ',
            'AND cf.DisplayOrder = ', @slot, ' ',
            'AND c.SortOrder = (SELECT MIN(c2.SortOrder) FROM dunnage_custom_field_choices c2 WHERE c2.CustomFieldID = cf.ID)'
        );
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;

        -- 2) Backfill blank udc slots on archived history rows for the field's type.
        SET @sql := CONCAT(
            'UPDATE dunnage_history h ',
            'JOIN dunnage_parts p ON p.part_id = h.part_id ',
            'JOIN dunnage_custom_fields cf ON cf.DunnageTypeID = p.type_id AND cf.DisplayOrder = ', @slot, ' ',
            'SET h.udc', @slot, ' = cf.DefaultValue ',
            'WHERE cf.DefaultValue IS NOT NULL AND TRIM(cf.DefaultValue) <> '''' ',
            'AND (h.udc', @slot, ' IS NULL OR TRIM(h.udc', @slot, ') = '''')'
        );
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;

        -- 3) Backfill blank udc slots on active label-data queue rows.
        SET @sql := CONCAT(
            'UPDATE dunnage_label_data d ',
            'JOIN dunnage_parts p ON p.part_id = d.part_id ',
            'JOIN dunnage_custom_fields cf ON cf.DunnageTypeID = p.type_id AND cf.DisplayOrder = ', @slot, ' ',
            'SET d.udc', @slot, ' = cf.DefaultValue ',
            'WHERE cf.DefaultValue IS NOT NULL AND TRIM(cf.DefaultValue) <> '''' ',
            'AND (d.udc', @slot, ' IS NULL OR TRIM(d.udc', @slot, ') = '''')'
        );
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;

        SET v_slot := v_slot + 1;
    END WHILE;
END $$

DELIMITER ;

-- The backfill intentionally touches many rows whose WHERE clauses do not use a
-- KEY column on the table being updated (e.g. blank udc slots across all history
-- and label-data rows). MySQL Workbench enables SQL_SAFE_UPDATES by default,
-- which rejects those updates with error 1175. Disable it for this one-time data
-- fix and restore the previous session value immediately afterward.
SET @mig15_previous_safe_updates = @@SESSION.SQL_SAFE_UPDATES;
SET SESSION SQL_SAFE_UPDATES = 0;

CALL sp_mig15_backfill_spec_defaults();

SET SESSION SQL_SAFE_UPDATES = @mig15_previous_safe_updates;

DROP PROCEDURE IF EXISTS sp_mig15_backfill_spec_defaults;
