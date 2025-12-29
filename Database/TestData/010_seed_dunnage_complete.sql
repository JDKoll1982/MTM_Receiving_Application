-- =============================================
-- Dunnage Receiving System - Seed Data
-- Feature: 010-dunnage-complete
-- Description: Populates dunnage types, specs, parts, and preferences for testing
-- =============================================

USE mtm_receiving_application;

-- =============================================
-- 1. Seed Dunnage Types
-- =============================================
INSERT IGNORE INTO dunnage_types (type_name, icon, created_by, created_date) VALUES
('Pallet', '&#xE8B9;', 'SYSTEM', NOW()),
('Crate', '&#xE7B8;', 'SYSTEM', NOW()),
('Box', '&#xE7B8;', 'SYSTEM', NOW()),
('Skid', '&#xE8B9;', 'SYSTEM', NOW()),
('Foam', '&#xE945;', 'SYSTEM', NOW()),
('Shrink Wrap', '&#xE7B8;', 'SYSTEM', NOW()),
('Bubble Wrap', '&#xE7B8;', 'SYSTEM', NOW()),
('Gaylord', '&#xE7B8;', 'SYSTEM', NOW()),
('Foldable Crate', '&#xE7B8;', 'SYSTEM', NOW()),
('Wooden Crate', '&#xE7B8;', 'SYSTEM', NOW()),
('Plastic Totes', '&#xE7B8;', 'SYSTEM', NOW());

-- =============================================
-- 2. Seed Default Specification Schemas
-- =============================================
-- Width, Height, Depth for all types
INSERT IGNORE INTO dunnage_specs (type_id, spec_key, spec_value, created_by, created_date)
SELECT 
    id, 
    'Width', 
    JSON_OBJECT('type', 'number', 'unit', 'inches', 'required', true), 
    'SYSTEM', 
    NOW()
FROM dunnage_types;

INSERT IGNORE INTO dunnage_specs (type_id, spec_key, spec_value, created_by, created_date)
SELECT 
    id, 
    'Height', 
    JSON_OBJECT('type', 'number', 'unit', 'inches', 'required', true), 
    'SYSTEM', 
    NOW()
FROM dunnage_types;

INSERT IGNORE INTO dunnage_specs (type_id, spec_key, spec_value, created_by, created_date)
SELECT 
    id, 
    'Depth', 
    JSON_OBJECT('type', 'number', 'unit', 'inches', 'required', true), 
    'SYSTEM', 
    NOW()
FROM dunnage_types;

-- =============================================
-- 3. Seed Custom Field Definitions
-- =============================================
-- Add 'Material' and 'Color' to Pallet
SET @pallet_id = (SELECT id FROM dunnage_types WHERE type_name = 'Pallet');

INSERT IGNORE INTO custom_field_definitions (DunnageTypeID, FieldName, DatabaseColumnName, FieldType, DisplayOrder, IsRequired, CreatedBy) VALUES
(@pallet_id, 'Material', 'material', 'Text', 1, TRUE, 'SYSTEM'),
(@pallet_id, 'Color', 'color', 'Text', 2, FALSE, 'SYSTEM');

-- =============================================
-- 4. Seed Dunnage Parts
-- =============================================
INSERT IGNORE INTO dunnage_parts (part_id, type_id, spec_values, created_by, created_date) VALUES
('PALLET-48X40', @pallet_id, JSON_OBJECT('Width', 48, 'Height', 40, 'Depth', 6, 'material', 'Wood', 'color', 'Natural'), 'SYSTEM', NOW()),
('PALLET-60X48', @pallet_id, JSON_OBJECT('Width', 60, 'Height', 48, 'Depth', 6, 'material', 'Wood', 'color', 'Natural'), 'SYSTEM', NOW());

SET @box_id = (SELECT id FROM dunnage_types WHERE type_name = 'Box');
INSERT IGNORE INTO dunnage_parts (part_id, type_id, spec_values, created_by, created_date) VALUES
('BOX-SMALL', @box_id, JSON_OBJECT('Width', 12, 'Height', 12, 'Depth', 12), 'SYSTEM', NOW());

SET @crate_id = (SELECT id FROM dunnage_types WHERE type_name = 'Crate');
INSERT IGNORE INTO dunnage_parts (part_id, type_id, spec_values, created_by, created_date) VALUES
('CRATE-LARGE', @crate_id, JSON_OBJECT('Width', 48, 'Height', 48, 'Depth', 48), 'SYSTEM', NOW());

-- =============================================
-- 5. Seed Inventoried Dunnage
-- =============================================
INSERT IGNORE INTO inventoried_dunnage (part_id, inventory_method, notes, created_by, created_date) VALUES
('PALLET-48X40', 'Both', 'Standard pallet tracking for Visual ERP', 'SYSTEM', NOW());

-- =============================================
-- 6. Seed User Preferences
-- =============================================
-- Seed some recently used icons for 'johnk'
INSERT IGNORE INTO user_preferences (UserId, PreferenceKey, PreferenceValue) VALUES
('johnk', 'recent_icons', '["&#xE8B9;", "&#xE7B8;", "&#xE945;"]');

-- =============================================
-- SEEDING COMPLETE
-- =============================================
SELECT 'Dunnage seed data populated successfully' AS Status;
