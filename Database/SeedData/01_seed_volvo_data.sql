-- =====================================================
-- Seed: Volvo Module Master Data
-- =====================================================
-- Purpose: Initial Volvo master data and component relationships.
-- Database: mtm_receiving_application
-- =====================================================
INSERT INTO
    volvo_masterdata (part_number, quantity_per_skid)
VALUES
    ('V-EMB-1', 10),
    ('V-EMB-2', 20),
    ('V-EMB-6', 10),
    ('V-EMB-9', 10),
    ('V-EMB-21', 50),
    ('V-EMB-22', 50),
    ('V-EMB-26', 32),
    ('V-EMB-29', 32),
    ('V-EMB-61', 120),
    ('V-EMB-62', 120),
    ('V-EMB-66', 100),
    ('V-EMB-69', 100),
    ('V-EMB-71', 25),
    ('V-EMB-72', 25),
    ('V-EMB-76', 25),
    ('V-EMB-79', 25),
    ('V-EMB-91', 25),
    ('V-EMB-92', 25),
    ('V-EMB-116', 150),
    ('V-EMB-500', 88),
    ('V-EMB-600', 44),
    ('V-EMB-701', 10),
    ('V-EMB-702', 20),
    ('V-EMB-706', 25),
    ('V-EMB-750', 80),
    ('V-EMB-757', 80),
    ('V-EMB-780', 40),
    ('V-EMB-787', 40),
    ('V-EMB-800', 40),
    ('V-EMB-840', 20) ON DUPLICATE KEY
UPDATE
    quantity_per_skid =
VALUES
    (quantity_per_skid);

INSERT INTO
    volvo_part_components (
        parent_part_number,
        component_part_number,
        quantity
    )
VALUES
    ('V-EMB-116', 'V-EMB-1', 1),
    ('V-EMB-116', 'V-EMB-71', 1),
    ('V-EMB-116', 'V-EMB-26', 3),
    ('V-EMB-21', 'V-EMB-1', 2),
    ('V-EMB-22', 'V-EMB-2', 2),
    ('V-EMB-500', 'V-EMB-2', 1),
    ('V-EMB-500', 'V-EMB-92', 1),
    ('V-EMB-600', 'V-EMB-2', 1),
    ('V-EMB-600', 'V-EMB-92', 1),
    ('V-EMB-61', 'V-EMB-1', 1),
    ('V-EMB-61', 'V-EMB-71', 1),
    ('V-EMB-61', 'V-EMB-21', 2),
    ('V-EMB-62', 'V-EMB-2', 1),
    ('V-EMB-62', 'V-EMB-72', 1),
    ('V-EMB-62', 'V-EMB-22', 2),
    ('V-EMB-66', 'V-EMB-6', 1),
    ('V-EMB-66', 'V-EMB-76', 1),
    ('V-EMB-66', 'V-EMB-26', 2),
    ('V-EMB-69', 'V-EMB-9', 1),
    ('V-EMB-69', 'V-EMB-79', 1),
    ('V-EMB-69', 'V-EMB-29', 2),
    ('V-EMB-702', 'V-EMB-701', 1),
    ('V-EMB-706', 'V-EMB-701', 1),
    ('V-EMB-71', 'V-EMB-1', 1),
    ('V-EMB-72', 'V-EMB-2', 1),
    ('V-EMB-750', 'V-EMB-1', 1),
    ('V-EMB-750', 'V-EMB-91', 1),
    ('V-EMB-76', 'V-EMB-6', 1),
    ('V-EMB-780', 'V-EMB-1', 1),
    ('V-EMB-780', 'V-EMB-71', 1),
    ('V-EMB-79', 'V-EMB-9', 1),
    ('V-EMB-800', 'V-EMB-1', 1),
    ('V-EMB-800', 'V-EMB-91', 1),
    ('V-EMB-91', 'V-EMB-1', 1),
    ('V-EMB-92', 'V-EMB-2', 1),
    ('V-EMB-26', 'V-EMB-6', 1),
    ('V-EMB-29', 'V-EMB-9', 1),
    ('V-EMB-787', 'V-EMB-1', 1),
    ('V-EMB-787', 'V-EMB-91', 1),
    ('V-EMB-757', 'V-EMB-1', 1),
    ('V-EMB-757', 'V-EMB-91', 1),
    ('V-EMB-840', 'V-EMB-1', 1),
    ('V-EMB-840', 'V-EMB-71', 1) ON DUPLICATE KEY
UPDATE
    quantity =
VALUES
    (quantity);