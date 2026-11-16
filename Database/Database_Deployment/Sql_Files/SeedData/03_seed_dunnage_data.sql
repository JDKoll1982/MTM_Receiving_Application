-- =============================================
-- Seed: Dunnage Master Data
-- Created  : 2026-03-19
-- Updated  : 2026-11-16
-- Source   : captured from the live database (mtm_receiving_application) on 2026-11-16
-- =============================================
-- Inserts (counts are what the live database holds):
--   Step 1 — dunnage_types                          24 rows
--   Step 2 — dunnage_custom_fields                  44 rows (+32 skipped, see below)
--   Step 3 — dunnage_custom_field_choices           21 rows (+16 skipped, see below)
--   Step 4 — dunnage_parts                          158 rows
--   Step 5 — dunnage_requires_inventory             18 rows (+10 skipped, see below)
--   Step 6 — dunnage_non_po_entries                 14 rows
--   Step 7 — dunnage_no_label_parts                 8 rows
--   Step 8 — dunnage_non_po_part_defaults           49 rows (+8 skipped, see below)
--   Note   — dunnage_quantity_types is seeded by schema file
--            40_Table_dunnage_quantity_types.sql
--
-- Contract: re-runnable. Every statement resolves its parent by natural key (type name, part
-- id, field name) and is guarded by NOT EXISTS, so a second run inserts no rows, touches no
-- timestamp, and does not bump dunnage_non_po_entries.use_count.
--
-- Left out on purpose — rows the live database holds that a seed cannot create:
--   * 32 dunnage_custom_fields rows whose DunnageTypeID points at a type that no
--     longer exists (types 4, 25, 26, 27, 28, 29, 30, 31, 34, 35, 37), plus the
--     16 choices that hang off them. The app cannot reach them either: its type
--     list comes from dunnage_types. They are named here so the omission is not mistaken for a
--     capture error. See the Change-Log entry for the cleanup query.
--   * 10 dunnage_requires_inventory rows and 8 dunnage_non_po_part_defaults rows
--     whose part_id has no dunnage_parts row. Seeding them would break the foreign key on a
--     fresh install, so they are only meaningful in the database that already has them.
-- =============================================

START TRANSACTION;

-- =============================================
-- Step 1 — dunnage_types
-- One row per dunnage type the app offers, with its icon and image.
-- =============================================
INSERT INTO dunnage_types (type_name, icon, image_path, created_by, created_date)
SELECT v.type_name, v.icon, v.image_path, 'seed', NOW()
FROM (
        SELECT 'Metal Dunnage' AS type_name, 'TrainCarContainer' AS icon, NULL AS image_path
        UNION ALL SELECT 'Plastic Dunnage', 'WindowOpenVariant', NULL
        UNION ALL SELECT 'Wooden Dunnage', 'PackageVariantClosed', NULL
        UNION ALL SELECT 'Cardboard Sheets', 'ShippingPallet', 'Types/DunnageType-Cardboard.png'
        UNION ALL SELECT 'Corrugated Boxes', 'PackageVariantClosed', 'Types/DunnageType-Boxes.jpg'
        UNION ALL SELECT 'Wire Baskets', 'PackageVariantClosed', NULL
        UNION ALL SELECT 'Gaylords / Bulk Bins', 'PackageVariantClosed', NULL
        UNION ALL SELECT 'Banding Supplies', 'PackageVariantClosed', 'Types/DunnageType-Banding Supplies.jpg'
        UNION ALL SELECT 'Oils & Lubricants', 'PackageVariantClosed', NULL
        UNION ALL SELECT 'Bags', 'BagPersonal', NULL
        UNION ALL SELECT 'Stretch Film / Shrink Wrap', 'PackageVariantClosed', NULL
        UNION ALL SELECT 'Pallets / Skids', 'PackageVariantClosed', 'Types/DunnageType-Skids.png'
        UNION ALL SELECT 'Office Supplies', 'PackageVariantClosed', NULL
        UNION ALL SELECT 'Janitor Closet', 'HardHat', NULL
        UNION ALL SELECT 'Vendor Supplied Filters', 'AirFilter', NULL
        UNION ALL SELECT 'Raffle', 'PackageVariantClosed', 'Types/DunnageType-Raffle.jpg'
        UNION ALL SELECT 'Trial Material', 'PackageVariantClosed', NULL
        UNION ALL SELECT 'Fabrication Area', 'PackageVariantClosed', NULL
        UNION ALL SELECT 'U-Line', 'PackageVariantClosed', 'Types/DunnageType-U-Line.png'
        UNION ALL SELECT 'Special', 'MotherHeart', NULL
        UNION ALL SELECT 'Quality Receiving', 'MagnifyExpand', NULL
        UNION ALL SELECT 'Edge Protectors', 'ShieldOutline', NULL
        UNION ALL SELECT 'Foam / Molded Inserts', 'LayersOutline', NULL
        UNION ALL SELECT 'Returnable Totes', 'BoxVariantClosed', NULL
) AS v
WHERE NOT EXISTS (
    SELECT 1
    FROM dunnage_types t
    WHERE t.type_name = v.type_name
);

