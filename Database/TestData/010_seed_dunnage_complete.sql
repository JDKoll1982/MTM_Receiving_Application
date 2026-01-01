-- =============================================
-- Dunnage Receiving System - Seed Data
-- Feature: 010-dunnage-complete
-- Description: Populates dunnage types, specs, parts, and preferences for testing
-- =============================================

USE mtm_receiving_application;

-- =============================================
-- 1. Seed Dunnage Types (Ensuring names match database exactly)
-- =============================================
INSERT IGNORE INTO dunnage_types (type_name, icon, created_by, created_date) VALUES
('Pallet', 'Pallet', 'SYSTEM', NOW()),
('Crate', 'CubeOutline', 'SYSTEM', NOW()),
('Box', 'PackageVariantClosed', 'SYSTEM', NOW()),
('Skid', 'ForkliftBox', 'SYSTEM', NOW()),
('Foam', 'Texture', 'SYSTEM', NOW()),
('ShrinkWrap', 'Wrap', 'SYSTEM', NOW()),
('BubbleWrap', 'BubbleSheet', 'SYSTEM', NOW()),
('Gaylord', 'Archive', 'SYSTEM', NOW()),
('FoldableCrate', 'PackageVariant', 'SYSTEM', NOW()),
('Wooden Crate', 'WoodBoard', 'SYSTEM', NOW()),
('PlasticTotes', 'ShoppingOutline', 'SYSTEM', NOW());

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
-- 4. Seed Dunnage Parts (5 parts per type)
-- =============================================

-- Pallet (ID 1)
INSERT IGNORE INTO dunnage_parts (part_id, type_id, spec_values, home_location, created_by, created_date) VALUES
('PALLET-48X40', @pallet_id, JSON_OBJECT('Width', 48, 'Height', 40, 'Depth', 6, 'material', 'Wood', 'color', 'Natural'), 'Warehouse Aisle A', 'SYSTEM', NOW()),
('PALLET-60X48', @pallet_id, JSON_OBJECT('Width', 60, 'Height', 48, 'Depth', 6, 'material', 'Wood', 'color', 'Natural'), 'Warehouse Aisle A', 'SYSTEM', NOW()),
('PALLET-PLASTIC-48X40', @pallet_id, JSON_OBJECT('Width', 48, 'Height', 40, 'Depth', 5, 'material', 'Plastic', 'color', 'Black'), 'Warehouse Aisle B', 'SYSTEM', NOW()),
('PALLET-EURO-1200X800', @pallet_id, JSON_OBJECT('Width', 47.2, 'Height', 31.5, 'Depth', 5.7, 'material', 'Wood', 'color', 'Natural'), 'Warehouse Aisle A', 'SYSTEM', NOW()),
('PALLET-HALF-24X24', @pallet_id, JSON_OBJECT('Width', 24, 'Height', 24, 'Depth', 6, 'material', 'Wood', 'color', 'Natural'), 'Warehouse Aisle A', 'SYSTEM', NOW());

-- Crate (ID 2)
SET @crate_id = (SELECT id FROM dunnage_types WHERE type_name = 'Crate');
INSERT IGNORE INTO dunnage_parts (part_id, type_id, spec_values, home_location, created_by, created_date) VALUES
('CRATE-L-48X48', @crate_id, JSON_OBJECT('Width', 48, 'Height', 48, 'Depth', 48), 'Storage Room 1', 'SYSTEM', NOW()),
('CRATE-M-36X36', @crate_id, JSON_OBJECT('Width', 36, 'Height', 36, 'Depth', 36), 'Storage Room 1', 'SYSTEM', NOW()),
('CRATE-S-24X24', @crate_id, JSON_OBJECT('Width', 24, 'Height', 24, 'Depth', 24), 'Storage Room 1', 'SYSTEM', NOW()),
('CRATE-XL-60X60', @crate_id, JSON_OBJECT('Width', 60, 'Height', 60, 'Depth', 60), 'Storage Room 1', 'SYSTEM', NOW()),
('CRATE-CUSTOM-40X30', @crate_id, JSON_OBJECT('Width', 40, 'Height', 30, 'Depth', 20), 'Storage Room 1', 'SYSTEM', NOW());

