-- =============================================
-- Migration: Seed Dunnage Master Data
-- Created  : 2026-03-19
-- Source   : docs/GoogleSheetsVersion/Dunnage-Migration-Data.md
-- =============================================
-- Inserts:
--   Step 1 — dunnage_types              (13 rows)
--   Step 2 — dunnage_parts              (75 rows)
--   Step 3 — dunnage_requires_inventory  SKIPPED — use the app to flag each item
--
-- Run once on a clean database. Re-running will fail at Step 1
-- because sp_Dunnage_Types_Insert guards against duplicate type names.
-- =============================================

USE mtm_receiving_application;

-- -------------------------------------------------------
-- Idempotency note
-- -------------------------------------------------------
-- sp_Dunnage_Types_Insert guards against duplicate type names.
-- Re-running this script will SIGNAL on the first duplicate
-- type and the transaction will be rolled back automatically.
-- -------------------------------------------------------

START TRANSACTION;

-- ============================================================
-- STEP 1 — dunnage_types (13 rows)
-- Each CALL captures the new auto-increment id into @tN so
-- dunnage_parts can reference it by variable instead of by
-- hard-coded id.
-- ============================================================

SET @id = 0;

CALL sp_Dunnage_Types_Insert('Pallets / Skids',                     'ShippingPallet',        'migration', @id); SET @t1  = @id;
CALL sp_Dunnage_Types_Insert('Cardboard Sheets / Slip Sheets',      'Layers',                'migration', @id); SET @t2  = @id;
CALL sp_Dunnage_Types_Insert('Corrugated Boxes',                    'PackageVariantClosed',  'migration', @id); SET @t3  = @id;
CALL sp_Dunnage_Types_Insert('Gaylords / Bulk Bins',                'PackageVariant',        'migration', @id); SET @t4  = @id;
CALL sp_Dunnage_Types_Insert('Stretch Film / Shrink Wrap',          'Autorenew',             'migration', @id); SET @t5  = @id;
CALL sp_Dunnage_Types_Insert('Bags',                                'BagPersonal',           'migration', @id); SET @t6  = @id;
CALL sp_Dunnage_Types_Insert('Tape / Strapping / Banding',          'Selection',             'migration', @id); SET @t7  = @id;
CALL sp_Dunnage_Types_Insert('Edge Protectors',                     'ShieldOutline',         'migration', @id); SET @t8  = @id;
CALL sp_Dunnage_Types_Insert('Foam / Molded Inserts',               'LayersOutline',         'migration', @id); SET @t9  = @id;
CALL sp_Dunnage_Types_Insert('Returnable Racks – John Deere',    'Warehouse',             'migration', @id); SET @t10 = @id;
CALL sp_Dunnage_Types_Insert('Returnable Racks – Other',          'Forklift',              'migration', @id); SET @t11 = @id;
CALL sp_Dunnage_Types_Insert('Returnable Totes',                    'BoxVariantClosed',      'migration', @id); SET @t12 = @id;
CALL sp_Dunnage_Types_Insert('Returnable Baskets / Wire Containers','BasketOutline',         'migration', @id); SET @t13 = @id;

-- ============================================================
-- STEP 2 — dunnage_parts (75 rows)
-- home_location is NULL for all *(TBD)* entries.
-- Signature: (p_part_id, p_type_id, p_spec_values, p_home_location, p_user, OUT p_new_id)
-- ============================================================

-- T1 — Pallets / Skids (rows 1–15) --------------------------

CALL sp_Dunnage_Parts_Insert('Pallet 20x20',               @t1, '{"dimensions":"20x20"}',                               NULL,                               'migration', @id);
CALL sp_Dunnage_Parts_Insert('Pallet 32x30',               @t1, '{"dimensions":"32x30"}',                               NULL,                               'migration', @id);
CALL sp_Dunnage_Parts_Insert('Pallet 40x48',               @t1, '{"dimensions":"40x48"}',                               NULL,                               'migration', @id);
CALL sp_Dunnage_Parts_Insert('Pallet 42x42',               @t1, '{"dimensions":"42x42"}',                               NULL,                               'migration', @id);
CALL sp_Dunnage_Parts_Insert('Pallet 42x54',               @t1, '{"dimensions":"42x54"}',                               NULL,                               'migration', @id);
CALL sp_Dunnage_Parts_Insert('Pallet 45x48',               @t1, '{"dimensions":"45x48"}',                               NULL,                               'migration', @id);
CALL sp_Dunnage_Parts_Insert('Pallet 47x52',               @t1, '{"dimensions":"47x52"}',                               NULL,                               'migration', @id);
CALL sp_Dunnage_Parts_Insert('Pallet 48x48',               @t1, '{"dimensions":"48x48"}',                               NULL,                               'migration', @id);
CALL sp_Dunnage_Parts_Insert('Pallet 52x47',               @t1, '{"dimensions":"52x47"}',                               NULL,                               'migration', @id);
CALL sp_Dunnage_Parts_Insert('Pallet 54x42',               @t1, '{"dimensions":"54x42"}',                               NULL,                               'migration', @id);
CALL sp_Dunnage_Parts_Insert('Pallet 74x40',               @t1, '{"dimensions":"74x40"}',                               NULL,                               'migration', @id);
CALL sp_Dunnage_Parts_Insert('Pallet 85x40',               @t1, '{"dimensions":"85x40"}',                               NULL,                               'migration', @id);
CALL sp_Dunnage_Parts_Insert('Pallet 100x40',              @t1, '{"dimensions":"100x40"}',                              NULL,                               'migration', @id);
CALL sp_Dunnage_Parts_Insert('Pallet 40x48 John Deere',    @t1, '{"dimensions":"40x48","customer":"John Deere"}',       NULL,                               'migration', @id);
CALL sp_Dunnage_Parts_Insert('Pallet Styberg',             @t1, '{"customer":"Styberg"}',                               NULL,                               'migration', @id);