-- =============================================
-- Step 2 — dunnage_custom_fields
-- The UDC slot definitions each type shows when a part is added or edited.
-- `Unit`, `MinValue`, `MaxValue` are backticked because MAXVALUE is a MySQL keyword.
-- =============================================
INSERT INTO dunnage_custom_fields (
    DunnageTypeID,
    FieldName,
    FieldType,
    DisplayOrder,
    IsRequired,
    `Unit`,
    `MinValue`,
    `MaxValue`,
    `DefaultValue`,
    CreatedBy
)
SELECT t.id, v.field_name, v.field_type, v.display_order, v.is_required, v.`unit`, v.min_value, v.max_value, v.default_value, 'seed'
FROM (
        SELECT 'Metal Dunnage' AS type_name, 'Customer' AS field_name, 'Text' AS field_type, 1 AS display_order, 0 AS is_required, NULL AS `unit`, NULL AS min_value, NULL AS max_value, NULL AS default_value
        UNION ALL SELECT 'Metal Dunnage', 'Owner', 'Choices', 2, 0, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Metal Dunnage', 'Style', 'Text', 3, 0, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Plastic Dunnage', 'Length', 'Number', 1, 0, 'Inches', 1.0000, 999.0000, NULL
        UNION ALL SELECT 'Plastic Dunnage', 'Width', 'Number', 2, 0, 'Inches', 1.0000, 999.0000, NULL
        UNION ALL SELECT 'Plastic Dunnage', 'Depth', 'Number', 3, 0, 'Inches', 1.0000, 999.0000, NULL
        UNION ALL SELECT 'Plastic Dunnage', 'Customer', 'Text', 4, 0, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Wooden Dunnage', 'Size', 'Choices', 1, 0, NULL, NULL, NULL, 'Short'
        UNION ALL SELECT 'Cardboard Sheets', 'Length', 'Number', 1, 0, 'Inches', 1.0000, 999.0000, NULL
        UNION ALL SELECT 'Cardboard Sheets', 'Width', 'Number', 2, 0, 'Inches', 1.0000, 999.0000, NULL
        UNION ALL SELECT 'Corrugated Boxes', 'Length', 'Number', 1, 0, 'Inches', 1.0000, 999.0000, NULL
        UNION ALL SELECT 'Corrugated Boxes', 'Width', 'Number', 2, 0, 'Inches', 1.0000, 999.0000, NULL
        UNION ALL SELECT 'Corrugated Boxes', 'Wall Thickness', 'Choices', 3, 0, NULL, NULL, NULL, 'Single Walled'
        UNION ALL SELECT 'Corrugated Boxes', 'Height', 'Number', 4, 0, 'Inches', 1.0000, 999.0000, NULL
        UNION ALL SELECT 'Wire Baskets', 'Customer', 'Text', 1, 0, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Gaylords / Bulk Bins', 'Size', 'Choices', 1, 0, NULL, NULL, NULL, 'Short'
        UNION ALL SELECT 'Banding Supplies', 'Type', 'Choices', 1, 0, NULL, NULL, NULL, 'Nylon'
        UNION ALL SELECT 'Oils & Lubricants', 'Quantity Type', 'Choices', 1, 0, NULL, NULL, NULL, 'Gallons'
        UNION ALL SELECT 'Oils & Lubricants', 'Quantity Amount', 'Number', 2, 0, NULL, 1.0000, 999.0000, NULL
        UNION ALL SELECT 'Bags', 'Length', 'Number', 1, 0, 'Inches', 1.0000, 999.0000, NULL
        UNION ALL SELECT 'Bags', 'Width', 'Number', 2, 0, 'Inches', 1.0000, 999.0000, NULL
        UNION ALL SELECT 'Bags', 'Height', 'Number', 3, 0, 'Inches', 1.0000, 999.0000, NULL
        UNION ALL SELECT 'Bags', 'Style', 'Choices', 4, 0, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Stretch Film / Shrink Wrap', 'Roll Length', 'Number', 1, 0, 'Feet', 1.0000, 9999.0000, '1'
        UNION ALL SELECT 'Stretch Film / Shrink Wrap', 'Height', 'Number', 2, 0, 'Inches', 1.0000, 99.0000, '1'
        UNION ALL SELECT 'Stretch Film / Shrink Wrap', 'Thickness', 'Number', 3, 0, 'Gauge', 1.0000, 999.0000, '1'
        UNION ALL SELECT 'Pallets / Skids', 'Length', 'Number', 1, 0, 'Inches', 1.0000, 999.0000, NULL
        UNION ALL SELECT 'Pallets / Skids', 'Width', 'Number', 2, 0, 'Inches', 1.0000, 999.0000, NULL
        UNION ALL SELECT 'Vendor Supplied Filters', 'Department', 'Choices', 1, 1, NULL, NULL, NULL, 'Maintenance'
        UNION ALL SELECT 'Trial Material', 'Order Number', 'Text', 1, 0, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Trial Material', 'Lot Number', 'Text', 2, 1, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'U-Line', 'U-Line Part Number', 'Text', 1, 0, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'U-Line', 'Raffle Item', 'Boolean', 2, 0, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Edge Protectors', 'Width', 'Number', 1, 0, 'in', NULL, NULL, NULL
        UNION ALL SELECT 'Edge Protectors', 'Depth', 'Number', 2, 0, 'in', NULL, NULL, NULL
        UNION ALL SELECT 'Edge Protectors', 'Length', 'Number', 3, 0, 'in', NULL, NULL, NULL
        UNION ALL SELECT 'Foam / Molded Inserts', 'Type', 'Text', 1, 0, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Foam / Molded Inserts', 'Variant', 'Number', 2, 0, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Foam / Molded Inserts', 'Part Family', 'Text', 3, 0, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Foam / Molded Inserts', 'Piece', 'Choices', 4, 0, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Returnable Totes', 'Length', 'Number', 1, 0, 'in', NULL, NULL, NULL
        UNION ALL SELECT 'Returnable Totes', 'Width', 'Number', 2, 0, 'in', NULL, NULL, NULL
        UNION ALL SELECT 'Returnable Totes', 'Height', 'Number', 3, 0, 'in', NULL, NULL, NULL
        UNION ALL SELECT 'Returnable Totes', 'Customer', 'Text', 4, 0, NULL, NULL, NULL, NULL
) AS v
JOIN dunnage_types t ON t.type_name = v.type_name
WHERE NOT EXISTS (
    SELECT 1
    FROM dunnage_custom_fields f
    WHERE f.DunnageTypeID = t.id
      AND f.FieldName = v.field_name
);

-- =============================================
-- Step 3 — dunnage_custom_field_choices
-- The pick lists behind every Choices field.
-- =============================================
INSERT INTO dunnage_custom_field_choices (CustomFieldID, Choice, SortOrder)
SELECT f.ID, v.choice, v.sort_order
FROM (
        SELECT 'Wooden Dunnage' AS type_name, 'Size' AS field_name, 'Short' AS choice, 1 AS sort_order
        UNION ALL SELECT 'Wooden Dunnage', 'Size', 'Tall', 2
        UNION ALL SELECT 'Corrugated Boxes', 'Wall Thickness', 'Single Walled', 1
        UNION ALL SELECT 'Corrugated Boxes', 'Wall Thickness', 'Double Walled', 2
        UNION ALL SELECT 'Gaylords / Bulk Bins', 'Size', 'Short', 1
        UNION ALL SELECT 'Gaylords / Bulk Bins', 'Size', 'Tall', 2
        UNION ALL SELECT 'Gaylords / Bulk Bins', 'Size', 'Mixed', 3
        UNION ALL SELECT 'Banding Supplies', 'Type', 'Nylon', 1
        UNION ALL SELECT 'Banding Supplies', 'Type', '3/4 Inch', 2
        UNION ALL SELECT 'Banding Supplies', 'Type', '1 1/4 Inch', 3
        UNION ALL SELECT 'Oils & Lubricants', 'Quantity Type', 'Gallons', 1
        UNION ALL SELECT 'Oils & Lubricants', 'Quantity Type', 'Pounds', 2
        UNION ALL SELECT 'Vendor Supplied Filters', 'Department', 'Maintenance', 1
        UNION ALL SELECT 'Vendor Supplied Filters', 'Department', 'Die Shop', 2
        UNION ALL SELECT 'Vendor Supplied Filters', 'Department', 'Fab. Department', 3
        UNION ALL SELECT 'Bags', 'Style', 'Lay Flat', 1
        UNION ALL SELECT 'Bags', 'Style', 'Gaylord', 2
        UNION ALL SELECT 'Foam / Molded Inserts', 'Piece', 'Top / Cover', 1
        UNION ALL SELECT 'Foam / Molded Inserts', 'Piece', 'Bottom / Box', 2
        UNION ALL SELECT 'Metal Dunnage', 'Owner', 'Customer', 1
        UNION ALL SELECT 'Metal Dunnage', 'Owner', 'MTM', 2
) AS v
JOIN dunnage_types t ON t.type_name = v.type_name
JOIN dunnage_custom_fields f
  ON f.DunnageTypeID = t.id
 AND f.FieldName = v.field_name
