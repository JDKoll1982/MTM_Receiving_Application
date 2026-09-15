-- =============================================
-- Table: settings_weldedcoils
-- Purpose: Coils that require inner-diameter welding before they go on the
--          press cradle. Maintained by the Welded Coils tool in ShipRec Tools.
-- Matches the live MySQL table (verified 2026-09-15): id PK AI, partid NN with
-- unique key uq_settings_weldedcoils_partid, isActive TINYINT(1) default 1.
-- The unique key is the backstop that stops duplicate part numbers from returning;
-- sp_Settings_WeldedCoils_Insert reports the duplicate first so users get a clear
-- message instead of a constraint error.
-- Collation is utf8mb4_unicode_ci (MySQL 5.7 compatible) so this file deploys to
-- the test server; the live 8.0 table uses utf8mb4_0900_ai_ci (CREATE TABLE IF
-- NOT EXISTS never alters an existing table).
-- Existing databases: this file only helps fresh deploys. To clean duplicates and
-- add the key on a database that already has the table, run
-- Database/Scripts/DedupeWeldedCoils/Dedupe_Welded_Coils.py, which performs
-- ALTER TABLE settings_weldedcoils ADD UNIQUE KEY uq_settings_weldedcoils_partid (partid);
-- =============================================

CREATE TABLE IF NOT EXISTS settings_weldedcoils (
    id       INT(11)      NOT NULL AUTO_INCREMENT COMMENT 'Auto-incrementing unique identifier',
    partid   VARCHAR(11)  NOT NULL COMMENT 'Part number that requires inner-diameter welding',
    isActive TINYINT(1)   NOT NULL DEFAULT 1 COMMENT '1 = currently requires welding, 0 = inactive',
    PRIMARY KEY (id),
    UNIQUE KEY uq_settings_weldedcoils_partid (partid)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
  COMMENT='Coils that require inner-diameter welding before going on the press cradle';