-- T2 — Cardboard Sheets / Slip Sheets (rows 16–22) -----------

CALL sp_Dunnage_Parts_Insert('Cardboard Sheet 16x38.5',      @t2, '{"dimensions":"16x38.5"}',                           NULL,                               'migration', @id);
CALL sp_Dunnage_Parts_Insert('Cardboard Sheet 39x39',        @t2, '{"dimensions":"39x39"}',                             NULL,                               'migration', @id);
CALL sp_Dunnage_Parts_Insert('Cardboard Sheet 40x48',        @t2, '{"dimensions":"40x48"}',                             NULL,                               'migration', @id);
CALL sp_Dunnage_Parts_Insert('Cardboard Sheet 47x52',        @t2, '{"dimensions":"47x52"}',                             NULL,                               'migration', @id);
CALL sp_Dunnage_Parts_Insert('Cardboard Sheet 48x40',        @t2, '{"dimensions":"48x40"}',                             NULL,                               'migration', @id);
CALL sp_Dunnage_Parts_Insert('Cardboard Sheet 52x47',        @t2, '{"dimensions":"52x47"}',                             NULL,                               'migration', @id);
CALL sp_Dunnage_Parts_Insert('Cardboard Sheet John Deere',   @t2, '{"customer":"John Deere"}',                          'S-E Racking',                      'migration', @id);

-- T3 — Corrugated Boxes (rows 23–36) -------------------------

CALL sp_Dunnage_Parts_Insert('Corrugated Box 8x5x5',                   @t3, '{"length":"8","width":"5","height":"5"}',                                          NULL,                                    'migration', @id);
CALL sp_Dunnage_Parts_Insert('Corrugated Box 12x12x6 Single Wall',     @t3, '{"length":"12","width":"12","height":"6","wall_type":"Single Wall"}',               NULL,                                    'migration', @id);
CALL sp_Dunnage_Parts_Insert('Corrugated Box 12x12x6 Double Wall',     @t3, '{"length":"12","width":"12","height":"6","wall_type":"Double Wall"}',               NULL,                                    'migration', @id);
CALL sp_Dunnage_Parts_Insert('Corrugated Box 12x12x12 Single Wall',    @t3, '{"length":"12","width":"12","height":"12","wall_type":"Single Wall"}',              NULL,                                    'migration', @id);
CALL sp_Dunnage_Parts_Insert('Corrugated Box 12x12x12 Double Wall',    @t3, '{"length":"12","width":"12","height":"12","wall_type":"Double Wall"}',              NULL,                                    'migration', @id);
CALL sp_Dunnage_Parts_Insert('Corrugated Box 16x12x8',                 @t3, '{"length":"16","width":"12","height":"8"}',                                         NULL,                                    'migration', @id);
CALL sp_Dunnage_Parts_Insert('Corrugated Box 24x16x8 Volvo',           @t3, '{"length":"24","width":"16","height":"8","customer":"Volvo"}',                      NULL,                                    'migration', @id);
CALL sp_Dunnage_Parts_Insert('MPB0000005',                             @t3, '{"length":"25","width":"16","height":"18","part_number":"MPB0000005"}',             NULL,                                    'migration', @id);
CALL sp_Dunnage_Parts_Insert('Corrugated Box 26x26x26',                @t3, '{"length":"26","width":"26","height":"26"}',                                        NULL,                                    'migration', @id);
CALL sp_Dunnage_Parts_Insert('Corrugated Box 28x16x17',                @t3, '{"length":"28","width":"16","height":"17"}',                                        NULL,                                    'migration', @id);
CALL sp_Dunnage_Parts_Insert('Corrugated Box 32x30x15 Electrolux',     @t3, '{"length":"32","width":"30","height":"15","customer":"Electrolux"}',                'Floor - In front of V-N Racking',       'migration', @id);
CALL sp_Dunnage_Parts_Insert('Corrugated Box 32x30x25 Electrolux',     @t3, '{"length":"32","width":"30","height":"25","customer":"Electrolux"}',                NULL,                                    'migration', @id);
CALL sp_Dunnage_Parts_Insert('Corrugated Box 42x24x30',                @t3, '{"length":"42","width":"24","height":"30"}',                                        NULL,                                    'migration', @id);
CALL sp_Dunnage_Parts_Insert('Corrugated Box Lennox',                  @t3, '{"customer":"Lennox"}',                                                            NULL,                                    'migration', @id);