WHERE NOT EXISTS (
    SELECT 1
    FROM dunnage_custom_field_choices c
    WHERE c.CustomFieldID = f.ID
      AND c.Choice = v.choice
);

-- =============================================
-- Step 4 — dunnage_parts
-- Parts with their type, image, label quantity header, home location, and udc1..udc10 values.
-- =============================================
INSERT INTO dunnage_parts (
    part_id,
    type_id,
    image_path,
    quantity_type,
    home_location,
    udc1,
    udc2,
    udc3,
    udc4,
    udc5,
    udc6,
    udc7,
    udc8,
    udc9,
    udc10,
    created_by,
    created_date
)
SELECT
    v.part_id,
    t.id,
    v.image_path,
    COALESCE(NULLIF(TRIM(v.quantity_type), ''), 'Quantity'),
    v.home_location,
    v.udc1,
    v.udc2,
    v.udc3,
    v.udc4,
    v.udc5,
    v.udc6,
    v.udc7,
    v.udc8,
    v.udc9,
    v.udc10,
    'seed',
    NOW()
FROM (
        SELECT '16 x 20" 4 Mil' AS part_id, 'Bags' AS type_name, 'Parts/Bags-16 x 20- 4 Mil.jpg' AS image_path, 'Bags' AS quantity_type, NULL AS home_location,
               '20' AS udc1, '4' AS udc2, '16' AS udc3, NULL AS udc4, NULL AS udc5,
               NULL AS udc6, NULL AS udc7, NULL AS udc8, NULL AS udc9, NULL AS udc10
        UNION ALL SELECT '40 x 48 L - Bags', 'Bags', 'Parts/Bags-40 x 48 L - Bags.jpg', 'Boxes', NULL,
                         '48', '40', '4', NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '40x48 Gaylord', 'Bags', NULL, 'Quantity', NULL,
                         NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '40x48 Lay Flat', 'Bags', NULL, 'Quantity', NULL,
                         NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Gaylord Bags', 'Bags', 'Parts/Bags-Gaylord Bags.png', 'Quantity', 'V-A0-01',
                         '48', '42', '66', NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'VBG00006', 'Bags', 'Parts/Bags-VBG00006.jpg', 'Bags', 'DD-00',
                         '16', '10', '21', NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '1 1/4 Inch Steel Banding', 'Banding Supplies', 'Parts/Banding-1 1-4 Inch Steel Banding.png', 'Quantity', 'T-A Racking',
                         '1 1/4 Inch', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '3/4 Inch Steel Banding', 'Banding Supplies', 'Parts/Banding-3-4 Inch Steel Banding.png', 'Coils', 'T-A Racking',
                         '3/4 Inch', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '3/4" Seals', 'Banding Supplies', 'Parts/Banding Supplies-3-4- Seals.jpg', 'Quantity', NULL,
                         NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Nylon Banding', 'Banding Supplies', 'Parts/Banding-Nylon Banding.png', 'Boxes', 'T-A Racking',
                         'Nylon', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '39 x 39 Cardboard', 'Cardboard Sheets', 'Parts/Cardboard-39 x 39.png', 'Quantity', 'U-A Racking',
                         '39', '39', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '40 x 48 Cardboard', 'Cardboard Sheets', 'Parts/Cardboard-40 x 48.png', 'Sheets', 'V-A Racking',
                         '48', '40', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '42 x 28 x 7.5 Divider', 'Cardboard Sheets', NULL, 'Quantity', NULL,
                         '42', '28', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '47 x 52 Cardboard', 'Cardboard Sheets', 'Parts/Cardboard-47 x 52.jpg', 'Sheets', 'R Bay',
                         '52', '47', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Used 40 x 48 Cardboard', 'Cardboard Sheets', 'Parts/Cardboard-Used 40 x 48.png', 'Sheets', NULL,
                         '48', '40', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '12 x 12 x 12 (DW)', 'Corrugated Boxes', 'Parts/Boxes-12 x 12 x 12 - Double Walled.jpg', 'Boxes', NULL,
                         '12', '12', 'Double Walled', '12', NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '12 x 12 x 12 (SW)', 'Corrugated Boxes', 'Parts/Boxes-12 X 12 X 12.jpg', 'Boxes', NULL,
                         '12', '12', 'Single Walled', '12', NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '12 x 12 x 6 (DW)', 'Corrugated Boxes', 'Parts/Boxes-12 x 12 x 6 - Double Walled.jpg', 'Boxes', 'V-B0-00',
                         '12', '12', 'Double Walled', '6', NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '12 x 12 x 6 (SW)', 'Corrugated Boxes', 'Parts/Boxes-12 x 12 x 6 - Single Walled.jpg', 'Boxes', 'V-B0-00',
                         '12', '12', 'Single Walled', '6', NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '12 x 8 x 4 (SW)', 'Corrugated Boxes', 'Parts/Boxes-12 x 8 x 4 - Single Walled.jpg', 'Boxes', NULL,
                         '12', '6', 'Single Walled', '4', NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '14 x 8 x 6 (SW)', 'Corrugated Boxes', 'Parts/Boxes-14 x 8 x 6 - Single Walled.jpg', 'Boxes', NULL,
                         '14', '8', 'Single Walled', '6', NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '26 x 26 x 26 (DW)', 'Corrugated Boxes', 'Parts/Boxes-26 x 26 x 26 (DW).jpg', 'Boxes', NULL,
                         '26', '26', 'Double Walled', '26', NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '30.75 x 28.75 x 25', 'Corrugated Boxes', 'Parts/Boxes-Electrolux Boxes.png', 'Boxes', 'U-04',
                         '28.75', '30.75', 'Double Walled', '25', NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '3x3x6', 'Edge Protectors', NULL, 'Quantity', NULL,
                         NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Mig Welding Wire', 'Fabrication Area', 'Parts/Fabrication Area-Mig Welding Wire.jpg', 'Weight', 'S-00',
                         NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Waterjet Garnet', 'Fabrication Area', 'Parts/Fabrication Area-Waterjet Garnet.jpg', 'Weight', NULL,
                         NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Foam Roll', 'Foam / Molded Inserts', 'Parts/Foam-Foam Roll.jpg', 'Rolls', NULL,
                         NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Gen Walls', 'Foam / Molded Inserts', NULL, 'Quantity', NULL,
                         'Gen Walls', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Genfoam 1', 'Foam / Molded Inserts', NULL, 'Quantity', NULL,
                         NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Genfoam 2', 'Foam / Molded Inserts', NULL, 'Quantity', NULL,
                         NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Genfoam 3', 'Foam / Molded Inserts', NULL, 'Quantity', NULL,
                         NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Genfoam 4', 'Foam / Molded Inserts', NULL, 'Quantity', NULL,
                         NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'MPB0000017 Box', 'Foam / Molded Inserts', NULL, 'Quantity', NULL,
                         NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'MPB0000017 Cover', 'Foam / Molded Inserts', NULL, 'Quantity', NULL,
                         NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Mixed Gaylords', 'Gaylords / Bulk Bins', 'Parts/Gaylords-Mixed Gaylords.png', 'Quantity', 'R Bay',
                         'Mixed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Short Gaylords', 'Gaylords / Bulk Bins', 'Parts/Gaylords-Short Gaylords.jpg', 'Gaylords', 'R-04',
                         'Short', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Tall Gaylords', 'Gaylords / Bulk Bins', 'Parts/Gaylords-Tall Gaylords.jpg', 'Quantity', 'R Bay',
                         'Tall', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Aqua Blast', 'Janitor Closet', 'Parts/Janitor Closet-Aqua Blast.jpg', 'Gallons', NULL,
                         NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Blue Spruce', 'Janitor Closet', 'Parts/Janitor Closet-Blue Spruce.jpg', 'Bottles', NULL,
                         NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Fireball', 'Janitor Closet', 'Parts/Janitor Closet-Fireball.jpg', 'Bottles', NULL,
                         NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Healthy Hands', 'Janitor Closet', 'Parts/Janitor Closet-Healthy Hands.jpg', 'Bottles', NULL,
                         NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Paint Remover', 'Janitor Closet', 'Parts/Janitor Closet-Paint Remover.jpg', 'Bottles', NULL,
                         NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Paper Towels', 'Janitor Closet', 'Parts/Janitor Closet-Paper Towels.jpg', 'Boxes', 'Janitor''s Closet',
                         NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Sparkle Cleaner', 'Janitor Closet', 'Parts/Janitor Closet-Sparkle Cleaner.jpg', 'Boxes', 'Janitor''s Closet',
                         NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Sunbath', 'Janitor Closet', 'Parts/Janitor Closet-Sunbath.jpg', 'Bottles', NULL,
                         NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Toilet Paper', 'Janitor Closet', 'Parts/Janitor Closet-Toilet Paper.jpg', 'Boxes', NULL,
                         NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'AKK46582', 'Metal Dunnage', 'Parts/Metal Racks-John Deere - AKK46582.png', 'Racks', 'Outside Door 5',
                         'John Deere', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'AKK54380', 'Metal Dunnage', 'Parts/Metal Racks-John Deere - AKK543.png', 'Racks', 'Outside Door 5',
                         'John Deere', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Blue Jarkies', 'Metal Dunnage', 'Parts/Metal Dunnage-Blue Jarkies.jpg', 'Jarkies', 'Door 13',
                         'Lennox', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Blue Racks', 'Metal Dunnage', 'Parts/Metal Dunnage-Blue Racks.jpg', 'Racks', NULL,
                         'Daimler', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Crenlo Racks', 'Metal Dunnage', NULL, 'Quantity', NULL,
                         NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'IMD559878', 'Metal Dunnage', 'Parts/Metal Dunnage-IMD559878.jpg', 'Racks', NULL,
                         'John Deere', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'IMD560176', 'Metal Dunnage', 'Parts/Metal Dunnage-IMD560176.jpg', 'Racks', NULL,
                         'John Deere', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'MTM Crates', 'Metal Dunnage', NULL, 'Quantity', 'Outside Door 5',
                         NULL, 'MTM', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Multi-Purpose Rack', 'Metal Dunnage', 'Parts/Metal Racks-Crenlo Rack.png', 'Racks', NULL,
                         'Crenlo', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '2" Tape', 'Office Supplies', 'Parts/Office Supplies-2- Tape.jpg', 'Rolls', NULL,
                         NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '3" Tape', 'Office Supplies', 'Parts/Office Supplies-3- Tape.jpg', 'Rolls', NULL,
                         NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '3" Tape Roller', 'Office Supplies', 'Parts/Office Supplies-3- Tape Roller.jpg', 'Pieces', NULL,
                         NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '6 x 4 Thermal Labels', 'Office Supplies', 'Parts/Office Supplies-6 x 4 Thermal Labels.jpg', 'Rolls', NULL,
                         NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'CAD Bond Rolls', 'Office Supplies', 'Parts/Office Supplies-CAD Bond Rolls.jpg', 'Rolls', NULL,
                         NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'NCM Tags', 'Office Supplies', 'Parts/Office Supplies-NCM Tags.jpg', 'Quantity', 'Production Office',
                         NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Receiving Tags', 'Office Supplies', 'Parts/Office Supplies-Receiving Tags.png', 'Quantity', 'Receiving Office',
                         NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Castrol Hysol MB50', 'Oils & Lubricants', 'Parts/Oils & Lubricants-Castrol Hysol MB50.jpg', 'Gallons', 'Die Shop',
                         'Gallons', '55', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Draw Lube 47', 'Oils & Lubricants', 'Parts/Oils & Lubricants-Draw Lube 47.jpg', 'Gallons', 'U-A0-00',
                         'Gallons', '55', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'DuraDraw 1446', 'Oils & Lubricants', 'Parts/Oils & Lubricants-DuraDraw 1446.jpg', 'Gallons', NULL,
                         'Gallons', '5', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'DuraDraw 6802', 'Oils & Lubricants', 'Parts/Oils & Lubricants-DuraDraw 6802.jpg', 'Gallons', 'U-A0-00',
                         'Gallons', '55', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'DuraDraw 77NF', 'Oils & Lubricants', 'Parts/Oils & Lubricants-DuraDraw 77NF.jpg', 'Buckets', NULL,
                         'Gallons', '5', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'DuraKlean', 'Oils & Lubricants', 'Parts/Oils & Lubricants-DuraKlean.jpg', 'Gallons', 'U-A0-00',
                         'Gallons', '55', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'IWT-4B-55', 'Oils & Lubricants', 'Parts/Oils & Lubricants-IWT-4B-55.jpg', 'Gallons', 'U-A0-00',
                         'Gallons', '55', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Metchem 6850', 'Oils & Lubricants', 'Parts/Oils & Lubricants-Metchem 6850.jpg', 'Gallons', 'U-A0-00',
                         'Gallons', '55', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Mobilux EP 0', 'Oils & Lubricants', 'Parts/Oils & Lubricants-Mobilux EP 0.jpg', 'Gallons', NULL,
                         'Gallons', '5', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Mystik Lithium EP-2 Grease', 'Oils & Lubricants', NULL, 'Gallons', NULL,
                         'Gallons', '5', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Perdraw 1043', 'Oils & Lubricants', 'Parts/Oils & Lubricants-Perdraw 1043.png', 'Gallons', 'T-A or R-A Racking',
                         'Gallons', '330', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Pro Performance AW 32-55', 'Oils & Lubricants', 'Parts/Oils & Lubricants-Pro Performance AW 32-55.jpg', 'Gallons', 'U-A0-00',
                         'Gallons', '55', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Pro Performance AW 46-55', 'Oils & Lubricants', 'Parts/Oils & Lubricants-Pro Performance AW 46-55.jpg', 'Gallons', 'U-A0-00',
                         'Gallons', '55', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Pro Performance Gear EP 220-55', 'Oils & Lubricants', 'Parts/Oils & Lubricants-Pro Performance Gear EP 220-55.png', 'Gallons', 'U-A Area',
                         'Gallons', '55', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Pro Performance Gear EP100-55', 'Oils & Lubricants', NULL, 'Gallons', NULL,
                         NULL, '55', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Pro Performance Way 220-55', 'Oils & Lubricants', 'Parts/Oils & Lubricants-Pro Performance Way 220-55.png', 'Gallons', 'U-A0-00',
                         'Gallons', '55', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Pro Performance Way 68-55', 'Oils & Lubricants', 'Parts/Oils & Lubricants-Pro Performance Way 68-55.png', 'Gallons', 'U-A Racking',
                         'Gallons', '55', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'RenoForm OS 7802', 'Oils & Lubricants', 'Parts/Oils & Lubricants-RenoForm OS 7802.jpg', 'Gallons', NULL,
                         'Gallons', '330', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Syntilo 9930', 'Oils & Lubricants', 'Parts/Oils & Lubricants-Syntilo 9930.jpg', 'Gallons', 'Die Shop',
                         'Gallons', '55', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '100 x 40', 'Pallets / Skids', NULL, 'Skids', NULL,
                         '100', '40', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '32 x 30 Skids', 'Pallets / Skids', 'Parts/Skids-32 x 30 Skids.png', 'Quantity', 'Outside Door 13',
                         '32', '30', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '40 x 100 Skids', 'Pallets / Skids', NULL, 'Skids', NULL,
                         '100', '40', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '40 x 48 Skids', 'Pallets / Skids', 'Parts/Skids-40 x 48 Skids.jpg', 'Skids', 'Back Pad',
                         '48', '40', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '42 x 42 Skids', 'Pallets / Skids', 'Parts/Skids-42 x 42 Skids.jpg', 'Skids', 'Door 13',
                         '42', '42', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '42 x 54 Heat Treated', 'Pallets / Skids', 'Parts/Skids-42 x 54 Heat Treated.jpg', 'Skids', NULL,
                         '54', '42', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '48 x 48 Skids', 'Pallets / Skids', NULL, 'Skids', NULL,
                         '48', '48', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '52 x 47 Skids', 'Pallets / Skids', NULL, 'Skids', 'Back Pad',
                         '52', '47', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '74 x 40 Heat Treated', 'Pallets / Skids', 'Parts/Skids-74 x 40 Heat Treated.jpg', 'Skids', NULL,
                         '74', '40', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '85 x 40 Skids', 'Pallets / Skids', NULL, 'Skids', 'Door 13',
                         '84', '40', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '15 x 12 x 7 - Grey Totes', 'Plastic Dunnage', 'Parts/Kawasaki Totes-7 x 9.5 x 13 - Grey.jpg', 'Totes', 'RECV',
                         '14', '12', '7', 'Kawasaki', NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '16 x 15 x 9 - Grey Totes', 'Plastic Dunnage', 'Parts/Kawasaki Totes-9.5 x 13 x 13 - Grey.jpg', 'Totes', 'RECV',
                         '13', '13', '9', 'Kawasaki', NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '24 x 15 x 7 - Grey Totes', 'Plastic Dunnage', 'Parts/Kawasaki Totes-7.5 x 13 x 21.5 - Grey.jpg', 'Totes', 'RECV',
                         '24', '14', '7', 'Kawasaki', NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '24 x 22 x 9 - Green Totes', 'Plastic Dunnage', 'Parts/Kawasaki Totes-9 x 20.5 x 21.5 - Green.jpg', 'Totes', 'RECV',
                         '20', '21', '9', 'Kawasaki', NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Black A Totes', 'Plastic Dunnage', 'Parts/Plastic Dunnage-Black A Totes.jpg', 'Quantity', 'Vits Drive',
                         '12', '11', '8', 'Allison Transmission', NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Long A Totes', 'Plastic Dunnage', 'Parts/Plastic Dunnage-Long A Totes.jpg', 'Quantity', 'RECV-VITS',
                         '23', '11', '9', 'Allison Transmission', NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'RCA05', 'Plastic Dunnage', 'Parts/Plastic Dunnage-RCA05.jpg', 'Totes', 'RECV-VITS',
                         '6', '10', '5', 'John Deere', NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'RCB05', 'Plastic Dunnage', 'Parts/Plastic Dunnage-RCB05.jpg', 'Totes', NULL,
                         '13', '10', '5', 'John Deere', NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'RCG25', 'Plastic Dunnage', 'Parts/Plastic Dunnage-RCG25.jpg', 'Crates', 'Outside Receiving',
                         '32', '30', '25', 'John Deere', NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'RCG34', 'Plastic Dunnage', 'Parts/Plastic Dunnage-RCG34.jpg', 'Crates', 'Outside Receiving',
                         '32', '30', '34', 'John Deere', NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'RCK25', 'Plastic Dunnage', 'Parts/Plastic Dunnage-RCK25.jpg', 'Quantity', 'Outside Receiving Docks',
                         '48', '45', '25', 'John Deere', NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'RCK34', 'Plastic Dunnage', 'Parts/Plastic Dunnage-RCK34.jpg', 'Crates', 'Outside Receiving Docks',
                         '48', '45', '34', 'John Deere', NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Short IM Foldable', 'Plastic Dunnage', NULL, 'Quantity', NULL,
                         '48', '45', '36', 'IMMI', NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Tall IM Foldable', 'Plastic Dunnage', 'Parts/Plastic Dunnage-IMMI Foldable.jpg', 'Crates', NULL,
                         '45', '48', '50', 'IMMI', NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'ULine 48 x 45 x 25', 'Plastic Dunnage', 'Parts/Plastic Dunnage-ULine 48 x 45 x 25.jpg', 'Crates', NULL,
                         '45', '48', '25', 'ULine', NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Z Totes', 'Plastic Dunnage', 'Parts/Plastic Dunnage-ZF Totes.jpg', 'Totes', 'RECV-VITS',
                         '15', '12', '9', 'ZF Transmissions', NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'RMA Parts', 'Quality Receiving', NULL, 'Pieces', 'Quality',
                         NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Camo Multi-Tool', 'Raffle', 'Parts/Raffle-Camo Multi-Tool.png', 'Pieces', NULL,
                         NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Counter Brush Combo', 'Raffle', 'Parts/Raffle-Counter Brush Combo.jpg', 'Pieces', NULL,
                         NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Allison', 'Returnable Totes', NULL, 'Quantity', 'RECV',
                         NULL, NULL, NULL, 'Allison', NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Ariens', 'Returnable Totes', NULL, 'Quantity', NULL,
                         NULL, NULL, NULL, 'Ariens', NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Kawasaki 14x12x7', 'Returnable Totes', NULL, 'Quantity', 'WC',
                         NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Kawasaki 15x12x7', 'Returnable Totes', NULL, 'Quantity', NULL,
                         NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Kawasaki 24x14x7', 'Returnable Totes', NULL, 'Quantity', NULL,
                         NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Lennox Tote', 'Returnable Totes', NULL, 'Quantity', NULL,
                         NULL, NULL, NULL, 'Lennox', NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'ZF', 'Returnable Totes', NULL, 'Quantity', NULL,
                         NULL, NULL, NULL, 'ZF', NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'BH012-FORGE-CS', 'Special', 'Parts/Special-BH012-FORGE-CS.jpg', 'Weight', NULL,
                         NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'MT-C-GS-A', 'Special', 'Parts/Special-MT-C-GS-A.jpg', 'Pieces', NULL,
                         NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'MT-C-GS-HA', 'Special', 'Parts/Special-MT-C-GS-HA.jpg', 'Pieces', NULL,
                         NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Auto Wrapper', 'Stretch Film / Shrink Wrap', 'Parts/Shrink Wrap-Auto Wrapper.jpg', 'Rolls', NULL,
                         NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Blue Shrink Wrap', 'Stretch Film / Shrink Wrap', 'Parts/Shrink Wrap-Blue Generac Shrink Wrap.png', 'Rolls', 'V-A0-01',
                         '1000', '20', '80', NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Hand Held', 'Stretch Film / Shrink Wrap', 'Parts/Shrink Wrap-Hand Held.png', 'Boxes', 'T-A0-00',
                         '1500', '18', '80', NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Polyolefin Shrink Film', 'Stretch Film / Shrink Wrap', 'Parts/Shrink Wrap-Polyolefin Shrink Film.jpg', 'Quantity', NULL,
                         NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'ULine Hand Savers', 'Stretch Film / Shrink Wrap', 'Parts/Shrink Wrap-ULine Hand Savers.jpg', 'Quantity', NULL,
                         NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'CO-084232/1', 'Trial Material', 'Parts/Trial Material-CO-084232-1.jpg', 'Weight', 'V-C0-07',
                         'CO-084232/1', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'CO-086060 / 1', 'Trial Material', 'Parts/Trial Material-CO-086060 - 1.jpg', 'Quantity', NULL,
                         'CO-086060 / 1', 'Refer to Vendor Tag', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'CO-086065/1', 'Trial Material', 'Parts/Trial Material-CO-086065-1.jpg', 'Weight', NULL,
                         'CO-086065/1', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Hussey Seating Trial', 'Trial Material', 'Parts/Trial Material-Hussey Seating Trial.jpg', 'Weight', NULL,
                         'Customer Supplied', 'N/A', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '12 x 18 x 4 Preopen Bag', 'U-Line', 'Parts/U-Line-12 x 18 x 4 Preopen Bag.jpg', 'Rolls', NULL,
                         'S-6530', 'false', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '2" Steel Cutter', 'U-Line', 'Parts/U-Line-2- Steel Cutter.jpg', 'Pieces', NULL,
                         'H-601', 'false', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '2" Tape Dispenser', 'U-Line', 'Parts/U-Line-2- Tape Dispenser.jpg', 'Pieces', NULL,
                         'H-150', 'false', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '36 x 18 x 72 Cabinet', 'U-Line', 'Parts/U-Line-36 x 18 x 72 Cabinet.jpg', 'Pieces', NULL,
                         'H-1105ABL', 'false', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '7/8" White Strap Guards', 'U-Line', 'Parts/U-Line-7-8- White Strap Guards.jpg', 'Pieces', NULL,
                         'S-764', 'false', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '8 Oz Bulk Jars', 'U-Line', 'Parts/U-Line-8 Oz Bulk Jars.jpg', 'Pieces', NULL,
                         'S-14508B-W / S-14508B-JAR / S-12758BPLID', 'false', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '9'' Umbrella - Black', 'U-Line', 'Parts/U-Line-9'' Umbrella - Black.jpg', 'Boxes', NULL,
                         'H-8158BL', 'false', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Chipboard Pads', 'U-Line', 'Parts/U-Line-Chipboard Pads.jpg', 'Boxes', NULL,
                         'S-14209', 'false', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Cotton Tip Applicator', 'U-Line', 'Parts/U-Line-Cotton Tip Applicator.jpg', 'Pieces', NULL,
                         'S-21101', 'false', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'HD Steel Shelving', 'U-Line', 'Parts/U-Line-HD Steel Shelving.jpg', 'Pieces', NULL,
                         'H-11965, H-11967, H-1523-POST', 'false', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Hardware Pack/Handle for H-2488', 'U-Line', 'Parts/U-Line-Hardware Pack-Handle for H-2488.jpg', 'Pieces', NULL,
                         'H-2488-LKHND', 'false', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Pure Flow Saline Cartridge', 'U-Line', 'Parts/U-Line-Pure Flow Saline Cartridge.jpg', 'Cartridges', NULL,
                         'S-11507', 'false', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Retractable Barrier', 'U-Line', 'Parts/U-Line-Retractable Barrier.jpg', 'Pieces', NULL,
                         'H-5875{Color}', 'false', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Safety Glasses Dispenser', 'U-Line', 'Parts/U-Line-Safety Glasses Dispenser.jpg', 'Boxes', NULL,
                         'H-4157', 'false', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Spray Paint', 'U-Line', 'Parts/U-Line-Spray Paint.jpg', 'Cans', NULL,
                         'S-21952+(Color)', 'false', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'T50 Staple Gun', 'U-Line', 'Parts/U-Line-T50 Staple Gun.jpg', 'Pieces', NULL,
                         'H-2019', 'false', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '14 x 25 x 2', 'Vendor Supplied Filters', NULL, 'Filters', 'Maintenance',
                         'Maintenance', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '16 x 16 x 2', 'Vendor Supplied Filters', NULL, 'Filters', 'Maintenance',
                         'Maintenance', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '16 x 25 x 2', 'Vendor Supplied Filters', NULL, 'Filters', 'Maintenance',
                         'Maintenance', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '18 x 24 x 2', 'Vendor Supplied Filters', NULL, 'Filters', 'Maintenance',
                         'Maintenance', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '20 x 20 x 2', 'Vendor Supplied Filters', NULL, 'Filters', 'Maintenance',
                         'Maintenance', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '20 x 25 x 1', 'Vendor Supplied Filters', NULL, 'Filters', 'Maintenance',
                         'Maintenance', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '20 x 25 x 2', 'Vendor Supplied Filters', NULL, 'Filters', 'Maintenance',
                         'Maintenance', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT '24 x 24 x 2', 'Vendor Supplied Filters', NULL, 'Filters', 'Maintenance',
                         'Maintenance', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Generac Wire Basket', 'Wire Baskets', 'Parts/Wire Baskets-Generac Wire Basket.jpg', 'Baskets', 'Backpad',
                         'Any Customer', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Short Baskets', 'Wire Baskets', 'Parts/Insinkerator Baskets-Short Baskets.png', 'Quantity', 'U-04',
                         'Insinkerator', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Tall Baskets', 'Wire Baskets', 'Parts/Insinkerator Baskets-Tall Baskets.png', 'Quantity', 'U-04',
                         'Insinkerator', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Short K Crates', 'Wooden Dunnage', 'Parts/Wooden Dunnage-Short K Crates.png', 'Quantity', 'Back Pad',
                         'Short', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
        UNION ALL SELECT 'Tall K Crates', 'Wooden Dunnage', 'Parts/Wooden Dunnage-Tall K Crates.jpg', 'Quantity', NULL,
                         'Tall', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
) AS v
JOIN dunnage_types t ON t.type_name = v.type_name
WHERE NOT EXISTS (
    SELECT 1
    FROM dunnage_parts p
    WHERE p.part_id = v.part_id
);

