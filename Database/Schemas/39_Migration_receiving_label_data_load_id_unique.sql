-- ============================================================================
-- Migration 39: Add UNIQUE constraint to receiving_label_data.load_id
-- ============================================================================
-- Purpose:
--   Prevent duplicate rows from being inserted with the same load_id GUID.
--   Each Model_ReceivingLoad session object is assigned Guid.NewGuid() so
--   load_id must be unique across all rows in the active queue.
--
--   Background: A bug in the "Add Another Part" workflow was previously
--   causing SaveToXLSOnlyAsync to save loads to the DB mid-session, then
--   SaveSessionAsync would save them again on final submit. The application
--   bug has been fixed in ViewModel_Receiving_Review.AddAnotherPartAsync,
--   but this constraint adds a database-level safety net to prevent future
--   duplicate inserts from any code path.
--
-- IMPORTANT: Run the de-duplication step below before adding the constraint
--   if the table already contains duplicate load_id values from the bug.
-- ============================================================================

USE mtm_receiving_application;

-- Step 1: Remove duplicate rows, keeping the earliest inserted row (lowest id)
-- for each load_id that has been duplicated.
DELETE rld
FROM receiving_label_data rld
INNER JOIN (
    SELECT load_id, MIN(id) AS keep_id
    FROM receiving_label_data
    WHERE load_id IS NOT NULL
    GROUP BY load_id
    HAVING COUNT(*) > 1
) AS dupes ON rld.load_id = dupes.load_id AND rld.id != dupes.keep_id;

-- Step 2: Drop the old non-unique index on load_id (if it exists).
-- IF EXISTS is not available for DROP INDEX in MySQL 5.x, so use a procedure.
DROP PROCEDURE IF EXISTS mig39_drop_idx;
DELIMITER $$
CREATE PROCEDURE mig39_drop_idx()
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.statistics
        WHERE table_schema = DATABASE()
          AND table_name   = 'receiving_label_data'
          AND index_name   = 'idx_load_id'
    ) THEN
        ALTER TABLE receiving_label_data DROP INDEX idx_load_id;
    END IF;
END$$
DELIMITER ;
CALL mig39_drop_idx();
DROP PROCEDURE IF EXISTS mig39_drop_idx;

-- Step 3: Add the UNIQUE constraint.
ALTER TABLE receiving_label_data
    ADD CONSTRAINT uq_receiving_label_data_load_id UNIQUE (load_id);

-- ============================================================================
-- Migration 39b: Add skid-counter columns to receiving_label_data
-- ============================================================================
-- Purpose:
--   Support "N of M" label printing for LabelView2022.
--   part_skid_sequence : position of this skid among all skids of the same
--                        part_id in this save batch  (1, 2, 3 … M)
--   part_skid_total    : total skids for this part_id in this save batch (M)
--
--   Example: 6 skids of MMC0001000 → each row stores part_skid_total = 6 and
--   part_skid_sequence = 1..6, so the label can print "2 of 6".
-- ============================================================================

DROP PROCEDURE IF EXISTS mig39b_add_col;
DELIMITER $$
CREATE PROCEDURE mig39b_add_col()
BEGIN
    -- part_skid_sequence
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = DATABASE()
          AND table_name   = 'receiving_label_data'
          AND column_name  = 'part_skid_sequence'
    ) THEN
        ALTER TABLE receiving_label_data
            ADD COLUMN part_skid_sequence INT NULL
            COMMENT 'Position of this skid among all skids for this part_id in the save batch (e.g. 2)';
    END IF;

    -- part_skid_total
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = DATABASE()
          AND table_name   = 'receiving_label_data'
          AND column_name  = 'part_skid_total'
    ) THEN
        ALTER TABLE receiving_label_data
            ADD COLUMN part_skid_total INT NULL
            COMMENT 'Total skids for this part_id in the save batch (e.g. 6 → label reads "N of 6")';
    END IF;

    -- Mirror columns in receiving_history
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = DATABASE()
          AND table_name   = 'receiving_history'
          AND column_name  = 'part_skid_sequence'
    ) THEN
        ALTER TABLE receiving_history
            ADD COLUMN part_skid_sequence INT NULL
            COMMENT 'Position of this skid among all skids for this part_id in the save batch';
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = DATABASE()
          AND table_name   = 'receiving_history'
          AND column_name  = 'part_skid_total'
    ) THEN
        ALTER TABLE receiving_history
            ADD COLUMN part_skid_total INT NULL
            COMMENT 'Total skids for this part_id in the save batch';
    END IF;
END$$
DELIMITER ;
CALL mig39b_add_col();
DROP PROCEDURE IF EXISTS mig39b_add_col;
