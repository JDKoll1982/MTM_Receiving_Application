-- ============================================================================
-- View: view_receiving_history
-- Module: Reporting
-- Purpose: Flattened view of receiving loads for reporting
-- ============================================================================

CREATE OR REPLACE VIEW view_receiving_history AS
SELECT
    LoadID AS id,
    PONumber AS po_number,
    PartID AS part_number,
    PartType AS part_description,
    WeightQuantity AS quantity,
    WeightPerPackage AS weight_lbs,
    HeatLotNumber AS heat_lot_number,
    DATE(ReceivedDate) AS created_date,
    NULL AS employee_number,
    'Receiving' AS source_module
FROM receiving_history
ORDER BY ReceivedDate DESC, LoadID DESC;

-- ============================================================================