-- =============================================
-- Step 5 — dunnage_requires_inventory
-- Returnable dunnage tracked in Visual ERP, one method per part.
-- =============================================
-- Only parts the seed itself creates can carry an inventory method; a row for a part the
-- database does not have would fail the foreign key on a fresh install.
INSERT INTO dunnage_requires_inventory (part_id, inventory_method, created_by, created_date)
SELECT v.part_id, v.inventory_method, 'seed', NOW()
FROM (
        SELECT 'RCK34' AS part_id, 'Adjust In' AS inventory_method
        UNION ALL SELECT 'RCK25', 'Adjust In'
        UNION ALL SELECT 'RCG34', 'Adjust In'
        UNION ALL SELECT 'RCG25', 'Adjust In'
        UNION ALL SELECT 'RCA05', 'Adjust In'
        UNION ALL SELECT 'RCB05', 'Adjust In'
        UNION ALL SELECT 'VBG00006', 'Receive In'
        UNION ALL SELECT 'CO-086065/1', 'WIP'
        UNION ALL SELECT 'CO-086060 / 1', 'WIP'
        UNION ALL SELECT 'AKK46582', 'Adjust In'
        UNION ALL SELECT 'Crenlo Racks', 'Adjust In'
        UNION ALL SELECT 'Kawasaki 14x12x7', 'Adjust In'
        UNION ALL SELECT 'Kawasaki 15x12x7', 'Adjust In'
        UNION ALL SELECT 'Kawasaki 24x14x7', 'Adjust In'
        UNION ALL SELECT 'Allison', 'Adjust In'
        UNION ALL SELECT 'Ariens', 'Adjust In'
        UNION ALL SELECT 'Lennox Tote', 'Adjust In'
        UNION ALL SELECT 'ZF', 'Adjust In'
) AS v
JOIN dunnage_parts p ON p.part_id = v.part_id
WHERE NOT EXISTS (
    SELECT 1
    FROM dunnage_requires_inventory r
    WHERE r.part_id = v.part_id
      AND COALESCE(r.inventory_method, '') = COALESCE(v.inventory_method, '')
);

