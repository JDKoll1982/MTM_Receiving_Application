CREATE TABLE IF NOT EXISTS dunnage_quantity_types (
    id INT AUTO_INCREMENT PRIMARY KEY COMMENT 'Unique identifier for a reusable dunnage quantity label header',
    quantity_type VARCHAR(100) NOT NULL UNIQUE COMMENT 'Display label used for quantity on dunnage labels, for example Weight or Pieces',
    created_by VARCHAR(50) NOT NULL DEFAULT 'SYSTEM' COMMENT 'User who first saved this reusable quantity type',
    created_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Timestamp when the quantity type was created'
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_unicode_ci COMMENT = 'Reusable quantity label headers for Dunnage part setup';

INSERT IGNORE INTO dunnage_quantity_types (quantity_type, created_by)
VALUES
    ('Weight', 'SYSTEM'),
    ('Gallons', 'SYSTEM'),
    ('Boxes', 'SYSTEM'),
    ('Sheets', 'SYSTEM'),
    ('Bags', 'SYSTEM'),
    ('Pieces', 'SYSTEM');