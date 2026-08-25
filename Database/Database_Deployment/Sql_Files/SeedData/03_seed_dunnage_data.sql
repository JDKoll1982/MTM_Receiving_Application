-- =============================================
-- Seed: Dunnage Master Data
-- Created  : 2026-03-19
-- Updated  : 2026-08-25
-- Source   : docs/GoogleSheetsVersion/Dunnage-Migration-Data.md
-- =============================================
-- Inserts:
--   Step 1 — dunnage_types
--   Step 2 — dunnage_parts (udc1..udc10 values mapped from the former spec_values JSON)
--   Step 3 — dunnage_custom_fields + dunnage_custom_field_choices (UDC slot definitions)
--   Step 4 — dunnage_requires_inventory (returnable dunnage tracked in Visual ERP)
--   Step 5 — dunnage_non_po_entries (reusable non-PO reference reasons)
--   Note   — dunnage_quantity_types is seeded by schema file 40_Table_dunnage_quantity_types.sql
-- =============================================
USE mtm_receiving_application_test;

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

--   Step 2 - dunnage_parts (UDC values mapped from spec_values by type field order)
CALL sp_Dunnage_Parts_Insert(
    '20x20',
    @t1,
    '20x20',
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '32x30',
    @t1,
    '32x30',
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '40x48',
    @t1,
    '40x48',
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '42x42',
    @t1,
    '42x42',
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '42x54',
    @t1,
    '42x54',
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '45x48',
    @t1,
    '45x48',
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '47x52',
    @t1,
    '47x52',
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '48x48',
    @t1,
    '48x48',
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '52x47',
    @t1,
    '52x47',
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '54x42',
    @t1,
    '54x42',
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '74x40',
    @t1,
    '74x40',
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '85x40',
    @t1,
    '85x40',
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '100x40',
    @t1,
    '100x40',
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '40x48 John Deere',
    @t1,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Styberg',
    @t1,
    NULL,
    'Styberg',
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '16x38.5',
    @t2,
    '16x38.5',
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '39x39',
    @t2,
    '39x39',
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '40x48 Sheet',
    @t2,
    '40x48',
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '47x52 Sheet',
    @t2,
    '47x52',
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '48x40',
    @t2,
    '48x40',
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '52x47 Sheet',
    @t2,
    '52x47',
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'John Deere',
    @t2,
    NULL,
    'John Deere',
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    'S-E Racking',
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '8x5x5',
    @t3,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '12x12x6 Single Wall',
    @t3,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '12x12x6 Double Wall',
    @t3,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '12x12x12 Single Wall',
    @t3,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '12x12x12 Double Wall',
    @t3,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '16x12x8',
    @t3,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '24x16x8 Volvo',
    @t3,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'MPB0000005',
    @t3,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '26x26x26',
    @t3,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '28x16x17',
    @t3,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '32x30x15 Electrolux',
    @t3,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    'Floor - In front of V-N Racking',
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '32x30x25 Electrolux',
    @t3,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '42x24x30',
    @t3,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Lennox Box',
    @t3,
    NULL,
    NULL,
    NULL,
    NULL,
    'Lennox',
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Short',
    @t4,
    'Short',
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Tall',
    @t4,
    'Tall',
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Hand-Held',
    @t5,
    'Hand-Held',
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    'T - Bay',
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Auto-Wrapper',
    @t5,
    'Auto-Wrapper',
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    'Rack by 100-15',
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '40x48 Lay Flat',
    @t6,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '40x48 Gaylord',
    @t6,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Tape 2 Inch',
    @t7,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    'T - Bay',
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Banding 3/4 Inch Steel',
    @t7,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    'T - Bay',
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Strapping 1-1/4 Inch Steel',
    @t7,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    'T - Bay',
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Banding Nylon',
    @t7,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    'T - Bay',
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    '3x3x6',
    @t8,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Genfoam 1',
    @t9,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Genfoam 2',
    @t9,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Genfoam 3',
    @t9,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Genfoam 4',
    @t9,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Gen Walls',
    @t9,
    'Gen Walls',
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'MPB0000017 Cover',
    @t9,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'MPB0000017 Box',
    @t9,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'AKK543',
    @t10,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'AKK46582',
    @t10,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'AKK46543',
    @t10,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'AKK546',
    @t10,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'AKK419925',
    @t10,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Rack Daimler Jackies',
    @t11,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Rack Crenlo',
    @t11,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Crate MTM',
    @t11,
    NULL,
    'MTM',
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    'Outside Door 5',
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Kawasaki 14x12x7',
    @t12,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    'WC',
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Kawasaki 15x12x7',
    @t12,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Kawasaki 24x14x7',
    @t12,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Allison',
    @t12,
    NULL,
    NULL,
    NULL,
    'Allison',
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    'RECV',
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Ariens',
    @t12,
    NULL,
    NULL,
    NULL,
    'Ariens',
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Lennox Tote',
    @t12,
    NULL,
    NULL,
    NULL,
    'Lennox',
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'ZF',
    @t12,
    NULL,
    NULL,
    NULL,
    'ZF',
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Basket Insinkerator Short',
    @t13,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Basket Insinkerator Tall',
    @t13,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Crate Kohler Short',
    @t13,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Crate Kohler Tall',
    @t13,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Crate Kohler Half',
    @t13,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

CALL sp_Dunnage_Parts_Insert(
    'Jarkies Lennox',
    @t13,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    'Quantity',
    NULL,
    'seed',
    @id
);

--   Step 3 - dunnage_custom_fields + dunnage_custom_field_choices (UDC definitions)
CALL sp_Dunnage_CustomFields_Insert(@t1, 'Dimensions', 'Text', 1, 0, NULL, NULL, NULL, NULL, NULL, 'seed', @cfid, @cfs, @cfm);
CALL sp_Dunnage_CustomFields_Insert(@t1, 'Customer', 'Text', 2, 0, NULL, NULL, NULL, NULL, NULL, 'seed', @cfid, @cfs, @cfm);
CALL sp_Dunnage_CustomFields_Insert(@t2, 'Dimensions', 'Text', 1, 0, NULL, NULL, NULL, NULL, NULL, 'seed', @cfid, @cfs, @cfm);
CALL sp_Dunnage_CustomFields_Insert(@t2, 'Customer', 'Text', 2, 0, NULL, NULL, NULL, NULL, NULL, 'seed', @cfid, @cfs, @cfm);
CALL sp_Dunnage_CustomFields_Insert(@t3, 'Length', 'Number', 1, 0, 'in', NULL, NULL, NULL, NULL, 'seed', @cfid, @cfs, @cfm);
CALL sp_Dunnage_CustomFields_Insert(@t3, 'Width', 'Number', 2, 0, 'in', NULL, NULL, NULL, NULL, 'seed', @cfid, @cfs, @cfm);
CALL sp_Dunnage_CustomFields_Insert(@t3, 'Height', 'Number', 3, 0, 'in', NULL, NULL, NULL, NULL, 'seed', @cfid, @cfs, @cfm);
CALL sp_Dunnage_CustomFields_Insert(@t3, 'Wall Type', 'Choices', 4, 0, NULL, NULL, NULL, NULL, NULL, 'seed', @cfid, @cfs, @cfm);
CALL sp_Dunnage_CustomFieldChoices_Insert(@cfid, 'Single Wall', 1);
CALL sp_Dunnage_CustomFieldChoices_Insert(@cfid, 'Double Wall', 2);
CALL sp_Dunnage_CustomFields_Insert(@t3, 'Customer', 'Text', 5, 0, NULL, NULL, NULL, NULL, NULL, 'seed', @cfid, @cfs, @cfm);
CALL sp_Dunnage_CustomFields_Insert(@t3, 'Part Number', 'Text', 6, 0, NULL, NULL, NULL, NULL, NULL, 'seed', @cfid, @cfs, @cfm);
CALL sp_Dunnage_CustomFields_Insert(@t4, 'Height Type', 'Choices', 1, 0, NULL, NULL, NULL, NULL, NULL, 'seed', @cfid, @cfs, @cfm);
CALL sp_Dunnage_CustomFieldChoices_Insert(@cfid, 'Short', 1);
CALL sp_Dunnage_CustomFieldChoices_Insert(@cfid, 'Tall', 2);
CALL sp_Dunnage_CustomFields_Insert(@t5, 'Application', 'Choices', 1, 0, NULL, NULL, NULL, NULL, NULL, 'seed', @cfid, @cfs, @cfm);
CALL sp_Dunnage_CustomFieldChoices_Insert(@cfid, 'Hand-Held', 1);
CALL sp_Dunnage_CustomFieldChoices_Insert(@cfid, 'Auto-Wrapper', 2);
CALL sp_Dunnage_CustomFields_Insert(@t6, 'Length', 'Number', 1, 0, 'in', NULL, NULL, NULL, NULL, 'seed', @cfid, @cfs, @cfm);
CALL sp_Dunnage_CustomFields_Insert(@t6, 'Width', 'Number', 2, 0, 'in', NULL, NULL, NULL, NULL, 'seed', @cfid, @cfs, @cfm);
CALL sp_Dunnage_CustomFields_Insert(@t6, 'Mil', 'Number', 3, 0, NULL, NULL, NULL, NULL, NULL, 'seed', @cfid, @cfs, @cfm);
CALL sp_Dunnage_CustomFields_Insert(@t6, 'Style', 'Choices', 4, 0, NULL, NULL, NULL, NULL, NULL, 'seed', @cfid, @cfs, @cfm);
CALL sp_Dunnage_CustomFieldChoices_Insert(@cfid, 'Lay Flat', 1);
CALL sp_Dunnage_CustomFieldChoices_Insert(@cfid, 'Gaylord', 2);
CALL sp_Dunnage_CustomFields_Insert(@t7, 'Width', 'Text', 1, 0, NULL, NULL, NULL, NULL, NULL, 'seed', @cfid, @cfs, @cfm);
CALL sp_Dunnage_CustomFields_Insert(@t7, 'Material', 'Choices', 2, 0, NULL, NULL, NULL, NULL, NULL, 'seed', @cfid, @cfs, @cfm);
CALL sp_Dunnage_CustomFieldChoices_Insert(@cfid, 'Tape', 1);
CALL sp_Dunnage_CustomFieldChoices_Insert(@cfid, 'Steel', 2);
CALL sp_Dunnage_CustomFieldChoices_Insert(@cfid, 'Nylon', 3);
CALL sp_Dunnage_CustomFields_Insert(@t7, 'Style', 'Choices', 3, 0, NULL, NULL, NULL, NULL, NULL, 'seed', @cfid, @cfs, @cfm);
CALL sp_Dunnage_CustomFieldChoices_Insert(@cfid, 'Banding', 1);
CALL sp_Dunnage_CustomFieldChoices_Insert(@cfid, 'Strapping', 2);
CALL sp_Dunnage_CustomFields_Insert(@t8, 'Width', 'Number', 1, 0, 'in', NULL, NULL, NULL, NULL, 'seed', @cfid, @cfs, @cfm);
CALL sp_Dunnage_CustomFields_Insert(@t8, 'Depth', 'Number', 2, 0, 'in', NULL, NULL, NULL, NULL, 'seed', @cfid, @cfs, @cfm);
CALL sp_Dunnage_CustomFields_Insert(@t8, 'Length', 'Number', 3, 0, 'in', NULL, NULL, NULL, NULL, 'seed', @cfid, @cfs, @cfm);
CALL sp_Dunnage_CustomFields_Insert(@t9, 'Type', 'Text', 1, 0, NULL, NULL, NULL, NULL, NULL, 'seed', @cfid, @cfs, @cfm);
CALL sp_Dunnage_CustomFields_Insert(@t9, 'Variant', 'Number', 2, 0, NULL, NULL, NULL, NULL, NULL, 'seed', @cfid, @cfs, @cfm);
CALL sp_Dunnage_CustomFields_Insert(@t9, 'Part Family', 'Text', 3, 0, NULL, NULL, NULL, NULL, NULL, 'seed', @cfid, @cfs, @cfm);
CALL sp_Dunnage_CustomFields_Insert(@t9, 'Piece', 'Choices', 4, 0, NULL, NULL, NULL, NULL, NULL, 'seed', @cfid, @cfs, @cfm);
CALL sp_Dunnage_CustomFieldChoices_Insert(@cfid, 'Top / Cover', 1);
CALL sp_Dunnage_CustomFieldChoices_Insert(@cfid, 'Bottom / Box', 2);
CALL sp_Dunnage_CustomFields_Insert(@t10, 'Rack Number', 'Text', 1, 0, NULL, NULL, NULL, NULL, NULL, 'seed', @cfid, @cfs, @cfm);
CALL sp_Dunnage_CustomFields_Insert(@t10, 'Customer', 'Text', 2, 0, NULL, NULL, NULL, NULL, NULL, 'seed', @cfid, @cfs, @cfm);
CALL sp_Dunnage_CustomFields_Insert(@t11, 'Customer', 'Text', 1, 0, NULL, NULL, NULL, NULL, NULL, 'seed', @cfid, @cfs, @cfm);
CALL sp_Dunnage_CustomFields_Insert(@t11, 'Owner', 'Choices', 2, 0, NULL, NULL, NULL, NULL, NULL, 'seed', @cfid, @cfs, @cfm);
CALL sp_Dunnage_CustomFieldChoices_Insert(@cfid, 'Customer', 1);
CALL sp_Dunnage_CustomFieldChoices_Insert(@cfid, 'MTM', 2);
CALL sp_Dunnage_CustomFields_Insert(@t11, 'Style', 'Text', 3, 0, NULL, NULL, NULL, NULL, NULL, 'seed', @cfid, @cfs, @cfm);
CALL sp_Dunnage_CustomFields_Insert(@t12, 'Length', 'Number', 1, 0, 'in', NULL, NULL, NULL, NULL, 'seed', @cfid, @cfs, @cfm);
CALL sp_Dunnage_CustomFields_Insert(@t12, 'Width', 'Number', 2, 0, 'in', NULL, NULL, NULL, NULL, 'seed', @cfid, @cfs, @cfm);
CALL sp_Dunnage_CustomFields_Insert(@t12, 'Height', 'Number', 3, 0, 'in', NULL, NULL, NULL, NULL, 'seed', @cfid, @cfs, @cfm);
CALL sp_Dunnage_CustomFields_Insert(@t12, 'Customer', 'Text', 4, 0, NULL, NULL, NULL, NULL, NULL, 'seed', @cfid, @cfs, @cfm);
CALL sp_Dunnage_CustomFields_Insert(@t13, 'Customer', 'Text', 1, 0, NULL, NULL, NULL, NULL, NULL, 'seed', @cfid, @cfs, @cfm);
CALL sp_Dunnage_CustomFields_Insert(@t13, 'Height Type', 'Choices', 2, 0, NULL, NULL, NULL, NULL, NULL, 'seed', @cfid, @cfs, @cfm);
CALL sp_Dunnage_CustomFieldChoices_Insert(@cfid, 'Short', 1);
CALL sp_Dunnage_CustomFieldChoices_Insert(@cfid, 'Tall', 2);
CALL sp_Dunnage_CustomFieldChoices_Insert(@cfid, 'Half', 3);
CALL sp_Dunnage_CustomFields_Insert(@t13, 'Style', 'Text', 3, 0, NULL, NULL, NULL, NULL, NULL, 'seed', @cfid, @cfs, @cfm);

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