-- Box (ID 3)
SET @box_id = (SELECT id FROM dunnage_types WHERE type_name = 'Box');
INSERT IGNORE INTO dunnage_parts (part_id, type_id, spec_values, home_location, created_by, created_date) VALUES
('BOX-S-12X12', @box_id, JSON_OBJECT('Width', 12, 'Height', 12, 'Depth', 12), 'Receiving Dock', 'SYSTEM', NOW()),
('BOX-M-18X18', @box_id, JSON_OBJECT('Width', 18, 'Height', 18, 'Depth', 18), 'Receiving Dock', 'SYSTEM', NOW()),
('BOX-L-24X24', @box_id, JSON_OBJECT('Width', 24, 'Height', 24, 'Depth', 24), 'Receiving Dock', 'SYSTEM', NOW()),
('BOX-XL-36X24', @box_id, JSON_OBJECT('Width', 36, 'Height', 24, 'Depth', 24), 'Receiving Dock', 'SYSTEM', NOW()),
('BOX-FLAT-20X20', @box_id, JSON_OBJECT('Width', 20, 'Height', 20, 'Depth', 4), 'Receiving Dock', 'SYSTEM', NOW());

-- Skid (ID 4)
SET @skid_id = (SELECT id FROM dunnage_types WHERE type_name = 'Skid');
INSERT IGNORE INTO dunnage_parts (part_id, type_id, spec_values, home_location, created_by, created_date) VALUES
('SKID-48X40', @skid_id, JSON_OBJECT('Width', 48, 'Height', 40, 'Depth', 4), 'Warehouse Aisle C', 'SYSTEM', NOW()),
('SKID-36X36', @skid_id, JSON_OBJECT('Width', 36, 'Height', 36, 'Depth', 4), 'Warehouse Aisle C', 'SYSTEM', NOW()),
('SKID-60X48', @skid_id, JSON_OBJECT('Width', 60, 'Height', 48, 'Depth', 4), 'Warehouse Aisle C', 'SYSTEM', NOW()),
('SKID-HEAVY-48X48', @skid_id, JSON_OBJECT('Width', 48, 'Height', 48, 'Depth', 5), 'Warehouse Aisle C', 'SYSTEM', NOW()),
('SKID-NARROW-48X24', @skid_id, JSON_OBJECT('Width', 48, 'Height', 24, 'Depth', 4), 'Warehouse Aisle C', 'SYSTEM', NOW());

-- Foam (ID 5)
SET @foam_id = (SELECT id FROM dunnage_types WHERE type_name = 'Foam');
INSERT IGNORE INTO dunnage_parts (part_id, type_id, spec_values, home_location, created_by, created_date) VALUES
('FOAM-BLOCK-12X12', @foam_id, JSON_OBJECT('Width', 12, 'Height', 12, 'Depth', 12), 'Storage Room 2', 'SYSTEM', NOW()),
('FOAM-SHEET-48X48', @foam_id, JSON_OBJECT('Width', 48, 'Height', 48, 'Depth', 1), 'Storage Room 2', 'SYSTEM', NOW()),
('FOAM-CORNER-4X4', @foam_id, JSON_OBJECT('Width', 4, 'Height', 4, 'Depth', 4), 'Storage Room 2', 'SYSTEM', NOW()),
('FOAM-PLANK-24X4', @foam_id, JSON_OBJECT('Width', 24, 'Height', 4, 'Depth', 2), 'Storage Room 2', 'SYSTEM', NOW()),
('FOAM-CUSTOM-20X10', @foam_id, JSON_OBJECT('Width', 20, 'Height', 10, 'Depth', 5), 'Storage Room 2', 'SYSTEM', NOW());

