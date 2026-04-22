DROP TABLE IF EXISTS settings_volvo_recipents;

CREATE TABLE IF NOT EXISTS settings_volvo_recipents (
    id INT NOT NULL AUTO_INCREMENT COMMENT 'Primary key for the Volvo recipient row',
    first_name VARCHAR(100) NOT NULL COMMENT 'Recipient first name',
    last_name VARCHAR(100) NOT NULL COMMENT 'Recipient last name',
    recipient_type ENUM('To', 'CC') NOT NULL COMMENT 'Outlook recipient bucket',
    email VARCHAR(255) NOT NULL COMMENT 'Full email address saved exactly as entered',
    PRIMARY KEY (id),
    UNIQUE KEY uq_settings_volvo_recipents_type_email (recipient_type, email)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_unicode_ci COMMENT = 'Volvo email recipients used in the shipment-entry email preview';