-- =============================================
-- Step 6 — dunnage_non_po_entries
-- Reusable non-PO reference reasons.
-- =============================================
-- sp_Dunnage_NonPO_Upsert increments use_count when the value already exists, so the seed
-- writes the row itself and leaves an existing entry - and its use count - untouched.
INSERT INTO dunnage_non_po_entries (value, created_by, use_count)
SELECT v.value, 'seed', 1
FROM (
        SELECT 'Customer Supplied' AS value
        UNION ALL SELECT 'Return to Inventory'
        UNION ALL SELECT 'Vendor Supplied'
        UNION ALL SELECT 'Delivered from Expo'
        UNION ALL SELECT 'Trial Material'
        UNION ALL SELECT 'Customer RMA'
        UNION ALL SELECT '071067'
        UNION ALL SELECT 'forgot PO'
        UNION ALL SELECT 'No PO Required'
        UNION ALL SELECT 'Internal Use'
        UNION ALL SELECT 'Maintenance'
        UNION ALL SELECT 'Sample / Demo'
        UNION ALL SELECT 'Warranty Return'
        UNION ALL SELECT 'Scrap / Disposal'
) AS v
WHERE NOT EXISTS (
    SELECT 1
    FROM dunnage_non_po_entries e
    WHERE e.value = v.value
);

