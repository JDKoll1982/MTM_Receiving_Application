-- =====================================================================
-- Table: lot_marking_part_scope
-- Purpose: The opt-in list of part numbers that receive heat/lot markings.
--          A part number not present here receives no marking, and its
--          receipt is unaffected.
-- Notes: Removing a part sets is_enabled to 0 rather than deleting the row,
--        so markings already recorded for that part keep their meaning and
--        the list keeps its own history.
-- =====================================================================

CREATE TABLE IF NOT EXISTS `lot_marking_part_scope` (
    `id`         INT         NOT NULL AUTO_INCREMENT COMMENT 'Surrogate key',
    `part_id`    VARCHAR(50) NOT NULL COMMENT 'Part number that receives lot markings',
    `is_enabled` TINYINT(1)  NOT NULL DEFAULT 1 COMMENT '1 = markings are assigned, 0 = removed from the list',
    `added_by`   VARCHAR(50) NULL COMMENT 'User who added the part',
    `added_at`   DATETIME    NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'When the part was added',
    PRIMARY KEY (`id`),
    UNIQUE KEY `uq_lot_marking_part_scope_part` (`part_id`),
    KEY `ix_lot_marking_part_scope_enabled` (`is_enabled`, `part_id`)
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_unicode_ci
  COMMENT = 'Part numbers that receive heat/lot shape and color markings';
