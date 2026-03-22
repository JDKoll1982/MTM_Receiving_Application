-- =============================================
-- Seed: Dunnage Master Data
-- Created  : 2026-03-19
-- Source   : docs/GoogleSheetsVersion/Dunnage-Migration-Data.md
-- =============================================
-- Inserts:
--   Step 1 — dunnage_types
--   Step 2 — dunnage_parts
-- =============================================
USE mtm_receiving_application;

START TRANSACTION;

SET
    @id = 0;

CALL sp_Dunnage_Types_Insert(
    'Pallets / Skids',
    'ShippingPallet',
    'seed',
    @id
);

SET
    @t1 = @id;

CALL sp_Dunnage_Types_Insert(
    'Cardboard Sheets / Slip Sheets',
    'Layers',
    'seed',
    @id
);

SET
    @t2 = @id;

CALL sp_Dunnage_Types_Insert(
    'Corrugated Boxes',
    'PackageVariantClosed',
    'seed',
    @id
);

SET
    @t3 = @id;

CALL sp_Dunnage_Types_Insert(
    'Gaylords / Bulk Bins',
    'PackageVariant',
    'seed',
    @id
);

SET
    @t4 = @id;

CALL sp_Dunnage_Types_Insert(
    'Stretch Film / Shrink Wrap',
    'Autorenew',
    'seed',
    @id
);

SET
    @t5 = @id;

CALL sp_Dunnage_Types_Insert(
    'Bags',
    'BagPersonal',
    'seed',
    @id
);

SET
    @t6 = @id;

CALL sp_Dunnage_Types_Insert(
    'Tape / Strapping / Banding',
    'Selection',
    'seed',
    @id
);

SET
    @t7 = @id;

CALL sp_Dunnage_Types_Insert(
    'Edge Protectors',
    'ShieldOutline',
    'seed',
    @id
);

SET
    @t8 = @id;

CALL sp_Dunnage_Types_Insert(
    'Foam / Molded Inserts',
    'LayersOutline',
    'seed',
    @id
);

SET
    @t9 = @id;

CALL sp_Dunnage_Types_Insert(
    'Returnable Racks - John Deere',
    'Warehouse',
    'seed',
    @id
);

SET
    @t10 = @id;

CALL sp_Dunnage_Types_Insert(
    'Returnable Racks - Other',
    'Forklift',
    'seed',
    @id
);

SET
    @t11 = @id;

CALL sp_Dunnage_Types_Insert(
    'Returnable Totes',
    'BoxVariantClosed',
    'seed',
    @id
);

SET
    @t12 = @id;

CALL sp_Dunnage_Types_Insert(
    'Returnable Baskets / Wire Containers',
    'BasketOutline',
    'seed',
    @id
);

SET
    @t13 = @id;