-- ShrinkWrap (ID 6)
SET @shrink_id = (SELECT id FROM dunnage_types WHERE type_name = 'ShrinkWrap');
INSERT IGNORE INTO dunnage_parts (part_id, type_id, spec_values, home_location, created_by, created_date) VALUES
('SHRINK-80GA-18IN', @shrink_id, JSON_OBJECT('Width', 18, 'Height', 1500, 'Depth', 0.01), 'Packaging Area', 'SYSTEM', NOW()),
('SHRINK-60GA-12IN', @shrink_id, JSON_OBJECT('Width', 12, 'Height', 2000, 'Depth', 0.01), 'Packaging Area', 'SYSTEM', NOW()),
('SHRINK-100GA-20IN', @shrink_id, JSON_OBJECT('Width', 20, 'Height', 1000, 'Depth', 0.01), 'Packaging Area', 'SYSTEM', NOW()),
('SHRINK-BLACK-18IN', @shrink_id, JSON_OBJECT('Width', 18, 'Height', 1500, 'Depth', 0.01), 'Packaging Area', 'SYSTEM', NOW()),
('SHRINK-EXT-5IN', @shrink_id, JSON_OBJECT('Width', 5, 'Height', 1000, 'Depth', 0.01), 'Packaging Area', 'SYSTEM', NOW());

-- BubbleWrap (ID 7)
SET @bubble_id = (SELECT id FROM dunnage_types WHERE type_name = 'BubbleWrap');
INSERT IGNORE INTO dunnage_parts (part_id, type_id, spec_values, home_location, created_by, created_date) VALUES
('BUBBLE-S-12IN', @bubble_id, JSON_OBJECT('Width', 12, 'Height', 1200, 'Depth', 0.18), 'Packaging Area', 'SYSTEM', NOW()),
('BUBBLE-M-24IN', @bubble_id, JSON_OBJECT('Width', 24, 'Height', 600, 'Depth', 0.31), 'Packaging Area', 'SYSTEM', NOW()),
('BUBBLE-L-48IN', @bubble_id, JSON_OBJECT('Width', 48, 'Height', 300, 'Depth', 0.5), 'Packaging Area', 'SYSTEM', NOW()),
('BUBBLE-ANTI-12IN', @bubble_id, JSON_OBJECT('Width', 12, 'Height', 1200, 'Depth', 0.18), 'Packaging Area', 'SYSTEM', NOW()),
('BUBBLE-PERF-12IN', @bubble_id, JSON_OBJECT('Width', 12, 'Height', 1200, 'Depth', 0.18), 'Packaging Area', 'SYSTEM', NOW());

-- Gaylord (ID 8)
SET @gaylord_id = (SELECT id FROM dunnage_types WHERE type_name = 'Gaylord');
INSERT IGNORE INTO dunnage_parts (part_id, type_id, spec_values, home_location, created_by, created_date) VALUES
('GAYLORD-STD-48X40', @gaylord_id, JSON_OBJECT('Width', 48, 'Height', 40, 'Depth', 36), 'Warehouse Aisle D', 'SYSTEM', NOW()),
('GAYLORD-TALL-48X40', @gaylord_id, JSON_OBJECT('Width', 48, 'Height', 40, 'Depth', 48), 'Warehouse Aisle D', 'SYSTEM', NOW()),
('GAYLORD-OCT-48X48', @gaylord_id, JSON_OBJECT('Width', 48, 'Height', 48, 'Depth', 40), 'Warehouse Aisle D', 'SYSTEM', NOW()),
('GAYLORD-HEAVY-48X40', @gaylord_id, JSON_OBJECT('Width', 48, 'Height', 40, 'Depth', 36), 'Warehouse Aisle D', 'SYSTEM', NOW()),
('GAYLORD-SMALL-36X36', @gaylord_id, JSON_OBJECT('Width', 36, 'Height', 36, 'Depth', 30), 'Warehouse Aisle D', 'SYSTEM', NOW());

-- FoldableCrate (ID 9)
SET @fold_id = (SELECT id FROM dunnage_types WHERE type_name = 'FoldableCrate');
INSERT IGNORE INTO dunnage_parts (part_id, type_id, spec_values, home_location, created_by, created_date) VALUES
('FOLD-CRATE-600X400', @fold_id, JSON_OBJECT('Width', 23.6, 'Height', 15.7, 'Depth', 11.8), 'Storage Room 1', 'SYSTEM', NOW()),
('FOLD-CRATE-400X300', @fold_id, JSON_OBJECT('Width', 15.7, 'Height', 11.8, 'Depth', 9.8), 'Storage Room 1', 'SYSTEM', NOW()),
('FOLD-CRATE-LARGE', @fold_id, JSON_OBJECT('Width', 31.5, 'Height', 23.6, 'Depth', 17.7), 'Storage Room 1', 'SYSTEM', NOW()),
('FOLD-CRATE-VENT', @fold_id, JSON_OBJECT('Width', 23.6, 'Height', 15.7, 'Depth', 11.8), 'Storage Room 1', 'SYSTEM', NOW()),
('FOLD-CRATE-HEAVY', @fold_id, JSON_OBJECT('Width', 23.6, 'Height', 15.7, 'Depth', 11.8), 'Storage Room 1', 'SYSTEM', NOW());

