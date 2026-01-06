-- =====================================================
-- Migration: Add Test Shipment History Data
-- =====================================================
-- Purpose: Create multiple completed Volvo shipments for testing the history view
-- Database: mtm_receiving_application
-- Author: System Generated
-- Date: 2026-01-05
-- =====================================================

-- Insert completed shipments spanning different dates with various scenarios
-- Shipment 1: Completed shipment from 3 weeks ago - Standard parts, no discrepancies
INSERT INTO volvo_shipments (shipment_date, shipment_number, po_number, receiver_number, employee_number, notes, status, created_date)
VALUES 
('2025-12-15', 1, 'PO-2025-1234', 'RCV-001234', 'EMP001', 'First test shipment - all parts as expected', 'completed', '2025-12-15 08:30:00');

SET @shipment1_id = LAST_INSERT_ID();

INSERT INTO volvo_shipment_lines (shipment_id, part_number, received_skid_count, calculated_piece_count, has_discrepancy, expected_skid_count, discrepancy_note)
VALUES 
(@shipment1_id, 'V-EMB-500', 5, 440, 0, NULL, NULL),
(@shipment1_id, 'V-EMB-750', 3, 240, 0, NULL, NULL),
(@shipment1_id, 'V-EMB-116', 2, 300, 0, NULL, NULL);

-- Shipment 2: Completed shipment from 2 weeks ago - With discrepancies
INSERT INTO volvo_shipments (shipment_date, shipment_number, po_number, receiver_number, employee_number, notes, status, created_date)
VALUES 
('2025-12-22', 1, 'PO-2025-1456', 'RCV-001456', 'EMP002', 'Discrepancy on V-EMB-600 - received more than expected', 'completed', '2025-12-22 09:15:00');

SET @shipment2_id = LAST_INSERT_ID();

INSERT INTO volvo_shipment_lines (shipment_id, part_number, received_skid_count, calculated_piece_count, has_discrepancy, expected_skid_count, discrepancy_note)
VALUES 
(@shipment2_id, 'V-EMB-600', 8, 352, 1, 5, 'Packlist showed 5 skids but received 8 - called Volvo, they confirmed correct quantity'),
(@shipment2_id, 'V-EMB-21', 10, 500, 0, NULL, NULL),
(@shipment2_id, 'V-EMB-71', 6, 150, 0, NULL, NULL);

-- Shipment 3: Completed shipment from 10 days ago - Multiple discrepancies
INSERT INTO volvo_shipments (shipment_date, shipment_number, po_number, receiver_number, employee_number, notes, status, created_date)
VALUES 
('2025-12-26', 1, 'PO-2025-1523', 'RCV-001523', 'EMP001', 'Multiple discrepancies - shortage on frames', 'completed', '2025-12-26 10:45:00');

SET @shipment3_id = LAST_INSERT_ID();

INSERT INTO volvo_shipment_lines (shipment_id, part_number, received_skid_count, calculated_piece_count, has_discrepancy, expected_skid_count, discrepancy_note)
VALUES 
(@shipment3_id, 'V-EMB-61', 4, 480, 1, 5, 'Short 1 skid - damage noted on packing slip'),
(@shipment3_id, 'V-EMB-26', 8, 256, 1, 10, 'Short 2 skids - follow-up shipment requested'),
(@shipment3_id, 'V-EMB-800', 5, 200, 0, NULL, NULL);

-- Shipment 4: Completed shipment from 1 week ago - Large volume shipment
INSERT INTO volvo_shipments (shipment_date, shipment_number, po_number, receiver_number, employee_number, notes, status, created_date)
VALUES 
('2025-12-29', 1, 'PO-2025-1601', 'RCV-001601', 'EMP003', 'Large volume shipment for year-end inventory', 'completed', '2025-12-29 07:00:00');

SET @shipment4_id = LAST_INSERT_ID();

INSERT INTO volvo_shipment_lines (shipment_id, part_number, received_skid_count, calculated_piece_count, has_discrepancy, expected_skid_count, discrepancy_note)
VALUES 
(@shipment4_id, 'V-EMB-500', 12, 1056, 0, NULL, NULL),
(@shipment4_id, 'V-EMB-116', 8, 1200, 0, NULL, NULL),
(@shipment4_id, 'V-EMB-62', 6, 720, 0, NULL, NULL),
(@shipment4_id, 'V-EMB-750', 10, 800, 0, NULL, NULL),
(@shipment4_id, 'V-EMB-21', 15, 750, 0, NULL, NULL);

-- Shipment 5: Completed shipment from 5 days ago - Multiple shipments same day
INSERT INTO volvo_shipments (shipment_date, shipment_number, po_number, receiver_number, employee_number, notes, status, created_date)
VALUES 
('2025-12-31', 1, 'PO-2025-1678', 'RCV-001678', 'EMP002', 'First shipment of the day - morning delivery', 'completed', '2025-12-31 08:20:00');

SET @shipment5_id = LAST_INSERT_ID();

