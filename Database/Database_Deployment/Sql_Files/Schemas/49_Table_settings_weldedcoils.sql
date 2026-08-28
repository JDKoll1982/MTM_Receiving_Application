-- =============================================
-- Table: settings_weldedcoils
-- Purpose: Coils that require inner-diameter welding before they go on the
--          press cradle. Maintained by the Welded Coils tool in ShipRec Tools.
-- Matches the live MySQL table (verified 2026-08-28): id PK AI, partid NN,
-- isActive TINYINT(1) default 1. No unique key on partid in the live table;
-- duplicates are blocked at the stored-procedure layer.
-- Collation is utf8mb4_unicode_ci (MySQL 5.7 compatible) so this file deploys to
-- the test server; the live 8.0 table uses utf8mb4_0900_ai_ci (CREATE TABLE IF
-- NOT EXISTS never alters an existing table).
-- =============================================

CREATE TABLE IF NOT EXISTS settings_weldedcoils (
    id       INT(11)      NOT NULL AUTO_INCREMENT COMMENT 'Auto-incrementing unique identifier',
    partid   VARCHAR(11)  NOT NULL COMMENT 'Part number that requires inner-diameter welding',
    isActive TINYINT(1)   NOT NULL DEFAULT 1 COMMENT '1 = currently requires welding, 0 = inactive',
    PRIMARY KEY (id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
  COMMENT='Coils that require inner-diameter welding before going on the press cradle';
