-- =====================================================================
-- Migration: 16_Migration_lot_markings
-- Module:    Receiving
-- Purpose:
--   1. Add heat_shape_code and heat_color_code to receiving_label_data.
--   2. Add heat_shape_code and heat_color_code to receiving_history.
--   3. Put the marking onto any reprint queue row that is already waiting, so a reprinted
--      label shows the marking its history row recorded.
--
-- Lifecycle: Run AFTER 15_Migration_dunnage_spec_defaults_and_delete_cascade.
--
-- Safety:    Strictly additive and idempotent. Both columns are NULL, no existing column is
--            altered, no row is deleted, and the helper drops itself at the end. A build that
--            predates this feature is unaffected: it never selects or writes either column, and
--            the receiving procedures keep their existing parameter lists.
--
-- Scope:     This file performs the column adds only. The two new tables, the new procedures
--            and the updated receiving procedures are deployed from their canonical files under
--            Sql_Files/Schemas and Sql_Files/StoredProcedures by the deployment tool, which is
--            the same arrangement 15_Migration_dunnage_spec_defaults_and_delete_cascade uses.
--            Re-deploying the procedure files is required alongside this migration, because
--            those procedures enumerate their columns explicitly.
-- =====================================================================

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS sp_mig16_add_column_if_missing;
DROP PROCEDURE IF EXISTS sp_mig16_apply;

DELIMITER $$

CREATE PROCEDURE sp_mig16_add_column_if_missing(
    IN p_table_name VARCHAR(64),
    IN p_column_name VARCHAR(64),
    IN p_ddl TEXT
)
BEGIN
    DECLARE v_exists INT DEFAULT 0;

    SELECT COUNT(*)
    INTO v_exists
    FROM information_schema.columns
    WHERE table_schema = DATABASE()
      AND table_name = p_table_name
      AND column_name = p_column_name;

    IF v_exists = 0 THEN
        SET @ddl_sql = p_ddl;
        PREPARE stmt FROM @ddl_sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
END $$

CREATE PROCEDURE sp_mig16_apply()
BEGIN
    CALL sp_mig16_add_column_if_missing('receiving_label_data', 'heat_shape_code',
        'ALTER TABLE receiving_label_data ADD COLUMN heat_shape_code VARCHAR(32) NULL COMMENT ''Heat/lot marking shape emoji, resolved once from lot_markings.shape_code'' AFTER heat');
    CALL sp_mig16_add_column_if_missing('receiving_label_data', 'heat_color_code',
        'ALTER TABLE receiving_label_data ADD COLUMN heat_color_code VARCHAR(32) NULL COMMENT ''Heat/lot marking color code, resolved once from lot_markings.color_code'' AFTER heat_shape_code');

    CALL sp_mig16_add_column_if_missing('receiving_history', 'heat_shape_code',
        'ALTER TABLE receiving_history ADD COLUMN heat_shape_code VARCHAR(32) NULL COMMENT ''Heat/lot marking shape emoji, resolved once from lot_markings.shape_code'' AFTER heat');
    CALL sp_mig16_add_column_if_missing('receiving_history', 'heat_color_code',
        'ALTER TABLE receiving_history ADD COLUMN heat_color_code VARCHAR(32) NULL COMMENT ''Heat/lot marking color code, resolved once from lot_markings.color_code'' AFTER heat_shape_code');

    -- Carry the marking onto reprint rows that are already sitting in the queue. Reprint rows
    -- were queued before the columns existed, so their marking is still only on the history row.
    UPDATE receiving_label_data rld
    INNER JOIN receiving_history rh ON rh.load_guid = rld.load_id
    SET rld.heat_shape_code = rh.heat_shape_code,
        rld.heat_color_code = rh.heat_color_code
    WHERE rld.is_reprint = 1
      AND rld.heat_shape_code IS NULL
      AND rh.heat_shape_code IS NOT NULL;
END $$

DELIMITER ;

CALL sp_mig16_apply();

DROP PROCEDURE IF EXISTS sp_mig16_apply;
DROP PROCEDURE IF EXISTS sp_mig16_add_column_if_missing;