-- =============================================
-- Step 7 — dunnage_no_label_parts
-- Parts that never need a printed label.
-- =============================================
INSERT INTO dunnage_no_label_parts (part_id, created_by, created_date)
SELECT v.part_id, 'seed', NOW()
FROM (
        SELECT 'Blue Jarkies' AS part_id
        UNION ALL SELECT 'Pro Performance Gear EP 220-55'
        UNION ALL SELECT 'Short Gaylords'
        UNION ALL SELECT 'Short Baskets'
        UNION ALL SELECT 'Tall Baskets'
        UNION ALL SELECT 'Short K Crates'
        UNION ALL SELECT 'Tall K Crates'
        UNION ALL SELECT 'MTM Crates'
) AS v
JOIN dunnage_parts p ON p.part_id = v.part_id
WHERE NOT EXISTS (
    SELECT 1
    FROM dunnage_no_label_parts n
    WHERE n.part_id = v.part_id
);

-- =============================================
-- Step 8 — dunnage_non_po_part_defaults
-- The non-PO reference each part suggests by default.
-- =============================================
INSERT INTO dunnage_non_po_part_defaults (part_id, value, updated_by)
SELECT v.part_id, v.value, 'seed'
FROM (
        SELECT '100 x 40' AS part_id, '071067' AS value
        UNION ALL SELECT '12 x 12 x 6 (DW)', 'Return to Inventory'
        UNION ALL SELECT '12 x 12 x 6 (SW)', 'Customer Supplied'
        UNION ALL SELECT '14 x 25 x 2', 'Vendor Supplied'
        UNION ALL SELECT '14 x 8 x 6 (SW)', 'Return to Inventory'
        UNION ALL SELECT '15 x 12 x 7 - Grey Totes', 'Customer Supplied'
        UNION ALL SELECT '16 x 15 x 9 - Grey Totes', 'Customer Supplied'
        UNION ALL SELECT '16 x 16 x 2', 'Vendor Supplied'
        UNION ALL SELECT '16 x 25 x 2', 'Vendor Supplied'
        UNION ALL SELECT '18 x 24 x 2', 'Vendor Supplied'
        UNION ALL SELECT '20 x 20 x 2', 'Vendor Supplied'
        UNION ALL SELECT '20 x 25 x 1', 'Vendor Supplied'
        UNION ALL SELECT '20 x 25 x 2', 'Vendor Supplied'
        UNION ALL SELECT '24 x 15 x 7 - Grey Totes', 'Customer Supplied'
        UNION ALL SELECT '24 x 22 x 9 - Green Totes', 'Customer Supplied'
        UNION ALL SELECT '24 x 24 x 2', 'Vendor Supplied'
        UNION ALL SELECT '42 x 28 x 7.5 Divider', 'Return to Inventory'
        UNION ALL SELECT 'AKK46582', 'Customer Supplied'
        UNION ALL SELECT 'AKK54380', 'Customer Supplied'
        UNION ALL SELECT 'BH012-FORGE-CS', 'Vendor Supplied'
        UNION ALL SELECT 'Black A Totes', 'Customer Supplied'
        UNION ALL SELECT 'Blue Jarkies', 'Customer Supplied'
        UNION ALL SELECT 'Blue Racks', 'Customer Supplied'
        UNION ALL SELECT 'CO-084232/1', 'Trial Material'
        UNION ALL SELECT 'CO-086060 / 1', 'Trial Material'
        UNION ALL SELECT 'CO-086065/1', 'Trial Material'
        UNION ALL SELECT 'DuraDraw 1446', 'Trial Material'
        UNION ALL SELECT 'DuraDraw 6802', 'forgot PO'
        UNION ALL SELECT 'DuraDraw 77NF', 'Trial Material'
        UNION ALL SELECT 'IMD559878', 'Customer Supplied'
        UNION ALL SELECT 'IMD560176', 'Customer Supplied'
        UNION ALL SELECT 'Long A Totes', 'Customer Supplied'
        UNION ALL SELECT 'MT-C-GS-A', 'Vendor Supplied'
        UNION ALL SELECT 'MT-C-GS-HA', 'Vendor Supplied'
        UNION ALL SELECT 'Perdraw 1043', 'stock replenishment'
        UNION ALL SELECT 'RCA05', 'Customer Supplied'
        UNION ALL SELECT 'RCB05', 'Customer Supplied'
        UNION ALL SELECT 'RCG25', 'Customer Supplied'
        UNION ALL SELECT 'RCG34', 'Customer Supplied'
        UNION ALL SELECT 'RCK25', 'Customer Supplied'
        UNION ALL SELECT 'RCK34', 'Customer Supplied'
        UNION ALL SELECT 'RMA Parts', 'Customer RMA'
        UNION ALL SELECT 'Short Baskets', 'Customer Supplied'
        UNION ALL SELECT 'Short IM Foldable', 'Customer Supplied'
        UNION ALL SELECT 'Short K Crates', 'Customer Supplied'
        UNION ALL SELECT 'Tall Baskets', 'Customer Supplied'
        UNION ALL SELECT 'Tall IM Foldable', 'Customer Supplied'
        UNION ALL SELECT 'Tall K Crates', 'Customer Supplied'
        UNION ALL SELECT 'Z Totes', 'Customer Supplied'
) AS v
JOIN dunnage_parts p ON p.part_id = v.part_id
WHERE NOT EXISTS (
    SELECT 1
    FROM dunnage_non_po_part_defaults d
    WHERE d.part_id = v.part_id
);


