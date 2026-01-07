-- =============================================
-- Routing Module Seed Data
-- Date: 2026-01-06
-- Purpose: Populate routing module tables with test/sample data
-- Database: mtm_receiving_application (MySQL 5.7.24)
-- =============================================

USE `mtm_receiving_application`;

-- =============================================
-- SEED DATA FOR ROUTING RECIPIENTS
-- =============================================

-- Insert sample recipients (production-ready data)
INSERT INTO routing_recipients (name, location, department, is_active) VALUES
('Engineering - John Smith', 'Building A - Office 101', 'Engineering', 1),
('Production - Quality Control', 'Building B - QC Lab', 'Quality Assurance', 1),
('Maintenance - Tool Crib', 'Building C - Tool Room', 'Maintenance', 1),
('Shipping - Dock Supervisor', 'Building D - Dock 3', 'Shipping & Receiving', 1),
('Purchasing - Sarah Johnson', 'Building A - Office 205', 'Purchasing', 1),
('Production - Assembly Line 1', 'Building B - Line 1', 'Production', 1),
('Production - Assembly Line 2', 'Building B - Line 2', 'Production', 1),
('Engineering - Prototype Lab', 'Building A - Lab 3', 'Engineering', 1),
('Inventory - Stock Room', 'Building C - Stock Room', 'Inventory Management', 1),
('Facilities - Mike Davis', 'Building A - Facilities Office', 'Facilities', 1),
('IT Department - Help Desk', 'Building A - IT Room', 'Information Technology', 1),
('HR - Administrative Office', 'Building A - HR Suite', 'Human Resources', 1),
('Finance - Accounts Payable', 'Building A - Finance Office', 'Finance', 1),
('Sales - Customer Service', 'Building A - Sales Floor', 'Sales', 1),
('R&D - Testing Lab', 'Building E - Lab 5', 'Research & Development', 1)
ON DUPLICATE KEY UPDATE 
    location = VALUES(location),
    department = VALUES(department),
    is_active = VALUES(is_active);

-- =============================================
-- SEED DATA FOR OTHER REASONS
-- =============================================

INSERT INTO routing_other_reasons (reason_code, description, is_active, display_order) VALUES
('SAMPLE', 'Sample/Prototype Material', 1, 1),
('WARRANTY', 'Warranty Return/Replacement', 1, 2),
('REPAIR', 'Equipment Repair Parts', 1, 3),
('OFFICE', 'Office Supplies', 1, 4),
('TOOLS', 'Tools & Equipment', 1, 5),
('CONSUMABLE', 'Shop Consumables', 1, 6),
('MISC', 'Miscellaneous', 1, 99)
ON DUPLICATE KEY UPDATE 
    description = VALUES(description),
    is_active = VALUES(is_active),
    display_order = VALUES(display_order);

-- =============================================
-- SEED DATA FOR USAGE TRACKING (Employee 6229)
-- =============================================

-- Insert sample usage tracking for employee 6229 (for Quick Add feature)
INSERT INTO routing_usage_tracking (employee_number, recipient_id, usage_count, last_used_date)
SELECT 
    6229,
    id,
    CASE 
        WHEN name LIKE '%Tool Crib%' THEN 15
        WHEN name LIKE '%Assembly Line 1%' THEN 12
        WHEN name LIKE '%Quality Control%' THEN 8
        WHEN name LIKE '%Engineering%' THEN 5
        ELSE 2
    END,
    NOW() - INTERVAL FLOOR(RAND() * 30) DAY
FROM routing_recipients
WHERE name IN (
    'Maintenance - Tool Crib',
    'Production - Assembly Line 1',
    'Production - Quality Control',
    'Engineering - John Smith',
    'Inventory - Stock Room'
)
ON DUPLICATE KEY UPDATE 
    usage_count = VALUES(usage_count),
    last_used_date = VALUES(last_used_date);

-- =============================================
-- SEED DATA FOR SAMPLE ROUTING LABELS
-- =============================================

-- Insert sample routing labels (historical data for testing Edit Mode)
INSERT INTO routing_labels (
    po_number, 
    line_number, 
    description, 
    recipient_id, 
    quantity, 
    created_by, 
    created_date,
    csv_exported,
    csv_export_date
)
SELECT 
    CONCAT('PO', LPAD(FLOOR(1000 + RAND() * 9000), 4, '0')),
    LPAD(FLOOR(1 + RAND() * 5), 3, '0'),
    CONCAT('Part-', LPAD(FLOOR(1000 + RAND() * 9000), 4, '0')),
    r.id,
    FLOOR(1 + RAND() * 20),
    6229,
    NOW() - INTERVAL FLOOR(RAND() * 60) DAY,
    1,
    NOW() - INTERVAL FLOOR(RAND() * 60) DAY
FROM routing_recipients r
WHERE r.is_active = 1
LIMIT 25
ON DUPLICATE KEY UPDATE quantity = quantity;

-- Insert sample OTHER workflow labels
INSERT INTO routing_labels (
    po_number, 
    line_number, 
    description, 
    recipient_id, 
    quantity, 
    created_by, 
    created_date,
    other_reason_id,
    csv_exported
)
SELECT 
    'OTHER',
    '000',
    or_reason.description,
    r.id,
    FLOOR(1 + RAND() * 5),
    6229,
    NOW() - INTERVAL FLOOR(RAND() * 30) DAY,
    or_reason.id,
    1
FROM routing_other_reasons or_reason
CROSS JOIN routing_recipients r
WHERE or_reason.reason_code IN ('SAMPLE', 'TOOLS', 'OFFICE')
    AND r.name LIKE '%Tool Crib%'
LIMIT 5
ON DUPLICATE KEY UPDATE quantity = quantity;

-- =============================================
-- SEED DATA FOR USER PREFERENCES
-- =============================================

INSERT INTO routing_user_preferences (employee_number, default_mode, enable_validation)
VALUES (6229, 'WIZARD', 1)
ON DUPLICATE KEY UPDATE 
    default_mode = VALUES(default_mode),
    enable_validation = VALUES(enable_validation);

-- =============================================
-- VERIFICATION QUERIES
-- =============================================

SELECT 'Recipients' AS Table_Name, COUNT(*) AS Record_Count FROM routing_recipients;
SELECT 'Other Reasons' AS Table_Name, COUNT(*) AS Record_Count FROM routing_other_reasons;
SELECT 'Labels' AS Table_Name, COUNT(*) AS Record_Count FROM routing_labels;
SELECT 'Usage Tracking' AS Table_Name, COUNT(*) AS Record_Count FROM routing_usage_tracking;
SELECT 'User Preferences' AS Table_Name, COUNT(*) AS Record_Count FROM routing_user_preferences;