INSERT INTO volvo_shipment_lines (shipment_id, part_number, received_skid_count, calculated_piece_count, has_discrepancy, expected_skid_count, discrepancy_note)
VALUES 
(@shipment5_id, 'V-EMB-600', 4, 176, 0, NULL, NULL),
(@shipment5_id, 'V-EMB-71', 8, 200, 0, NULL, NULL);

-- Shipment 6: Completed shipment from 5 days ago - Second shipment same day
INSERT INTO volvo_shipments (shipment_date, shipment_number, po_number, receiver_number, employee_number, notes, status, created_date)
VALUES 
('2025-12-31', 2, 'PO-2025-1679', 'RCV-001679', 'EMP001', 'Second shipment of the day - afternoon delivery', 'completed', '2025-12-31 14:30:00');

SET @shipment6_id = LAST_INSERT_ID();

INSERT INTO volvo_shipment_lines (shipment_id, part_number, received_skid_count, calculated_piece_count, has_discrepancy, expected_skid_count, discrepancy_note)
VALUES 
(@shipment6_id, 'V-EMB-780', 3, 120, 0, NULL, NULL),
(@shipment6_id, 'V-EMB-91', 12, 300, 0, NULL, NULL);

-- Shipment 7: Completed shipment from 3 days ago - Overnight discrepancy
INSERT INTO volvo_shipments (shipment_date, shipment_number, po_number, receiver_number, employee_number, notes, status, created_date)
VALUES 
('2026-01-02', 1, 'PO-2026-0003', 'RCV-002003', 'EMP003', 'New year shipment - discrepancy found after overnight count', 'completed', '2026-01-02 09:00:00');

SET @shipment7_id = LAST_INSERT_ID();

INSERT INTO volvo_shipment_lines (shipment_id, part_number, received_skid_count, calculated_piece_count, has_discrepancy, expected_skid_count, discrepancy_note)
VALUES 
(@shipment7_id, 'V-EMB-66', 7, 700, 1, 8, 'One skid damaged during transit - credit requested'),
(@shipment7_id, 'V-EMB-29', 5, 160, 0, NULL, NULL),
(@shipment7_id, 'V-EMB-787', 4, 160, 0, NULL, NULL);

-- Shipment 8: Completed shipment from yesterday - Recent completion
INSERT INTO volvo_shipments (shipment_date, shipment_number, po_number, receiver_number, employee_number, notes, status, created_date)
VALUES 
('2026-01-04', 1, 'PO-2026-0012', 'RCV-002012', 'EMP002', 'Yesterday shipment - just completed', 'completed', '2026-01-04 10:30:00');

SET @shipment8_id = LAST_INSERT_ID();

INSERT INTO volvo_shipment_lines (shipment_id, part_number, received_skid_count, calculated_piece_count, has_discrepancy, expected_skid_count, discrepancy_note)
VALUES 
(@shipment8_id, 'V-EMB-500', 6, 528, 0, NULL, NULL),
(@shipment8_id, 'V-EMB-840', 3, 60, 0, NULL, NULL),
(@shipment8_id, 'V-EMB-757', 5, 400, 0, NULL, NULL);

-- Shipment 9: Completed shipment with mixed parts - testing component explosion
INSERT INTO volvo_shipments (shipment_date, shipment_number, po_number, receiver_number, employee_number, notes, status, created_date)
VALUES 
('2026-01-04', 2, 'PO-2026-0013', 'RCV-002013', 'EMP001', 'Second shipment yesterday - variety of parts', 'completed', '2026-01-04 15:45:00');

SET @shipment9_id = LAST_INSERT_ID();

INSERT INTO volvo_shipment_lines (shipment_id, part_number, received_skid_count, calculated_piece_count, has_discrepancy, expected_skid_count, discrepancy_note)
VALUES 
(@shipment9_id, 'V-EMB-702', 10, 200, 0, NULL, NULL),
(@shipment9_id, 'V-EMB-706', 8, 200, 0, NULL, NULL),
(@shipment9_id, 'V-EMB-69', 4, 400, 0, NULL, NULL);

-- Shipment 10: Pending PO shipment (not yet completed) - for comparison
INSERT INTO volvo_shipments (shipment_date, shipment_number, po_number, receiver_number, employee_number, notes, status, created_date)
VALUES 
('2026-01-05', 1, NULL, NULL, 'EMP002', 'Today shipment - waiting for PO number from purchasing', 'pending_po', '2026-01-05 09:00:00');

SET @shipment10_id = LAST_INSERT_ID();

INSERT INTO volvo_shipment_lines (shipment_id, part_number, received_skid_count, calculated_piece_count, has_discrepancy, expected_skid_count, discrepancy_note)
VALUES 
(@shipment10_id, 'V-EMB-116', 3, 450, 0, NULL, NULL),
(@shipment10_id, 'V-EMB-500', 4, 352, 1, 5, 'Short 1 skid - checking with Volvo');

-- Add summary comment
-- =====================================================
-- Migration Complete: 10 test shipments created
-- - 9 Completed shipments (various dates and scenarios)
-- - 1 Pending PO shipment (for status filter testing)
-- - Mix of shipments with and without discrepancies
-- - Multiple shipments on same date (testing sorting)
-- - Date range: Dec 15, 2025 to Jan 5, 2026
-- =====================================================
