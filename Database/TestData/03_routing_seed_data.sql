-- =============================================
-- Routing Module Sample/Seed Data
-- Feature: 001-routing-module
-- Date: 2026-01-04
-- Purpose: Initial test data for routing module
-- =============================================

-- Seed Other Reasons (enumeration)
INSERT IGNORE INTO routing_other_reasons (reason_code, description, display_order) VALUES
('RETURNED', 'Returned Item', 1),
('SAMPLE', 'Vendor Sample', 2),
('MISLABELED', 'Mislabeled Package', 3),
('CUSTOMER_RETURN', 'Customer Return', 4),
('INTERNAL_TRANSFER', 'Internal Transfer', 5);

-- Sample Recipients (for testing)
INSERT IGNORE INTO routing_recipients (name, location, department, is_active) VALUES
-- Production Departments
('Production - Assembly', 'Building A - Floor 1', 'Production', 1),
('Production - Machining', 'Building A - Floor 2', 'Production', 1),
('Production - Quality Control', 'Building B - Floor 1', 'Quality', 1),

-- Engineering Departments
('Engineering - Design', 'Building C - Floor 2', 'Engineering', 1),
('Engineering - Testing', 'Building C - Floor 1', 'Engineering', 1),

-- Support Departments
('Shipping & Receiving', 'Building A - Dock', 'Logistics', 1),
('Warehouse - Raw Materials', 'Building D - Floor 1', 'Warehouse', 1),
('Warehouse - Finished Goods', 'Building D - Floor 2', 'Warehouse', 1),
('Purchasing', 'Building B - Floor 2', 'Administration', 1),
('Maintenance', 'Building A - Floor 1', 'Facilities', 1),

-- Individual Recipients (examples)
('John Smith - Supervisor', 'Building A - Office 101', 'Production', 1),
('Jane Doe - Manager', 'Building C - Office 201', 'Engineering', 1),
('Bob Wilson - Lead', 'Building A - Floor 2', 'Production', 1),
('Sarah Johnson - Inspector', 'Building B - Floor 1', 'Quality', 1),
('Mike Davis - Technician', 'Building C - Lab 3', 'Engineering', 1);

-- Note: routing_labels, routing_usage_tracking, routing_user_preferences, and routing_label_history
-- will be populated by user activity, not seed data
