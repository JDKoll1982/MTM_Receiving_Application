-- ============================================================================
-- Migration: 13_Migration_dunnage_retire_json
-- Module: Dunnage
-- Purpose:
--   Final cleanup AFTER 12_Migration_dunnage_json_to_udc has backfilled the
--   udc1..udc10 columns. Drops the legacy JSON spec columns (spec_values,
--   specs_json), the retired dunnage_specs table, and the obsolete
--   sp_Dunnage_Specs_* / sp_Dunnage_LabelData_InsertBatch stored procedures.
-- Lifecycle: Run AFTER 12_Migration_dunnage_json_to_udc on existing DBs.
-- ============================================================================

DROP PROCEDURE IF EXISTS sp_mig13_drop_column_if_exists;
DROP PROCEDURE IF EXISTS sp_mig13_drop_indexes_for_column;

DELIMITER $$

CREATE PROCEDURE sp_mig13_drop_column_if_exists(
    IN p_table  VARCHAR(64),
    IN p_column VARCHAR(64)
)
BEGIN
    DECLARE v_exists INT DEFAULT 0;

    SELECT COUNT(*)
    INTO v_exists
    FROM information_schema.columns
    WHERE table_schema = DATABASE()
      AND table_name    = p_table
      AND column_name   = p_column;

    IF v_exists = 1 THEN
        SET @ddl = CONCAT('ALTER TABLE `', p_table, '` DROP COLUMN `', p_column, '`');
        PREPARE stmt FROM @ddl;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
END $$

CREATE PROCEDURE sp_mig13_drop_indexes_for_column(
    IN p_table  VARCHAR(64),
    IN p_column VARCHAR(64)
)
BEGIN
    DECLARE v_done INT DEFAULT 0;
    DECLARE v_idx  VARCHAR(64);
    DECLARE cur_idx CURSOR FOR
        SELECT DISTINCT INDEX_NAME
        FROM information_schema.statistics
        WHERE table_schema = DATABASE()
          AND table_name   = p_table
          AND column_name  = p_column;
    DECLARE CONTINUE HANDLER FOR NOT FOUND SET v_done = 1;

    OPEN cur_idx;
    idx_loop: LOOP
        FETCH cur_idx INTO v_idx;
        IF v_done = 1 THEN
            LEAVE idx_loop;
        END IF;
        SET @ddl = CONCAT('ALTER TABLE `', p_table, '` DROP INDEX `', v_idx, '`');
        PREPARE stmt FROM @ddl;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END LOOP;
    CLOSE cur_idx;
END $$

DELIMITER ;

CALL sp_mig13_drop_column_if_exists('dunnage_parts', 'spec_values');
CALL sp_mig13_drop_column_if_exists('dunnage_history', 'specs_json');
CALL sp_mig13_drop_column_if_exists('dunnage_label_data', 'specs_json');
CALL sp_mig13_drop_indexes_for_column('dunnage_custom_fields', 'DatabaseColumnName');
CALL sp_mig13_drop_column_if_exists('dunnage_custom_fields', 'DatabaseColumnName');

DROP PROCEDURE IF EXISTS sp_mig13_drop_column_if_exists;
DROP PROCEDURE IF EXISTS sp_mig13_drop_indexes_for_column;

SET FOREIGN_KEY_CHECKS = 0;
DROP TABLE IF EXISTS dunnage_specs;
SET FOREIGN_KEY_CHECKS = 1;

DROP PROCEDURE IF EXISTS `sp_Dunnage_Specs_CountPartsUsingSpec`;
DROP PROCEDURE IF EXISTS `sp_Dunnage_Specs_DeleteById`;
DROP PROCEDURE IF EXISTS `sp_Dunnage_Specs_DeleteByType`;
DROP PROCEDURE IF EXISTS `sp_Dunnage_Specs_GetAll`;
DROP PROCEDURE IF EXISTS `sp_Dunnage_Specs_GetAllKeys`;
DROP PROCEDURE IF EXISTS `sp_Dunnage_Specs_GetById`;
DROP PROCEDURE IF EXISTS `sp_Dunnage_Specs_GetByType`;
DROP PROCEDURE IF EXISTS `sp_dunnage_specs_insert`;
DROP PROCEDURE IF EXISTS `sp_dunnage_specs_update`;
DROP PROCEDURE IF EXISTS `sp_Dunnage_LabelData_InsertBatch`;
