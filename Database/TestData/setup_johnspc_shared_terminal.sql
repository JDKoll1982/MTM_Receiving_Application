-- Configure JOHNSPC as shared terminal for PIN login testing
USE mtm_receiving_application;

-- Insert or update workstation configuration
INSERT INTO workstation_config (workstation_name, workstation_type, description)
VALUES ('JOHNSPC', 'shared_terminal', 'Development workstation - PIN testing')
ON DUPLICATE KEY UPDATE 
    workstation_type = 'shared_terminal',
    description = 'Development workstation - PIN testing';

-- Verify the configuration
SELECT 
    workstation_name,
    workstation_type,
    description,
    created_date
FROM workstation_config;

-- Show your user account
SELECT 
    employee_number,
    windows_username,
    full_name,
    pin,
    department,
    shift,
    is_active
FROM users
WHERE windows_username = 'johnk'
ORDER BY created_date DESC
LIMIT 1;
