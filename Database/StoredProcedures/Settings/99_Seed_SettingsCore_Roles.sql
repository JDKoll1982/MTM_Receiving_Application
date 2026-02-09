-- =============================================
-- Author: MTM Development
-- Create date: 2026-02-05
-- Description: Seeds default settings roles
--              Provides the base roles for the permission system
-- =============================================

-- Insert default roles if they don't exist
INSERT IGNORE INTO settings_roles (role_name, description, created_at)
VALUES 
    ('User', 'Basic user role - can modify own settings', NOW()),
    ('Supervisor', 'Supervisor role - can manage user settings', NOW()),
    ('Admin', 'Administrator role - full access to all settings', NOW()),
    ('Developer', 'Developer role - unrestricted access for development', NOW());
