-- Core Settings schema (idempotent)

DROP TABLE IF EXISTS settings_user_roles;
DROP TABLE IF EXISTS settings_roles;
DROP TABLE IF EXISTS settings_activity;
DROP TABLE IF EXISTS settings_personal;
DROP TABLE IF EXISTS settings_universal;

CREATE TABLE settings_universal (
    id INT AUTO_INCREMENT PRIMARY KEY,
    category VARCHAR(100) NOT NULL,
    setting_key VARCHAR(150) NOT NULL,
    setting_value TEXT NOT NULL,
    data_type VARCHAR(50) NOT NULL,
    is_sensitive TINYINT(1) NOT NULL DEFAULT 0,
    is_locked TINYINT(1) NOT NULL DEFAULT 0,
    updated_by VARCHAR(100) NOT NULL,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY uq_settings_universal (category, setting_key)
);

CREATE TABLE settings_personal (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    category VARCHAR(100) NOT NULL,
    setting_key VARCHAR(150) NOT NULL,
    setting_value TEXT NOT NULL,
    data_type VARCHAR(50) NOT NULL,
    updated_by VARCHAR(100) NOT NULL,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY uq_settings_personal (user_id, category, setting_key)
);

CREATE TABLE settings_activity (
    id INT AUTO_INCREMENT PRIMARY KEY,
    scope VARCHAR(20) NOT NULL,
    category VARCHAR(100) NOT NULL,
    setting_key VARCHAR(150) NOT NULL,
    old_value TEXT NULL,
    new_value TEXT NULL,
    change_type VARCHAR(50) NOT NULL,
    user_id INT NULL,
    changed_by VARCHAR(100) NOT NULL,
    changed_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ip_address VARCHAR(50) NULL,
    workstation VARCHAR(100) NULL,
    INDEX idx_settings_activity_key (category, setting_key)
);

CREATE TABLE settings_roles (
    id INT AUTO_INCREMENT PRIMARY KEY,
    role_name VARCHAR(50) NOT NULL UNIQUE,
    description VARCHAR(255) NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE settings_user_roles (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    role_id INT NOT NULL,
    assigned_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY uq_settings_user_roles (user_id, role_id)
);
