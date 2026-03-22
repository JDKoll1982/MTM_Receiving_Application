CREATE TABLE IF NOT EXISTS `dunnage_non_po_entries` (
    `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
    `value` VARCHAR(100) NOT NULL,
    `created_by` VARCHAR(100) NOT NULL,
    `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `use_count` INT UNSIGNED NOT NULL DEFAULT 1,
    PRIMARY KEY (`id`),
    UNIQUE KEY `uq_dunnage_non_po_value` (`value`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_unicode_ci COMMENT = 'Saved non-PO reference reasons for dunnage receiving';