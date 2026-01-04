-- Reporting Module Views
-- Purpose: Create views for end-of-day reporting across all modules
-- Date: 2026-01-04
-- Feature: 003-reporting-module

-- View 1: Receiving History
-- Purpose: Flattened view of receiving loads for reporting
CREATE OR REPLACE VIEW vw_receiving_history AS
SELECT 
    id,
    po_number,
    part_id as part_number,
    part_description,
    quantity as quantity,
    NULL as weight_lbs,  -- Not in original schema
    heat as heat_lot_number,
    transaction_date as created_date,
    employee_number,
    'Receiving' as source_module
FROM label_table_receiving
ORDER BY transaction_date DESC, id DESC;

-- View 2: Dunnage History
-- Purpose: Flattened view of dunnage loads for reporting
CREATE OR REPLACE VIEW vw_dunnage_history AS
SELECT 
    dl.load_uuid as id,
    dt.type_name as dunnage_type,
    dp.part_number,
    GROUP_CONCAT(
        CONCAT(ds.spec_key, ':', COALESCE(ds.spec_value, '')) 
        ORDER BY ds.display_order 
        SEPARATOR ', '
    ) as specs_combined,
    dl.quantity,
    DATE(dl.created_at) as created_date,
    dl.employee_number,
    'Dunnage' as source_module
FROM dunnage_loads dl
INNER JOIN dunnage_types dt ON dl.type_id = dt.type_id
INNER JOIN dunnage_parts dp ON dl.part_id = dp.part_id
LEFT JOIN dunnage_specs ds ON dp.part_id = ds.part_id
GROUP BY dl.load_uuid, dt.type_name, dp.part_number, dl.quantity, dl.created_at, dl.employee_number
ORDER BY dl.created_at DESC;

-- View 3: Routing History (from label_table_parcel)
-- Purpose: Flattened view of routing labels for reporting
-- Note: Using label_table_parcel as routing labels table
CREATE OR REPLACE VIEW vw_routing_history AS
SELECT 
    id,
    deliver_to,
    department,
    package_description,
    po_number,
    work_order_number,
    employee_number,
    transaction_date as created_date,
    'Routing' as source_module
FROM label_table_parcel
ORDER BY transaction_date DESC, label_number ASC;

-- View 4: Volvo History (placeholder - may not exist in current schema)
-- Purpose: Flattened view of Volvo shipments for reporting
-- Note: This table may not exist yet - create as placeholder
CREATE OR REPLACE VIEW vw_volvo_history AS
SELECT 
    NULL as id,
    NULL as shipment_number,
    NULL as po_number,
    NULL as receiver_number,
    NULL as status,
    NULL as created_date,
    NULL as part_count,
    NULL as employee_number,
    'Volvo' as source_module
WHERE 1=0;  -- Empty view - placeholder until Volvo module is implemented