-- Wooden Crate (ID 10)
SET @wood_crate_id = (SELECT id FROM dunnage_types WHERE type_name = 'Wooden Crate');
INSERT IGNORE INTO dunnage_parts (part_id, type_id, spec_values, home_location, created_by, created_date) VALUES
('WOOD-CRATE-48X48', @wood_crate_id, JSON_OBJECT('Width', 48, 'Height', 48, 'Depth', 48), 'Storage Room 1', 'SYSTEM', NOW()),
('WOOD-CRATE-36X36', @wood_crate_id, JSON_OBJECT('Width', 36, 'Height', 36, 'Depth', 36), 'Storage Room 1', 'SYSTEM', NOW()),
('WOOD-CRATE-24X24', @wood_crate_id, JSON_OBJECT('Width', 24, 'Height', 24, 'Depth', 24), 'Storage Room 1', 'SYSTEM', NOW()),
('WOOD-CRATE-LONG-72X24', @wood_crate_id, JSON_OBJECT('Width', 72, 'Height', 24, 'Depth', 24), 'Storage Room 1', 'SYSTEM', NOW()),
('WOOD-CRATE-CUSTOM', @wood_crate_id, JSON_OBJECT('Width', 40, 'Height', 40, 'Depth', 40), 'Storage Room 1', 'SYSTEM', NOW());

-- PlasticTotes (ID 11)
SET @tote_id = (SELECT id FROM dunnage_types WHERE type_name = 'PlasticTotes');
INSERT IGNORE INTO dunnage_parts (part_id, type_id, spec_values, home_location, created_by, created_date) VALUES
('TOTE-BLUE-24X15', @tote_id, JSON_OBJECT('Width', 24, 'Height', 15, 'Depth', 12), 'Receiving Dock', 'SYSTEM', NOW()),
('TOTE-RED-24X15', @tote_id, JSON_OBJECT('Width', 24, 'Height', 15, 'Depth', 12), 'Receiving Dock', 'SYSTEM', NOW()),
('TOTE-GRAY-18X12', @tote_id, JSON_OBJECT('Width', 18, 'Height', 12, 'Depth', 10), 'Receiving Dock', 'SYSTEM', NOW()),
('TOTE-LARGE-30X20', @tote_id, JSON_OBJECT('Width', 30, 'Height', 20, 'Depth', 15), 'Receiving Dock', 'SYSTEM', NOW()),
('TOTE-DIVIDER-24X15', @tote_id, JSON_OBJECT('Width', 24, 'Height', 15, 'Depth', 12), 'Receiving Dock', 'SYSTEM', NOW());

-- =============================================
-- 5. Seed Inventoried Dunnage
-- =============================================
INSERT IGNORE INTO inventoried_dunnage (part_id, inventory_method, notes, created_by, created_date) VALUES
('PALLET-48X40', 'Both', 'Standard pallet tracking for Visual ERP', 'SYSTEM', NOW()),
('CRATE-L-48X48', 'Visual', 'Large crate tracking', 'SYSTEM', NOW()),
('TOTE-BLUE-24X15', 'Manual', 'Plastic tote tracking', 'SYSTEM', NOW());

-- =============================================
-- 6. Seed User Preferences
-- =============================================
-- Seed some recently used icons for 'johnk'
INSERT IGNORE INTO user_preferences (UserId, PreferenceKey, PreferenceValue) VALUES
('johnk', 'recent_icons', '["Pallet", "PackageVariantClosed", "CubeOutline", "Wrap", "Archive", "BubbleSheet"]');

-- =============================================
-- SEEDING COMPLETE
-- =============================================
SELECT 'Dunnage seed data populated successfully' AS Status;
