-- =============================================
-- Seed: Dunnage Master Data
-- Created  : 2026-03-19
-- Updated  : 2026-08-20
-- Source   : docs/GoogleSheetsVersion/Dunnage-Migration-Data.md
-- =============================================
-- Inserts:
--   Step 1 — dunnage_types
--   Step 2 — dunnage_parts
--   Step 3 — dunnage_specs (per-type spec templates derived from part spec_values)
--   Step 4 — dunnage_requires_inventory (returnable dunnage tracked in Visual ERP)
--   Step 5 — dunnage_non_po_entries (reusable non-PO reference reasons)
--   Note   — dunnage_quantity_types is seeded by schema file 40_Table_dunnage_quantity_types.sql
-- =============================================
USE mtm_receiving_application;

START TRANSACTION;

SET
    @id = 0;

CALL sp_Dunnage_Types_Insert(
    'Pallets / Skids',
    'ShippingPallet',
    NULL,
    'seed',
    @id
);

SET
    @t1 = @id;

CALL sp_Dunnage_Types_Insert(
    'Cardboard Sheets / Slip Sheets',
    'Layers',
    NULL,
    'seed',
    @id
);

SET
    @t2 = @id;

CALL sp_Dunnage_Types_Insert(
    'Corrugated Boxes',
    'PackageVariantClosed',
    NULL,
    'seed',
    @id
);

SET
    @t3 = @id;

CALL sp_Dunnage_Types_Insert(
    'Gaylords / Bulk Bins',
    'PackageVariant',
    NULL,
    'seed',
    @id
);

SET
    @t4 = @id;

CALL sp_Dunnage_Types_Insert(
    'Stretch Film / Shrink Wrap',
    'Autorenew',
    NULL,
    'seed',
    @id
);

SET
    @t5 = @id;

CALL sp_Dunnage_Types_Insert(
    'Bags',
    'BagPersonal',
    NULL,
    'seed',
    @id
);

SET
    @t6 = @id;

CALL sp_Dunnage_Types_Insert(
    'Tape / Strapping / Banding',
    'Selection',
    NULL,
    'seed',
    @id
);

SET
    @t7 = @id;

CALL sp_Dunnage_Types_Insert(
    'Edge Protectors',
    'ShieldOutline',
    NULL,
    'seed',
    @id
);

SET
    @t8 = @id;

CALL sp_Dunnage_Types_Insert(
    'Foam / Molded Inserts',
    'LayersOutline',
    NULL,
    'seed',
    @id
);

SET
    @t9 = @id;

CALL sp_Dunnage_Types_Insert(
    'Returnable Racks - John Deere',
    'Warehouse',
    NULL,
    'seed',
    @id
);

SET
    @t10 = @id;

CALL sp_Dunnage_Types_Insert(
    'Returnable Racks - Other',
    'Forklift',
    NULL,
    'seed',
    @id
);

SET
    @t11 = @id;

CALL sp_Dunnage_Types_Insert(
    'Returnable Totes',
    'BoxVariantClosed',
    NULL,
    'seed',
    @id
);

SET
    @t12 = @id;

CALL sp_Dunnage_Types_Insert(
    'Returnable Baskets / Wire Containers',
    'BasketOutline',
    NULL,
    'seed',
    @id
);

SET
    @t13 = @id;

