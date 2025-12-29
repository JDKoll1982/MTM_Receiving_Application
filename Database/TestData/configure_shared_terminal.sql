-- Configure current workstation as shared terminal (for testing PIN login)
-- Run this to test PIN authentication flow

-- First, check your computer name
SELECT 
    @@hostname AS current_computer_name,
    'Run this script to see your computer name' AS note;

-- Insert your computer as a shared terminal
-- REPLACE 'YOUR_COMPUTER_NAME' with the actual computer name from the query above
-- Example: If your computer is DESKTOP-ABC123, use that exact name

-- Uncomment and modify this line after checking computer name:
-- INSERT INTO workstation_config (workstation_name, workstation_type, description)
-- VALUES ('YOUR_COMPUTER_NAME', 'shared_terminal', 'Test shared terminal for PIN login')
-- ON DUPLICATE KEY UPDATE 
--     workstation_type = 'shared_terminal',
--     description = 'Test shared terminal for PIN login';

-- To switch back to personal workstation (Windows auto-login):
-- DELETE FROM workstation_config WHERE workstation_name = 'YOUR_COMPUTER_NAME';

-- View all configured workstations:
SELECT * FROM workstation_config;

-- View all users (to check if your account was created):
SELECT 
    employee_number,
    windows_username,
    full_name,
    pin,
    department,
    shift,
    is_active,
    CASE WHEN visual_username IS NOT NULL THEN 'Yes' ELSE 'No' END AS has_erp_access,
    created_date
FROM users
ORDER BY created_date DESC;
