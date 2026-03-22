DROP TABLE IF EXISTS settings_user_roles;

CREATE TABLE settings_user_roles (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    role_id INT NOT NULL,
    assigned_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY uq_settings_user_roles (user_id, role_id)
);