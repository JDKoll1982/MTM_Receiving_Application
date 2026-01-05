-- =====================================================
-- Volvo Module - Sample Test Data
-- =====================================================
-- Purpose: Initial master data from DataSheet.csv
-- Database: mtm_receiving_application
-- =====================================================

-- Sample data for volvo_parts_master (from DataSheet.csv)
-- All 30 parts from the Volvo Dunnage data sheet
INSERT INTO volvo_parts_master (part_number, description, quantity_per_skid) VALUES
('V-EMB-1', 'Base Skid Component', 10),
('V-EMB-2', 'Base Skid Component', 20),
('V-EMB-6', 'Base Skid Component', 10),
('V-EMB-9', 'Base Skid Component', 10),
('V-EMB-21', 'Dunnage with Skid', 50),
('V-EMB-22', 'Dunnage with Skid', 50),
('V-EMB-26', 'Frame Component', 32),
('V-EMB-29', 'Frame Component', 32),
('V-EMB-61', 'Dunnage with Skid/Lid/Frame', 120),
('V-EMB-62', 'Dunnage with Skid/Lid/Frame', 120),
('V-EMB-66', 'Dunnage with Skid/Lid/Frame', 100),
('V-EMB-69', 'Dunnage with Skid/Lid/Frame', 100),
('V-EMB-71', 'Lid Component', 25),
('V-EMB-72', 'Lid Component', 25),
('V-EMB-76', 'Lid Component', 25),
('V-EMB-79', 'Lid Component', 25),
('V-EMB-91', 'Lid Component', 25),
('V-EMB-92', 'Lid Component', 25),
('V-EMB-116', 'Dunnage with Skid/Lid/Frame', 150),
('V-EMB-500', 'Dunnage with Skid/Lid', 88),
('V-EMB-600', 'Dunnage with Skid/Lid', 44),
('V-EMB-701', 'Base Component', 10),
('V-EMB-702', 'Dunnage with Base', 20),
('V-EMB-706', 'Dunnage with Base', 25),
('V-EMB-750', 'Dunnage with Skid/Lid', 80),
('V-EMB-757', 'Dunnage with Skid/Lid', 80),
('V-EMB-780', 'Dunnage with Skid/Lid', 40),
('V-EMB-787', 'Dunnage with Skid/Lid', 40),
('V-EMB-800', 'Dunnage with Skid/Lid', 40),
('V-EMB-840', 'Dunnage with Skid/Lid', 20)
ON DUPLICATE KEY UPDATE quantity_per_skid = VALUES(quantity_per_skid);

-- Sample data for volvo_part_components (component explosion relationships)
-- All component relationships from the Volvo Dunnage data sheet
INSERT INTO volvo_part_components (parent_part_number, component_part_number, quantity) VALUES
-- V-EMB-116: Skid + Lid + Frame (3x)
('V-EMB-116', 'V-EMB-1', 1),
('V-EMB-116', 'V-EMB-71', 1),
('V-EMB-116', 'V-EMB-26', 3),
-- V-EMB-21: Skid (2x)
('V-EMB-21', 'V-EMB-1', 2),
-- V-EMB-22: Skid (2x)
('V-EMB-22', 'V-EMB-2', 2),
-- V-EMB-500: Skid + Lid
('V-EMB-500', 'V-EMB-2', 1),
('V-EMB-500', 'V-EMB-92', 1),
-- V-EMB-600: Skid + Lid
('V-EMB-600', 'V-EMB-2', 1),
('V-EMB-600', 'V-EMB-92', 1),
-- V-EMB-61: Skid + Lid + Frame (2x)
('V-EMB-61', 'V-EMB-1', 1),
('V-EMB-61', 'V-EMB-71', 1),
('V-EMB-61', 'V-EMB-21', 2),
-- V-EMB-62: Skid + Lid + Frame (2x)
('V-EMB-62', 'V-EMB-2', 1),
('V-EMB-62', 'V-EMB-72', 1),
('V-EMB-62', 'V-EMB-22', 2),
-- V-EMB-66: Skid + Lid + Frame (2x)
('V-EMB-66', 'V-EMB-6', 1),
('V-EMB-66', 'V-EMB-76', 1),
('V-EMB-66', 'V-EMB-26', 2),
-- V-EMB-69: Skid + Lid + Frame (2x)
('V-EMB-69', 'V-EMB-9', 1),
('V-EMB-69', 'V-EMB-79', 1),
('V-EMB-69', 'V-EMB-29', 2),
-- V-EMB-702: Base
('V-EMB-702', 'V-EMB-701', 1),
-- V-EMB-706: Base
('V-EMB-706', 'V-EMB-701', 1),
-- V-EMB-71: Skid
('V-EMB-71', 'V-EMB-1', 1),
-- V-EMB-72: Skid
('V-EMB-72', 'V-EMB-2', 1),
-- V-EMB-750: Skid + Lid (CORRECTED FROM CSV)
('V-EMB-750', 'V-EMB-1', 1),
('V-EMB-750', 'V-EMB-91', 1),
-- V-EMB-76: Skid
('V-EMB-76', 'V-EMB-6', 1),
-- V-EMB-780: Skid + Lid
('V-EMB-780', 'V-EMB-1', 1),
('V-EMB-780', 'V-EMB-71', 1),
-- V-EMB-79: Skid
('V-EMB-79', 'V-EMB-9', 1),
-- V-EMB-800: Skid + Lid
('V-EMB-800', 'V-EMB-1', 1),
('V-EMB-800', 'V-EMB-91', 1),
-- V-EMB-91: Skid
('V-EMB-91', 'V-EMB-1', 1),
-- V-EMB-92: Skid
('V-EMB-92', 'V-EMB-2', 1),
-- V-EMB-26: Skid
('V-EMB-26', 'V-EMB-6', 1),
-- V-EMB-29: Skid
('V-EMB-29', 'V-EMB-9', 1),
-- V-EMB-787: Skid + Lid
('V-EMB-787', 'V-EMB-1', 1),
('V-EMB-787', 'V-EMB-91', 1),
-- V-EMB-757: Skid + Lid
('V-EMB-757', 'V-EMB-1', 1),
('V-EMB-757', 'V-EMB-91', 1),
-- V-EMB-840: Skid + Lid
('V-EMB-840', 'V-EMB-1', 1),
('V-EMB-840', 'V-EMB-71', 1)
ON DUPLICATE KEY UPDATE quantity = VALUES(quantity);
