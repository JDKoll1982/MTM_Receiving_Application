DROP TABLE IF EXISTS settings_reporting_recipients;

CREATE TABLE IF NOT EXISTS settings_reporting_recipients (
    id INT NOT NULL AUTO_INCREMENT COMMENT 'Primary key for the Reporting recipient row',
    first_name VARCHAR(100) NOT NULL COMMENT 'Recipient first name',
    last_name VARCHAR(100) NOT NULL COMMENT 'Recipient last name',
    recipient_type ENUM('To', 'CC') NOT NULL COMMENT 'Outlook recipient bucket',
    email VARCHAR(255) NOT NULL COMMENT 'Full email address saved exactly as entered',
    PRIMARY KEY (id),
    UNIQUE KEY uq_settings_reporting_recipients_type_email (recipient_type, email)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_unicode_ci COMMENT = 'Reporting preview recipients copied from the reporting preview surface';