DROP TABLE IF EXISTS software_version;

CREATE TABLE software_version (
    id TINYINT NOT NULL PRIMARY KEY COMMENT 'Singleton row identifier; always 1',
    required_version VARCHAR(50) NOT NULL COMMENT 'Required semantic application version',
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Timestamp when the row was created',
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT 'Timestamp when the row was last updated',
    updated_by VARCHAR(100) NULL COMMENT 'Actor that last changed the required version'
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_unicode_ci COMMENT = 'Singleton table storing the required MTM application version';