DROP TABLE IF EXISTS receiving_vendor_variables;

CREATE TABLE receiving_vendor_variables (
	vendor_name VARCHAR(100) NOT NULL PRIMARY KEY COMMENT 'Vendor name from Infor Visual (unique key)',
	variable_name VARCHAR(100) NOT NULL COMMENT 'Custom variable label for this vendor (e.g., DM #, Heat #)',
	created_at DATETIME DEFAULT CURRENT_TIMESTAMP COMMENT 'Timestamp when record was created',
	updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT 'Timestamp when record was last updated',
	created_by INT NULL COMMENT 'FK to auth_users table (nullable)',
	INDEX idx_vendor_name (vendor_name)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_unicode_ci COMMENT = 'Global vendor-specific variable name mappings for Heat/Lot workflow';

-- Seed default Skana mapping
INSERT IGNORE INTO receiving_vendor_variables (
	vendor_name,
	variable_name
)
VALUES
	('Skana', 'DM #');
