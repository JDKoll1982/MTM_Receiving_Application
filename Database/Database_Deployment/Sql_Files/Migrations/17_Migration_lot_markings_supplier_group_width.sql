-- =====================================================================
-- Migration: 17_Migration_lot_markings_supplier_group_width
-- Module:    ShipRec Tools (Heat / Lot Management)
-- Purpose:
--   Widen lot_markings.supplier_id from VARCHAR(15) to VARCHAR(255), and pin the table to
--   ROW_FORMAT DYNAMIC, so the supplier group key can hold what the feature writes into it.
--
-- Why:
--   The supplier group is the resolved official supplier id when the Infor Visual vendor search
--   returns exactly one match, and the normalized supplier name otherwise. The name is the common
--   case, and po_vendor and vendor_name are VARCHAR(255) on both receiving tables, so the group key
--   has to be able to carry a real vendor name. At 15 characters it could not, and MySQL rejected
--   the value before a marking could be recorded, with:
--       Data too long for column 'p_supplier_id' at row 1   (error 1406)
--   The same 15 character width was declared on the p_supplier_id parameter of sp_LotMarkings_Resolve
--   and sp_LotMarkings_Insert, so the value was refused at the procedure boundary first and the
--   column would have refused it too.
--
--   Shortening the name to fit was rejected on purpose. Two vendors can share their first fifteen
--   characters, for example two divisions of one manufacturer. Truncating would collapse them into a
--   single group and hand them one shared marking, which is exactly what FR-005 forbids: the same
--   heat number from a second supplier has to stay a distinguishable lot.
--
-- Lifecycle: Run AFTER 16_Migration_lot_markings.
--
-- Safety:    Widens only. No row is deleted, no stored value is rewritten, and the column keeps its
--            NOT NULL and its DEFAULT 'NONPO'. Widening cannot create a duplicate under the unique
--            key, so no existing row can be invalidated. Idempotent: a second run finds the column
--            already 255 and does nothing. The helper drops itself at the end.
--
-- Follow-up: A procedure's declared parameter width is not something this file can change from here,
--            because the procedures are deployed from their canonical files and copying their bodies
--            in would let two copies drift apart. So this migration has to be followed by the
--            StoredProcedures step of the deployer, which re-creates sp_LotMarkings_Resolve.sql and
--            sp_LotMarkings_Insert.sql. The verification query at the end of this file reports the
--            width both halves are actually declared at, so a hand run can see whether that step
--            still owes work instead of inferring it from a second failure.
--
-- Note:      At 255 characters the unique key uq_lot_markings_heat_supplier is 1420 bytes in
--            utf8mb4, which needs ROW_FORMAT DYNAMIC and a large prefix (3072 bytes). MySQL 5.7
--            defaults to both, but the ALTER sets the row format explicitly rather than assuming it,
--            so a server that cannot take the wider key fails loudly and visibly here instead of
--            leaving a table that silently cannot hold its own key.
-- =====================================================================

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS sp_mig17_apply;

DELIMITER $$

CREATE PROCEDURE sp_mig17_apply()
BEGIN
    DECLARE v_length INT DEFAULT NULL;

    SELECT CHARACTER_MAXIMUM_LENGTH
    INTO v_length
    FROM information_schema.columns
    WHERE table_schema = DATABASE()
      AND table_name = 'lot_markings'
      AND column_name = 'supplier_id';

    -- A fresh install builds this column at 255 from the schema file, so it finds nothing to do.
    IF v_length IS NOT NULL AND v_length < 255 THEN
        SET @ddl_sql = 'ALTER TABLE lot_markings
            ROW_FORMAT = DYNAMIC,
            MODIFY COLUMN supplier_id VARCHAR(255) NOT NULL DEFAULT ''NONPO''
                COMMENT ''Supplier group: official supplier id, the normalized supplier name when no id resolved, or NONPO''';
        PREPARE stmt FROM @ddl_sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
END $$

DELIMITER ;

CALL sp_mig17_apply();

DROP PROCEDURE IF EXISTS sp_mig17_apply;

-- Verification. Every row should read 'ok'. A procedure still reporting 15 has not been re-created
-- yet: run the StoredProcedures step of the deployer, or run these two files by hand.
--   Sql_Files/StoredProcedures/Settings/sp_LotMarkings_Resolve.sql
--   Sql_Files/StoredProcedures/Settings/sp_LotMarkings_Insert.sql
SELECT
    'column' AS `object`,
    'lot_markings.supplier_id' AS `name`,
    CAST(c.`CHARACTER_MAXIMUM_LENGTH` AS CHAR) AS `declared_length`,
    CASE WHEN c.`CHARACTER_MAXIMUM_LENGTH` >= 255 THEN 'ok' ELSE 'too narrow' END AS `state`
FROM information_schema.`COLUMNS` AS c
WHERE c.`TABLE_SCHEMA` = DATABASE()
  AND c.`TABLE_NAME` = 'lot_markings'
  AND c.`COLUMN_NAME` = 'supplier_id'

UNION ALL

SELECT
    'parameter' AS `object`,
    CONCAT(p.`SPECIFIC_NAME`, '.', p.`PARAMETER_NAME`) AS `name`,
    CAST(p.`CHARACTER_MAXIMUM_LENGTH` AS CHAR) AS `declared_length`,
    CASE
        WHEN p.`CHARACTER_MAXIMUM_LENGTH` >= 255 THEN 'ok'
        ELSE 're-run the procedure file'
    END AS `state`
FROM information_schema.`PARAMETERS` AS p
WHERE p.`SPECIFIC_SCHEMA` = DATABASE()
  AND p.`PARAMETER_NAME` = 'p_supplier_id';