-- T4 — Gaylords / Bulk Bins (rows 37–38) ---------------------

CALL sp_Dunnage_Parts_Insert('Gaylord Short', @t4, '{"height_type":"Short"}', NULL, 'migration', @id);
CALL sp_Dunnage_Parts_Insert('Gaylord Tall',  @t4, '{"height_type":"Tall"}',  NULL, 'migration', @id);

-- T5 — Stretch Film / Shrink Wrap (rows 39–40) ---------------

CALL sp_Dunnage_Parts_Insert('Stretch Film Hand-Held',    @t5, '{"application":"Hand-Held"}',    'T - Bay',           'migration', @id);
CALL sp_Dunnage_Parts_Insert('Stretch Film Auto-Wrapper', @t5, '{"application":"Auto-Wrapper"}', 'Rack by 100-15',    'migration', @id);

-- T6 — Bags (rows 41–42) -------------------------------------

CALL sp_Dunnage_Parts_Insert('Bag 40x48 Lay Flat', @t6, '{"length":"40","width":"48","mil":"4","style":"Lay Flat"}', NULL, 'migration', @id);
CALL sp_Dunnage_Parts_Insert('Bag 40x48 Gaylord',  @t6, '{"length":"40","width":"48","mil":"4","style":"Gaylord"}',  NULL, 'migration', @id);

-- T7 — Tape / Strapping / Banding (rows 43–46) ---------------

CALL sp_Dunnage_Parts_Insert('Tape 2 Inch',              @t7, '{"width":"2 inch","material":"Tape"}',                        'T - Bay', 'migration', @id);
CALL sp_Dunnage_Parts_Insert('Banding 3/4 Inch Steel',   @t7, '{"width":"3/4 inch","material":"Steel","style":"Banding"}',   'T - Bay', 'migration', @id);
CALL sp_Dunnage_Parts_Insert('Strapping 1-1/4 Inch Steel',@t7,'{"width":"1-1/4 inch","material":"Steel","style":"Strapping"}','T - Bay', 'migration', @id);
CALL sp_Dunnage_Parts_Insert('Banding Nylon',            @t7, '{"material":"Nylon","style":"Banding"}',                      'T - Bay', 'migration', @id);

-- T8 — Edge Protectors (row 47) ------------------------------

CALL sp_Dunnage_Parts_Insert('Edge Protector 3x3x6', @t8, '{"width":"3","depth":"3","length":"6"}', NULL, 'migration', @id);

-- T9 — Foam / Molded Inserts (rows 48–54) --------------------

CALL sp_Dunnage_Parts_Insert('Genfoam 1',        @t9, '{"type":"Genfoam","variant":"1"}',                           NULL, 'migration', @id);
CALL sp_Dunnage_Parts_Insert('Genfoam 2',        @t9, '{"type":"Genfoam","variant":"2"}',                           NULL, 'migration', @id);
CALL sp_Dunnage_Parts_Insert('Genfoam 3',        @t9, '{"type":"Genfoam","variant":"3"}',                           NULL, 'migration', @id);
CALL sp_Dunnage_Parts_Insert('Genfoam 4',        @t9, '{"type":"Genfoam","variant":"4"}',                           NULL, 'migration', @id);
CALL sp_Dunnage_Parts_Insert('Gen Walls',        @t9, '{"type":"Gen Walls"}',                                       NULL, 'migration', @id);
CALL sp_Dunnage_Parts_Insert('MPB0000017 Cover', @t9, '{"part_family":"MPB0000017","piece":"Top / Cover"}',          NULL, 'migration', @id);
CALL sp_Dunnage_Parts_Insert('MPB0000017 Box',   @t9, '{"part_family":"MPB0000017","piece":"Bottom / Box"}',         NULL, 'migration', @id);

-- T10 — Returnable Racks – John Deere (rows 55–59) -----------

