CREATE TABLE IF NOT EXISTS `dunnage_non_po_part_defaults` (
    `part_id` VARCHAR(100) NOT NULL,
    `value` VARCHAR(100) NOT NULL,
    `updated_by` VARCHAR(100) NOT NULL,
    `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`part_id`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_unicode_ci COMMENT = 'Per-part default non-PO references for dunnage';