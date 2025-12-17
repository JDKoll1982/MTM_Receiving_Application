-- Insert Test Data for MTM Receiving Application
-- This creates initial user and configuration data for testing

USE mtm_receiving_application;

-- Insert test departments
INSERT INTO departments (department_name, is_active, sort_order) VALUES
('Production', TRUE, 1),
('Quality', TRUE, 2),
('Shipping', TRUE, 3),
('Maintenance', TRUE, 4)
ON DUPLICATE KEY UPDATE is_active = VALUES(is_active);

-- Workstation configurations already created in schema - skip

-- Insert test user (John Kollman - matches Windows username)
INSERT INTO users (
    employee_number,
    windows_username,
    full_name,
    pin,
    department,
    shift,
    is_active
) VALUES (
    6229,
    'johnk',  -- Your Windows username
    'John Kollman',
    '1234',   -- Test PIN
    'Production',
    '1st Shift',
    TRUE
) ON DUPLICATE KEY UPDATE 
    windows_username = VALUES(windows_username),
    is_active = VALUES(is_active);

-- Insert additional test users
INSERT INTO users (
    employee_number,
    windows_username,
    full_name,
    pin,
    department,
    shift,
    is_active
) VALUES 
(6230, 'jsmith', 'Jane Smith', '5678', 'Production', '1st Shift', TRUE),
(6231, 'bjohnson', 'Bob Johnson', '9012', 'Quality', '2nd Shift', TRUE),
(6232, 'awilliams', 'Alice Williams', '3456', 'Shipping', '1st Shift', TRUE)
ON DUPLICATE KEY UPDATE 
    is_active = VALUES(is_active);

SELECT 'Test data inserted successfully' AS Status;

-- Display created users
SELECT 
    employee_number,
    full_name,
    windows_username,
    pin,
    department,
    shift,
    is_active
FROM users
ORDER BY employee_number;
