-- =====================================================================
-- Table: lot_markings
-- Purpose: Holds one permanent shape and color for each heat/lot number
--          and supplier group. A row is written the first time a lot is
--          seen and is authoritative from then on, so adding a shape or
--          a color later cannot change a marking already recorded.
-- Notes: supplier_id is never NULL. MySQL treats NULLs in a unique key as
--        distinct, so a NULL supplier would allow duplicate rows for one
--        lot. Receipts with no supplier use the literal 'NONPO'.
-- =====================================================================

CREATE TABLE IF NOT EXISTS `lot_markings` (
    `id`                     INT          NOT NULL AUTO_INCREMENT COMMENT 'Surrogate key',
    `heat_number`            VARCHAR(100) NOT NULL COMMENT 'Heat/lot number exactly as captured, never rewritten',
    `heat_number_normalized` VARCHAR(100) NOT NULL COMMENT 'Matching key: upper case, separators and whitespace removed',
    `supplier_id`            VARCHAR(15)  NOT NULL DEFAULT 'NONPO' COMMENT 'Supplier group: official supplier id, or NONPO',
    `supplier_name`          VARCHAR(255) NULL COMMENT 'Supplier name as it appeared on the receipt',
    `shape_code`             VARCHAR(32)  NOT NULL COMMENT 'Assigned shape emoji, e.g. ⚡',
    `color_code`             VARCHAR(32)  NOT NULL COMMENT 'Assigned color code, e.g. TEAL',
    `marking_source`         VARCHAR(16)  NOT NULL DEFAULT 'Auto' COMMENT 'Auto, Manual or Import',
    `is_locked`              TINYINT(1)   NOT NULL DEFAULT 0 COMMENT '1 = never recompute or probe this marking',
    `first_seen_part_number` VARCHAR(50)  NULL COMMENT 'The part number that first created the lot',
    `change_reason`          VARCHAR(255) NULL COMMENT 'Why a user replaced the marking by hand',
    `resolved_by`            VARCHAR(50)  NULL COMMENT 'User who last resolved or changed the marking',
    `resolved_at`            DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'When the marking was last resolved or changed',
    `notes`                  VARCHAR(255) NULL COMMENT 'Free-text operator note, not the override reason',
    `created_at`             TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Row creation time',
    PRIMARY KEY (`id`),
    UNIQUE KEY `uq_lot_markings_heat_supplier` (`heat_number_normalized`, `supplier_id`),
    KEY `ix_lot_markings_marking` (`shape_code`, `color_code`),
    KEY `ix_lot_markings_resolved_at` (`resolved_at`)
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_unicode_ci
  COMMENT = 'Permanent heat/lot shape and color markings, one row per lot and supplier';