CALL sp_Dunnage_Parts_Insert(
    '20x20',
    @t1,
    '{"dimensions":"20x20"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '32x30',
    @t1,
    '{"dimensions":"32x30"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '40x48',
    @t1,
    '{"dimensions":"40x48"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '42x42',
    @t1,
    '{"dimensions":"42x42"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '42x54',
    @t1,
    '{"dimensions":"42x54"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '45x48',
    @t1,
    '{"dimensions":"45x48"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '47x52',
    @t1,
    '{"dimensions":"47x52"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '48x48',
    @t1,
    '{"dimensions":"48x48"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '52x47',
    @t1,
    '{"dimensions":"52x47"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '54x42',
    @t1,
    '{"dimensions":"54x42"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '74x40',
    @t1,
    '{"dimensions":"74x40"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '85x40',
    @t1,
    '{"dimensions":"85x40"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '100x40',
    @t1,
    '{"dimensions":"100x40"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '40x48 John Deere',
    @t1,
    '{"dimensions":"40x48","customer":"John Deere"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Styberg',
    @t1,
    '{"customer":"Styberg"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '16x38.5',
    @t2,
    '{"dimensions":"16x38.5"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '39x39',
    @t2,
    '{"dimensions":"39x39"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '40x48 Sheet',
    @t2,
    '{"dimensions":"40x48"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '47x52 Sheet',
    @t2,
    '{"dimensions":"47x52"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '48x40',
    @t2,
    '{"dimensions":"48x40"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '52x47 Sheet',
    @t2,
    '{"dimensions":"52x47"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'John Deere',
    @t2,
    '{"customer":"John Deere"}',
    NULL,
    'Quantity',
    'S-E Racking',
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '8x5x5',
    @t3,
    '{"length":"8","width":"5","height":"5"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '12x12x6 Single Wall',
    @t3,
    '{"length":"12","width":"12","height":"6","wall_type":"Single Wall"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '12x12x6 Double Wall',
    @t3,
    '{"length":"12","width":"12","height":"6","wall_type":"Double Wall"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '12x12x12 Single Wall',
    @t3,
    '{"length":"12","width":"12","height":"12","wall_type":"Single Wall"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '12x12x12 Double Wall',
    @t3,
    '{"length":"12","width":"12","height":"12","wall_type":"Double Wall"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '16x12x8',
    @t3,
    '{"length":"16","width":"12","height":"8"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '24x16x8 Volvo',
    @t3,
    '{"length":"24","width":"16","height":"8","customer":"Volvo"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'MPB0000005',
    @t3,
    '{"length":"25","width":"16","height":"18","part_number":"MPB0000005"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '26x26x26',
    @t3,
    '{"length":"26","width":"26","height":"26"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '28x16x17',
    @t3,
    '{"length":"28","width":"16","height":"17"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '32x30x15 Electrolux',
    @t3,
    '{"length":"32","width":"30","height":"15","customer":"Electrolux"}',
    NULL,
    'Quantity',
    'Floor - In front of V-N Racking',
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '32x30x25 Electrolux',
    @t3,
    '{"length":"32","width":"30","height":"25","customer":"Electrolux"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '42x24x30',
    @t3,
    '{"length":"42","width":"24","height":"30"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Lennox Box',
    @t3,
    '{"customer":"Lennox"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Short',
    @t4,
    '{"height_type":"Short"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Tall',
    @t4,
    '{"height_type":"Tall"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Hand-Held',
    @t5,
    '{"application":"Hand-Held"}',
    NULL,
    'Quantity',
    'T - Bay',
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Auto-Wrapper',
    @t5,
    '{"application":"Auto-Wrapper"}',
    NULL,
    'Quantity',
    'Rack by 100-15',
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '40x48 Lay Flat',
    @t6,
    '{"length":"40","width":"48","mil":"4","style":"Lay Flat"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '40x48 Gaylord',
    @t6,
    '{"length":"40","width":"48","mil":"4","style":"Gaylord"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Tape 2 Inch',
    @t7,
    '{"width":"2 inch","material":"Tape"}',
    NULL,
    'Quantity',
    'T - Bay',
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Banding 3/4 Inch Steel',
    @t7,
    '{"width":"3/4 inch","material":"Steel","style":"Banding"}',
    NULL,
    'Quantity',
    'T - Bay',
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Strapping 1-1/4 Inch Steel',
    @t7,
    '{"width":"1-1/4 inch","material":"Steel","style":"Strapping"}',
    NULL,
    'Quantity',
    'T - Bay',
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Banding Nylon',
    @t7,
    '{"material":"Nylon","style":"Banding"}',
    NULL,
    'Quantity',
    'T - Bay',
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '3x3x6',
    @t8,
    '{"width":"3","depth":"3","length":"6"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Genfoam 1',
    @t9,
    '{"type":"Genfoam","variant":"1"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Genfoam 2',
    @t9,
    '{"type":"Genfoam","variant":"2"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Genfoam 3',
    @t9,
    '{"type":"Genfoam","variant":"3"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Genfoam 4',
    @t9,
    '{"type":"Genfoam","variant":"4"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Gen Walls',
    @t9,
    '{"type":"Gen Walls"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'MPB0000017 Cover',
    @t9,
    '{"part_family":"MPB0000017","piece":"Top / Cover"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'MPB0000017 Box',
    @t9,
    '{"part_family":"MPB0000017","piece":"Bottom / Box"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'AKK543',
    @t10,
    '{"rack_number":"AKK543","customer":"John Deere"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'AKK46582',
    @t10,
    '{"rack_number":"AKK46582","customer":"John Deere"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'AKK46543',
    @t10,
    '{"rack_number":"AKK46543","customer":"John Deere"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'AKK546',
    @t10,
    '{"rack_number":"AKK546","customer":"John Deere"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'AKK419925',
    @t10,
    '{"rack_number":"AKK419925","customer":"John Deere"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Rack Daimler Jackies',
    @t11,
    '{"customer":"Daimler","owner":"Customer","style":"Jackies"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Rack Crenlo',
    @t11,
    '{"customer":"Crenlo","owner":"Customer"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Crate MTM',
    @t11,
    '{"owner":"MTM"}',
    NULL,
    'Quantity',
    'Outside Door 5',
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Kawasaki 14x12x7',
    @t12,
    '{"length":"14","width":"12","height":"7","customer":"Kawasaki"}',
    NULL,
    'Quantity',
    'WC',
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Kawasaki 15x12x7',
    @t12,
    '{"length":"15","width":"12","height":"7","customer":"Kawasaki"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Kawasaki 24x14x7',
    @t12,
    '{"length":"24","width":"14","height":"7","customer":"Kawasaki"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Allison',
    @t12,
    '{"customer":"Allison"}',
    NULL,
    'Quantity',
    'RECV',
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Ariens',
    @t12,
    '{"customer":"Ariens"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Lennox Tote',
    @t12,
    '{"customer":"Lennox"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'ZF',
    @t12,
    '{"customer":"ZF"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Basket Insinkerator Short',
    @t13,
    '{"customer":"Insinkerator","height_type":"Short"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Basket Insinkerator Tall',
    @t13,
    '{"customer":"Insinkerator","height_type":"Tall"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Crate Kohler Short',
    @t13,
    '{"customer":"Kohler","height_type":"Short"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Crate Kohler Tall',
    @t13,
    '{"customer":"Kohler","height_type":"Tall"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Crate Kohler Half',
    @t13,
    '{"customer":"Kohler","height_type":"Half"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Jarkies Lennox',
    @t13,
    '{"customer":"Lennox","style":"Jarkies"}',
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

-- =============================================
-- Step 3 — dunnage_specs (per-type spec templates)
-- Derived from the spec_values JSON used by the seeded dunnage_parts rows.
-- =============================================

-- Pallets / Skids
CALL sp_Dunnage_Specs_Insert(@t1, 'dimensions', '{"dataType":"Text","required":false,"defaultValue":"","unit":"","choices":[]}', 'seed', @id);
CALL sp_Dunnage_Specs_Insert(@t1, 'customer', '{"dataType":"Text","required":false,"defaultValue":"","unit":"","choices":[]}', 'seed', @id);

-- Cardboard Sheets / Slip Sheets
CALL sp_Dunnage_Specs_Insert(@t2, 'dimensions', '{"dataType":"Text","required":false,"defaultValue":"","unit":"","choices":[]}', 'seed', @id);
CALL sp_Dunnage_Specs_Insert(@t2, 'customer', '{"dataType":"Text","required":false,"defaultValue":"","unit":"","choices":[]}', 'seed', @id);

-- Corrugated Boxes
CALL sp_Dunnage_Specs_Insert(@t3, 'length', '{"dataType":"Number","required":false,"defaultValue":"","unit":"in","choices":[]}', 'seed', @id);
CALL sp_Dunnage_Specs_Insert(@t3, 'width', '{"dataType":"Number","required":false,"defaultValue":"","unit":"in","choices":[]}', 'seed', @id);
CALL sp_Dunnage_Specs_Insert(@t3, 'height', '{"dataType":"Number","required":false,"defaultValue":"","unit":"in","choices":[]}', 'seed', @id);
CALL sp_Dunnage_Specs_Insert(@t3, 'wall_type', '{"dataType":"Choices","required":false,"defaultValue":"","unit":"","choices":["Single Wall","Double Wall"]}', 'seed', @id);
CALL sp_Dunnage_Specs_Insert(@t3, 'customer', '{"dataType":"Text","required":false,"defaultValue":"","unit":"","choices":[]}', 'seed', @id);
CALL sp_Dunnage_Specs_Insert(@t3, 'part_number', '{"dataType":"Text","required":false,"defaultValue":"","unit":"","choices":[]}', 'seed', @id);

-- Gaylords / Bulk Bins
CALL sp_Dunnage_Specs_Insert(@t4, 'height_type', '{"dataType":"Choices","required":false,"defaultValue":"","unit":"","choices":["Short","Tall"]}', 'seed', @id);

-- Stretch Film / Shrink Wrap
CALL sp_Dunnage_Specs_Insert(@t5, 'application', '{"dataType":"Choices","required":false,"defaultValue":"","unit":"","choices":["Hand-Held","Auto-Wrapper"]}', 'seed', @id);

-- Bags
CALL sp_Dunnage_Specs_Insert(@t6, 'length', '{"dataType":"Number","required":false,"defaultValue":"","unit":"in","choices":[]}', 'seed', @id);
CALL sp_Dunnage_Specs_Insert(@t6, 'width', '{"dataType":"Number","required":false,"defaultValue":"","unit":"in","choices":[]}', 'seed', @id);
CALL sp_Dunnage_Specs_Insert(@t6, 'mil', '{"dataType":"Number","required":false,"defaultValue":"","unit":"","choices":[]}', 'seed', @id);
CALL sp_Dunnage_Specs_Insert(@t6, 'style', '{"dataType":"Choices","required":false,"defaultValue":"","unit":"","choices":["Lay Flat","Gaylord"]}', 'seed', @id);

-- Tape / Strapping / Banding
CALL sp_Dunnage_Specs_Insert(@t7, 'width', '{"dataType":"Text","required":false,"defaultValue":"","unit":"","choices":[]}', 'seed', @id);
CALL sp_Dunnage_Specs_Insert(@t7, 'material', '{"dataType":"Choices","required":false,"defaultValue":"","unit":"","choices":["Tape","Steel","Nylon"]}', 'seed', @id);
CALL sp_Dunnage_Specs_Insert(@t7, 'style', '{"dataType":"Choices","required":false,"defaultValue":"","unit":"","choices":["Banding","Strapping"]}', 'seed', @id);

-- Edge Protectors
CALL sp_Dunnage_Specs_Insert(@t8, 'width', '{"dataType":"Number","required":false,"defaultValue":"","unit":"in","choices":[]}', 'seed', @id);
CALL sp_Dunnage_Specs_Insert(@t8, 'depth', '{"dataType":"Number","required":false,"defaultValue":"","unit":"in","choices":[]}', 'seed', @id);
CALL sp_Dunnage_Specs_Insert(@t8, 'length', '{"dataType":"Number","required":false,"defaultValue":"","unit":"in","choices":[]}', 'seed', @id);

-- Foam / Molded Inserts
CALL sp_Dunnage_Specs_Insert(@t9, 'type', '{"dataType":"Text","required":false,"defaultValue":"","unit":"","choices":[]}', 'seed', @id);
CALL sp_Dunnage_Specs_Insert(@t9, 'variant', '{"dataType":"Number","required":false,"defaultValue":"","unit":"","choices":[]}', 'seed', @id);
CALL sp_Dunnage_Specs_Insert(@t9, 'part_family', '{"dataType":"Text","required":false,"defaultValue":"","unit":"","choices":[]}', 'seed', @id);
CALL sp_Dunnage_Specs_Insert(@t9, 'piece', '{"dataType":"Choices","required":false,"defaultValue":"","unit":"","choices":["Top / Cover","Bottom / Box"]}', 'seed', @id);

-- Returnable Racks - John Deere
CALL sp_Dunnage_Specs_Insert(@t10, 'rack_number', '{"dataType":"Text","required":false,"defaultValue":"","unit":"","choices":[]}', 'seed', @id);
CALL sp_Dunnage_Specs_Insert(@t10, 'customer', '{"dataType":"Text","required":false,"defaultValue":"","unit":"","choices":[]}', 'seed', @id);

-- Returnable Racks - Other
CALL sp_Dunnage_Specs_Insert(@t11, 'customer', '{"dataType":"Text","required":false,"defaultValue":"","unit":"","choices":[]}', 'seed', @id);
CALL sp_Dunnage_Specs_Insert(@t11, 'owner', '{"dataType":"Choices","required":false,"defaultValue":"","unit":"","choices":["Customer","MTM"]}', 'seed', @id);
CALL sp_Dunnage_Specs_Insert(@t11, 'style', '{"dataType":"Text","required":false,"defaultValue":"","unit":"","choices":[]}', 'seed', @id);

-- Returnable Totes
CALL sp_Dunnage_Specs_Insert(@t12, 'length', '{"dataType":"Number","required":false,"defaultValue":"","unit":"in","choices":[]}', 'seed', @id);
CALL sp_Dunnage_Specs_Insert(@t12, 'width', '{"dataType":"Number","required":false,"defaultValue":"","unit":"in","choices":[]}', 'seed', @id);
CALL sp_Dunnage_Specs_Insert(@t12, 'height', '{"dataType":"Number","required":false,"defaultValue":"","unit":"in","choices":[]}', 'seed', @id);
CALL sp_Dunnage_Specs_Insert(@t12, 'customer', '{"dataType":"Text","required":false,"defaultValue":"","unit":"","choices":[]}', 'seed', @id);

-- Returnable Baskets / Wire Containers
CALL sp_Dunnage_Specs_Insert(@t13, 'customer', '{"dataType":"Text","required":false,"defaultValue":"","unit":"","choices":[]}', 'seed', @id);
CALL sp_Dunnage_Specs_Insert(@t13, 'height_type', '{"dataType":"Choices","required":false,"defaultValue":"","unit":"","choices":["Short","Tall","Half"]}', 'seed', @id);
CALL sp_Dunnage_Specs_Insert(@t13, 'style', '{"dataType":"Text","required":false,"defaultValue":"","unit":"","choices":[]}', 'seed', @id);

-- =============================================
-- Step 4 — dunnage_requires_inventory
-- Returnable dunnage (types 10-13) is tracked in Visual ERP via "Adjust In".
-- Adjust this list to match site inventory policy.
-- =============================================
CALL sp_Dunnage_Inventory_Insert('AKK543', 'Adjust In', NULL, 'seed', @id);
CALL sp_Dunnage_Inventory_Insert('AKK46582', 'Adjust In', NULL, 'seed', @id);
CALL sp_Dunnage_Inventory_Insert('AKK46543', 'Adjust In', NULL, 'seed', @id);
CALL sp_Dunnage_Inventory_Insert('AKK546', 'Adjust In', NULL, 'seed', @id);
CALL sp_Dunnage_Inventory_Insert('AKK419925', 'Adjust In', NULL, 'seed', @id);
CALL sp_Dunnage_Inventory_Insert('Rack Daimler Jackies', 'Adjust In', NULL, 'seed', @id);
CALL sp_Dunnage_Inventory_Insert('Rack Crenlo', 'Adjust In', NULL, 'seed', @id);
CALL sp_Dunnage_Inventory_Insert('Crate MTM', 'Adjust In', NULL, 'seed', @id);
CALL sp_Dunnage_Inventory_Insert('Kawasaki 14x12x7', 'Adjust In', NULL, 'seed', @id);
CALL sp_Dunnage_Inventory_Insert('Kawasaki 15x12x7', 'Adjust In', NULL, 'seed', @id);
CALL sp_Dunnage_Inventory_Insert('Kawasaki 24x14x7', 'Adjust In', NULL, 'seed', @id);
CALL sp_Dunnage_Inventory_Insert('Allison', 'Adjust In', NULL, 'seed', @id);
CALL sp_Dunnage_Inventory_Insert('Ariens', 'Adjust In', NULL, 'seed', @id);
CALL sp_Dunnage_Inventory_Insert('Lennox Tote', 'Adjust In', NULL, 'seed', @id);
CALL sp_Dunnage_Inventory_Insert('ZF', 'Adjust In', NULL, 'seed', @id);
CALL sp_Dunnage_Inventory_Insert('Basket Insinkerator Short', 'Adjust In', NULL, 'seed', @id);
CALL sp_Dunnage_Inventory_Insert('Basket Insinkerator Tall', 'Adjust In', NULL, 'seed', @id);
CALL sp_Dunnage_Inventory_Insert('Crate Kohler Short', 'Adjust In', NULL, 'seed', @id);
CALL sp_Dunnage_Inventory_Insert('Crate Kohler Tall', 'Adjust In', NULL, 'seed', @id);
CALL sp_Dunnage_Inventory_Insert('Crate Kohler Half', 'Adjust In', NULL, 'seed', @id);
CALL sp_Dunnage_Inventory_Insert('Jarkies Lennox', 'Adjust In', NULL, 'seed', @id);

-- =============================================
-- Step 5 — dunnage_non_po_entries (reusable non-PO reference reasons)
-- =============================================
CALL sp_Dunnage_NonPO_Upsert('No PO Required', 'seed');
CALL sp_Dunnage_NonPO_Upsert('Internal Use', 'seed');
CALL sp_Dunnage_NonPO_Upsert('Maintenance', 'seed');
CALL sp_Dunnage_NonPO_Upsert('Sample / Demo', 'seed');
CALL sp_Dunnage_NonPO_Upsert('Warranty Return', 'seed');
CALL sp_Dunnage_NonPO_Upsert('Scrap / Disposal', 'seed');

COMMIT;

SELECT
    'dunnage_types' AS `table`,
    COUNT(*) AS `rows_inserted`
FROM
    dunnage_types
UNION
ALL
SELECT
    'dunnage_parts' AS `table`,
    COUNT(*) AS `rows_inserted`
FROM
    dunnage_parts
UNION
ALL
SELECT
    'dunnage_specs' AS `table`,
    COUNT(*) AS `rows_inserted`
FROM
    dunnage_specs
UNION
ALL
SELECT
    'dunnage_requires_inventory' AS `table`,
    COUNT(*) AS `rows_inserted`
FROM
    dunnage_requires_inventory
UNION
ALL
SELECT
    'dunnage_non_po_entries' AS `table`,
    COUNT(*) AS `rows_inserted`
FROM
    dunnage_non_po_entries
UNION
ALL
SELECT
    'dunnage_quantity_types' AS `table`,
    COUNT(*) AS `rows_inserted`
FROM
    dunnage_quantity_types;