-- Stored Procedure: sp_seed_user_default_modes
-- Purpose: Best-effort seeding of per-user default workflow modes for first-run.
-- MySQL Version: 5.7 compatible (no exception handlers required)
-- Notes:
--   - Safe to run even if the default_* columns have not been migrated yet.
--   - Does NOT throw if columns are missing; it simply skips those updates.

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS sp_seed_user_default_modes;

DELIMITER //

CREATE PROCEDURE sp_seed_user_default_modes(
    IN p_user_id INT
)
BEGIN
    DECLARE v_has_default_receiving_mode INT DEFAULT 0;
    DECLARE v_has_default_dunnage_mode INT DEFAULT 0;

    -- Seed default_receiving_mode if column exists
    SELECT COUNT(*)
    INTO v_has_default_receiving_mode
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'users'
      AND COLUMN_NAME = 'default_receiving_mode';

    IF v_has_default_receiving_mode > 0 THEN
        UPDATE users
        SET default_receiving_mode = COALESCE(default_receiving_mode, 'guided'),
            modified_date = NOW()
        WHERE employee_number = p_user_id;
    END IF;

    -- Seed default_dunnage_mode if column exists
    SELECT COUNT(*)
    INTO v_has_default_dunnage_mode
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'users'
      AND COLUMN_NAME = 'default_dunnage_mode';

    IF v_has_default_dunnage_mode > 0 THEN
        UPDATE users
        SET default_dunnage_mode = COALESCE(default_dunnage_mode, 'guided'),
            modified_date = NOW()
        WHERE employee_number = p_user_id;
    END IF;

    SELECT 1 AS success;
END //

DELIMITER ;