COMMIT;

SELECT
    'dunnage_types' AS `table`,
    COUNT(*) AS `row_count`
FROM
    dunnage_types
UNION
ALL
SELECT
    'dunnage_custom_fields' AS `table`,
    COUNT(*) AS `row_count`
FROM
    dunnage_custom_fields
UNION
ALL
SELECT
    'dunnage_custom_field_choices' AS `table`,
    COUNT(*) AS `row_count`
FROM
    dunnage_custom_field_choices
UNION
ALL
SELECT
    'dunnage_parts' AS `table`,
    COUNT(*) AS `row_count`
FROM
    dunnage_parts
UNION
ALL
SELECT
    'dunnage_requires_inventory' AS `table`,
    COUNT(*) AS `row_count`
FROM
    dunnage_requires_inventory
UNION
ALL
SELECT
    'dunnage_non_po_entries' AS `table`,
    COUNT(*) AS `row_count`
FROM
    dunnage_non_po_entries
UNION
ALL
SELECT
    'dunnage_no_label_parts' AS `table`,
    COUNT(*) AS `row_count`
FROM
    dunnage_no_label_parts
UNION
ALL
SELECT
    'dunnage_non_po_part_defaults' AS `table`,
    COUNT(*) AS `row_count`
FROM
    dunnage_non_po_part_defaults;