CALL sp_Dunnage_Parts_Insert(
    'Pallet 20x20',
    @t1,
    '{"dimensions":"20x20"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Pallet 32x30',
    @t1,
    '{"dimensions":"32x30"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Pallet 40x48',
    @t1,
    '{"dimensions":"40x48"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Pallet 42x42',
    @t1,
    '{"dimensions":"42x42"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Pallet 42x54',
    @t1,
    '{"dimensions":"42x54"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Pallet 45x48',
    @t1,
    '{"dimensions":"45x48"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Pallet 47x52',
    @t1,
    '{"dimensions":"47x52"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Pallet 48x48',
    @t1,
    '{"dimensions":"48x48"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Pallet 52x47',
    @t1,
    '{"dimensions":"52x47"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Pallet 54x42',
    @t1,
    '{"dimensions":"54x42"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Pallet 74x40',
    @t1,
    '{"dimensions":"74x40"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Pallet 85x40',
    @t1,
    '{"dimensions":"85x40"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Pallet 100x40',
    @t1,
    '{"dimensions":"100x40"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Pallet 40x48 John Deere',
    @t1,
    '{"dimensions":"40x48","customer":"John Deere"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Pallet Styberg',
    @t1,
    '{"customer":"Styberg"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Cardboard Sheet 16x38.5',
    @t2,
    '{"dimensions":"16x38.5"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Cardboard Sheet 39x39',
    @t2,
    '{"dimensions":"39x39"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Cardboard Sheet 40x48',
    @t2,
    '{"dimensions":"40x48"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Cardboard Sheet 47x52',
    @t2,
    '{"dimensions":"47x52"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Cardboard Sheet 48x40',
    @t2,
    '{"dimensions":"48x40"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Cardboard Sheet 52x47',
    @t2,
    '{"dimensions":"52x47"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Cardboard Sheet John Deere',
    @t2,
    '{"customer":"John Deere"}',
    'S-E Racking',
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Corrugated Box 8x5x5',
    @t3,
    '{"length":"8","width":"5","height":"5"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Corrugated Box 12x12x6 Single Wall',
    @t3,
    '{"length":"12","width":"12","height":"6","wall_type":"Single Wall"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Corrugated Box 12x12x6 Double Wall',
    @t3,
    '{"length":"12","width":"12","height":"6","wall_type":"Double Wall"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Corrugated Box 12x12x12 Single Wall',
    @t3,
    '{"length":"12","width":"12","height":"12","wall_type":"Single Wall"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Corrugated Box 12x12x12 Double Wall',
    @t3,
    '{"length":"12","width":"12","height":"12","wall_type":"Double Wall"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Corrugated Box 16x12x8',
    @t3,
    '{"length":"16","width":"12","height":"8"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Corrugated Box 24x16x8 Volvo',
    @t3,
    '{"length":"24","width":"16","height":"8","customer":"Volvo"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'MPB0000005',
    @t3,
    '{"length":"25","width":"16","height":"18","part_number":"MPB0000005"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Corrugated Box 26x26x26',
    @t3,
    '{"length":"26","width":"26","height":"26"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Corrugated Box 28x16x17',
    @t3,
    '{"length":"28","width":"16","height":"17"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Corrugated Box 32x30x15 Electrolux',
    @t3,
    '{"length":"32","width":"30","height":"15","customer":"Electrolux"}',
    'Floor - In front of V-N Racking',
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Corrugated Box 32x30x25 Electrolux',
    @t3,
    '{"length":"32","width":"30","height":"25","customer":"Electrolux"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Corrugated Box 42x24x30',
    @t3,
    '{"length":"42","width":"24","height":"30"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Corrugated Box Lennox',
    @t3,
    '{"customer":"Lennox"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Gaylord Short',
    @t4,
    '{"height_type":"Short"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Gaylord Tall',
    @t4,
    '{"height_type":"Tall"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Stretch Film Hand-Held',
    @t5,
    '{"application":"Hand-Held"}',
    'T - Bay',
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Stretch Film Auto-Wrapper',
    @t5,
    '{"application":"Auto-Wrapper"}',
    'Rack by 100-15',
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Bag 40x48 Lay Flat',
    @t6,
    '{"length":"40","width":"48","mil":"4","style":"Lay Flat"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Bag 40x48 Gaylord',
    @t6,
    '{"length":"40","width":"48","mil":"4","style":"Gaylord"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Tape 2 Inch',
    @t7,
    '{"width":"2 inch","material":"Tape"}',
    'T - Bay',
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Banding 3/4 Inch Steel',
    @t7,
    '{"width":"3/4 inch","material":"Steel","style":"Banding"}',
    'T - Bay',
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Strapping 1-1/4 Inch Steel',
    @t7,
    '{"width":"1-1/4 inch","material":"Steel","style":"Strapping"}',
    'T - Bay',
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Banding Nylon',
    @t7,
    '{"material":"Nylon","style":"Banding"}',
    'T - Bay',
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Edge Protector 3x3x6',
    @t8,
    '{"width":"3","depth":"3","length":"6"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Genfoam 1',
    @t9,
    '{"type":"Genfoam","variant":"1"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Genfoam 2',
    @t9,
    '{"type":"Genfoam","variant":"2"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Genfoam 3',
    @t9,
    '{"type":"Genfoam","variant":"3"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Genfoam 4',
    @t9,
    '{"type":"Genfoam","variant":"4"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Gen Walls',
    @t9,
    '{"type":"Gen Walls"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'MPB0000017 Cover',
    @t9,
    '{"part_family":"MPB0000017","piece":"Top / Cover"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'MPB0000017 Box',
    @t9,
    '{"part_family":"MPB0000017","piece":"Bottom / Box"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Rack John Deere AKK543',
    @t10,
    '{"rack_number":"AKK543","customer":"John Deere"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Rack John Deere AKK46582',
    @t10,
    '{"rack_number":"AKK46582","customer":"John Deere"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Rack John Deere AKK46543',
    @t10,
    '{"rack_number":"AKK46543","customer":"John Deere"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Rack John Deere AKK546',
    @t10,
    '{"rack_number":"AKK546","customer":"John Deere"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Rack John Deere AKK419925',
    @t10,
    '{"rack_number":"AKK419925","customer":"John Deere"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Rack Daimler Jackies',
    @t11,
    '{"customer":"Daimler","owner":"Customer","style":"Jackies"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Rack Crenlo',
    @t11,
    '{"customer":"Crenlo","owner":"Customer"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Crate MTM',
    @t11,
    '{"owner":"MTM"}',
    'Outside Door 5',
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Tote Kawasaki 14x12x7',
    @t12,
    '{"length":"14","width":"12","height":"7","customer":"Kawasaki"}',
    'WC',
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Tote Kawasaki 15x12x7',
    @t12,
    '{"length":"15","width":"12","height":"7","customer":"Kawasaki"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Tote Kawasaki 24x14x7',
    @t12,
    '{"length":"24","width":"14","height":"7","customer":"Kawasaki"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Tote Allison',
    @t12,
    '{"customer":"Allison"}',
    'RECV',
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Tote Ariens',
    @t12,
    '{"customer":"Ariens"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Tote Lennox',
    @t12,
    '{"customer":"Lennox"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Tote ZF',
    @t12,
    '{"customer":"ZF"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Basket Insinkerator Short',
    @t13,
    '{"customer":"Insinkerator","height_type":"Short"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Basket Insinkerator Tall',
    @t13,
    '{"customer":"Insinkerator","height_type":"Tall"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Crate Kohler Short',
    @t13,
    '{"customer":"Kohler","height_type":"Short"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Crate Kohler Tall',
    @t13,
    '{"customer":"Kohler","height_type":"Tall"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Crate Kohler Half',
    @t13,
    '{"customer":"Kohler","height_type":"Half"}',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Jarkies Lennox',
    @t13,
    '{"customer":"Lennox","style":"Jarkies"}',
    NULL,
    'seed',
    @id
);

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
    'dunnage_requires_inventory' AS `table`,
    COUNT(*) AS `rows_inserted`
FROM
    dunnage_requires_inventory;