-- =====================================================
-- Volvo Module - Sample Test Data
-- =====================================================
-- Purpose: Initial master data from DataSheet.csv
-- Database: mtm_receiving_application
-- =====================================================

-- Sample data for volvo_parts_master (from DataSheet.csv)
INSERT INTO volvo_parts_master (part_number, description, quantity_per_skid) VALUES
('V-EMB-1', 'Skid', 1),
('V-EMB-2', NULL, 20),
('V-EMB-6', NULL, 90),
('V-EMB-9', NULL, 30),
('V-EMB-21', NULL, 50),
('V-EMB-26', NULL, 30),
('V-EMB-61', NULL, 70),
('V-EMB-71', NULL, 1),
('V-EMB-92', NULL, 25),
('V-EMB-116', NULL, 150),
('V-EMB-500', NULL, 88),
('V-EMB-750', NULL, 80),
('V-EMB-780', NULL, 50)
ON DUPLICATE KEY UPDATE quantity_per_skid = VALUES(quantity_per_skid);

-- Sample data for volvo_part_components (component explosion relationships)
INSERT INTO volvo_part_components (parent_part_number, component_part_number, quantity) VALUES
('V-EMB-500', 'V-EMB-2', 1),
('V-EMB-500', 'V-EMB-92', 1),
('V-EMB-750', 'V-EMB-1', 1),
('V-EMB-750', 'V-EMB-6', 1),
('V-EMB-750', 'V-EMB-71', 1),
('V-EMB-780', 'V-EMB-9', 1),
('V-EMB-780', 'V-EMB-26', 3),
('V-EMB-116', 'V-EMB-1', 1),
('V-EMB-116', 'V-EMB-26', 3),
('V-EMB-116', 'V-EMB-71', 1)
ON DUPLICATE KEY UPDATE quantity = VALUES(quantity);
