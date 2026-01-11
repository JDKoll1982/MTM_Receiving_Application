-- ============================================================================
-- Stored Procedure: sp_UpsertUser
-- Purpose: Seed/Upsert a user row for migrations and environment setup.
-- MySQL Version: 5.7 compatible
-- Notes:
--   - Uses INSERT ... ON DUPLICATE KEY UPDATE for idempotent migrations.
--   - Intended for controlled data seeding (not the interactive CreateNewUser flow).
-- ============================================================================

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS sp_UpsertUser;

DELIMITER $$

CREATE PROCEDURE sp_UpsertUser(
    IN p_employee_number INT,
    IN p_windows_username VARCHAR(50),
    IN p_full_name VARCHAR(100),
    IN p_pin VARCHAR(4),
    IN p_department VARCHAR(50),
    IN p_shift VARCHAR(20),
    IN p_is_active BOOLEAN,
    IN p_visual_username VARCHAR(50),
    IN p_visual_password VARCHAR(100),
    IN p_created_by VARCHAR(50)
)
sp: BEGIN
    DECLARE v_emp_exists INT DEFAULT 0;
    DECLARE v_conflict_username INT DEFAULT 0;

    -- Prevent accidental cross-updates when other UNIQUE keys collide.
    SELECT COUNT(*)
    INTO v_conflict_username
    FROM users
    WHERE windows_username = p_windows_username
      AND employee_number <> p_employee_number;

    IF v_conflict_username > 0 THEN
        SELECT 0 AS success,
               0 AS affected_rows,
               'Windows username already assigned to a different employee_number' AS error_message;
        LEAVE sp;
    END IF;

    -- Deterministic upsert keyed on employee_number (PK)
    SELECT COUNT(*)
    INTO v_emp_exists
    FROM users
    WHERE employee_number = p_employee_number;

    IF v_emp_exists > 0 THEN
        UPDATE users
        SET windows_username = p_windows_username,
            full_name = p_full_name,
            pin = p_pin,
            department = p_department,
            shift = p_shift,
            is_active = p_is_active,
            visual_username = NULLIF(TRIM(p_visual_username), ''),
            visual_password = NULLIF(TRIM(p_visual_password), ''),
            created_by = p_created_by,
            modified_date = NOW()
        WHERE employee_number = p_employee_number;
    ELSE
        INSERT INTO auth_users (
            employee_number,
            windows_username,
            full_name,
            pin,
            department,
            shift,
            is_active,
            visual_username,
            visual_password,
            created_by,
            created_date,
            modified_date
        ) VALUES (
            p_employee_number,
            p_windows_username,
            p_full_name,
            p_pin,
            p_department,
            p_shift,
            p_is_active,
            NULLIF(TRIM(p_visual_username), ''),
            NULLIF(TRIM(p_visual_password), ''),
            p_created_by,
            NOW(),
            NOW()
        );
    END IF;

    -- Keep first-run defaults consistent.
    CALL sp_seed_user_default_modes(p_employee_number);

    SELECT 1 AS success,
           ROW_COUNT() AS affected_rows,
           NULL AS error_message;
END$$

DELIMITER ;
