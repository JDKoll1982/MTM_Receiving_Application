-- =====================================================
-- Migration 012: Add quantity_per_skid to volvo_shipment_lines
-- =====================================================
-- Purpose: Add quantity_per_skid column to preserve historical piece counts
-- This column stores the quantity_per_skid value at the time of shipment
-- to ensure accurate historical reporting even if master data changes
-- =====================================================

USE mtm_receiving_application;

-- Add quantity_per_skid column
ALTER TABLE volvo_shipment_lines
ADD COLUMN quantity_per_skid INT NOT NULL DEFAULT 0 COMMENT 'Cached quantity per skid from master data at time of shipment'
AFTER part_number;

-- Update existing rows with values from volvo_parts_master
UPDATE volvo_shipment_lines vsl
INNER JOIN volvo_parts_master vpm ON vsl.part_number = vpm.part_number
SET vsl.quantity_per_skid = vpm.quantity_per_skid;

-- Verify no rows have 0 (which would indicate missing master data)
SELECT COUNT(*) as rows_with_zero_qty
FROM volvo_shipment_lines
WHERE quantity_per_skid = 0;