CALL sp_Dunnage_Parts_Insert('Rack John Deere AKK543',    @t10, '{"rack_number":"AKK543","customer":"John Deere"}',    NULL, 'migration', @id);
CALL sp_Dunnage_Parts_Insert('Rack John Deere AKK46582',  @t10, '{"rack_number":"AKK46582","customer":"John Deere"}',  NULL, 'migration', @id);
CALL sp_Dunnage_Parts_Insert('Rack John Deere AKK46543',  @t10, '{"rack_number":"AKK46543","customer":"John Deere"}',  NULL, 'migration', @id);
CALL sp_Dunnage_Parts_Insert('Rack John Deere AKK546',    @t10, '{"rack_number":"AKK546","customer":"John Deere"}',    NULL, 'migration', @id);
CALL sp_Dunnage_Parts_Insert('Rack John Deere AKK419925', @t10, '{"rack_number":"AKK419925","customer":"John Deere"}', NULL, 'migration', @id);

-- T11 — Returnable Racks – Other (rows 60–62) ----------------

CALL sp_Dunnage_Parts_Insert('Rack Daimler Jackies', @t11, '{"customer":"Daimler","owner":"Customer","style":"Jackies"}', NULL,              'migration', @id);
CALL sp_Dunnage_Parts_Insert('Rack Crenlo',          @t11, '{"customer":"Crenlo","owner":"Customer"}',                   NULL,              'migration', @id);
CALL sp_Dunnage_Parts_Insert('Crate MTM',            @t11, '{"owner":"MTM"}',                                           'Outside Door 5',  'migration', @id);

-- T12 — Returnable Totes (rows 63–69) ------------------------

CALL sp_Dunnage_Parts_Insert('Tote Kawasaki 14x12x7', @t12, '{"length":"14","width":"12","height":"7","customer":"Kawasaki"}', 'WC',  'migration', @id);
CALL sp_Dunnage_Parts_Insert('Tote Kawasaki 15x12x7', @t12, '{"length":"15","width":"12","height":"7","customer":"Kawasaki"}', NULL,  'migration', @id);
CALL sp_Dunnage_Parts_Insert('Tote Kawasaki 24x14x7', @t12, '{"length":"24","width":"14","height":"7","customer":"Kawasaki"}', NULL,  'migration', @id);
CALL sp_Dunnage_Parts_Insert('Tote Allison',          @t12, '{"customer":"Allison"}',                                        'RECV', 'migration', @id);
CALL sp_Dunnage_Parts_Insert('Tote Ariens',           @t12, '{"customer":"Ariens"}',                                         NULL,  'migration', @id);
CALL sp_Dunnage_Parts_Insert('Tote Lennox',           @t12, '{"customer":"Lennox"}',                                         NULL,  'migration', @id);
CALL sp_Dunnage_Parts_Insert('Tote ZF',               @t12, '{"customer":"ZF"}',                                            NULL,  'migration', @id);

-- T13 — Returnable Baskets / Wire Containers (rows 70–75) ----

CALL sp_Dunnage_Parts_Insert('Basket Insinkerator Short', @t13, '{"customer":"Insinkerator","height_type":"Short"}', NULL, 'migration', @id);
CALL sp_Dunnage_Parts_Insert('Basket Insinkerator Tall',  @t13, '{"customer":"Insinkerator","height_type":"Tall"}',  NULL, 'migration', @id);
CALL sp_Dunnage_Parts_Insert('Crate Kohler Short',        @t13, '{"customer":"Kohler","height_type":"Short"}',       NULL, 'migration', @id);
CALL sp_Dunnage_Parts_Insert('Crate Kohler Tall',         @t13, '{"customer":"Kohler","height_type":"Tall"}',        NULL, 'migration', @id);
CALL sp_Dunnage_Parts_Insert('Crate Kohler Half',         @t13, '{"customer":"Kohler","height_type":"Half"}',        NULL, 'migration', @id);
CALL sp_Dunnage_Parts_Insert('Jarkies Lennox',            @t13, '{"customer":"Lennox","style":"Jarkies"}',           NULL, 'migration', @id);

-- ============================================================
-- STEP 3 — dunnage_requires_inventory
-- INTENTIONALLY SKIPPED — use the Dunnage Admin screen in the
-- app to flag the 24 inventory-tracked parts individually.
-- ============================================================

COMMIT;

-- ============================================================
-- Verification — review counts before leaving this session
-- ============================================================

SELECT 'dunnage_types'              AS `table`, COUNT(*) AS `rows_inserted` FROM dunnage_types
UNION ALL
SELECT 'dunnage_parts'              AS `table`, COUNT(*) AS `rows_inserted` FROM dunnage_parts
UNION ALL
SELECT 'dunnage_requires_inventory' AS `table`, COUNT(*) AS `rows_inserted` FROM dunnage_requires_inventory;
