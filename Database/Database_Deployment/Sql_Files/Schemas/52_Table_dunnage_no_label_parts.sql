-- ============================================================================
-- Table: dunnage_no_label_parts
-- Description: Part numbers that do NOT require a printed label. A row here is
--   the flag itself: the workflow saves that part's loads straight to
--   dunnage_history instead of queueing them in dunnage_label_data, so nothing
--   is ever sent to LabelView for it.
-- Notes:
--   * The flag is per part, not per receipt, so it is set once on the part
--     screens and holds for every receipt of that part.
--   * Deleting a part deletes its flag with it (sp_Dunnage_Parts_Delete does
--     this explicitly, and the FK covers a plain delete).
-- Feature: Dunnage Module
-- MySQL Version: 5.7 compatible
-- ============================================================================

CREATE TABLE IF NOT EXISTS dunnage_no_label_parts (
    id INT AUTO_INCREMENT PRIMARY KEY COMMENT 'Unique identifier for the flag row',
    part_id VARCHAR(50) NOT NULL COMMENT 'Part number that needs no printed label; FK to dunnage_parts.part_id',
    created_by VARCHAR(50) NOT NULL COMMENT 'User who marked the part as needing no label',
    created_date DATETIME NOT NULL COMMENT 'Timestamp when the part was marked',
    modified_by VARCHAR(50) NULL COMMENT 'User who last changed the flag',
    modified_date DATETIME NULL COMMENT 'Timestamp when the flag was last changed',
    UNIQUE KEY UX_dunnage_no_label_parts_part_id (part_id) COMMENT 'One flag per part number',
    CONSTRAINT FK_dunnage_no_label_parts_part_id FOREIGN KEY (part_id) REFERENCES dunnage_parts (part_id) ON DELETE CASCADE
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_unicode_ci COMMENT = 'Dunnage part numbers that need no printed label; their loads are saved straight to history';
