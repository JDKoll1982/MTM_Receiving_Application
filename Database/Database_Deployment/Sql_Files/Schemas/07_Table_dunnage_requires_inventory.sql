SET
    FOREIGN_KEY_CHECKS = 0;

DROP TABLE IF EXISTS dunnage_requires_inventory;

SET
    FOREIGN_KEY_CHECKS = 1;

CREATE TABLE dunnage_requires_inventory (
    id INT AUTO_INCREMENT PRIMARY KEY COMMENT 'Unique identifier for inventoried dunnage record',
    part_id VARCHAR(50) NOT NULL COMMENT 'Reference to dunnage part requiring inventory tracking',
    inventory_method VARCHAR(100) COMMENT 'Method used for Visual ERP inventory notification (e.g., manual, automatic)',
    notes TEXT COMMENT 'Additional notes about inventory handling or special requirements',
    created_by VARCHAR(50) NOT NULL COMMENT 'Username who created the record',
    created_date DATETIME NOT NULL COMMENT 'Timestamp when record was created',
    modified_by VARCHAR(50) COMMENT 'Username who last modified the record',
    modified_date DATETIME COMMENT 'Timestamp when record was last modified',
    CONSTRAINT FK_dunnage_requires_inventory_part_id FOREIGN KEY (part_id) REFERENCES dunnage_parts (part_id) ON DELETE CASCADE
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_unicode_ci COMMENT = 'Dunnage parts requiring Visual ERP inventory notification and tracking';