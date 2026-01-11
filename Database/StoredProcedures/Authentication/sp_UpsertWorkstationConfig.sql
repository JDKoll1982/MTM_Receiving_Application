-- ============================================================================
-- Stored Procedure: sp_UpsertWorkstationConfig
-- Description: Insert or update the current auth_workstation_config row
-- Notes:
--  - Used to ensure the current machine is persisted on startup
--  - Validates workstation_type to allowed enum values
-- ============================================================================

USE mtm_receiving_application;

DROP PROCEDURE IF EXISTS sp_UpsertWorkstationConfig;

DELIMITER $$

CREATE PROCEDURE sp_UpsertWorkstationConfig(
    IN p_workstation_name VARCHAR(50),
    IN p_workstation_type VARCHAR(50),
    IN p_is_active BOOLEAN,
    IN p_description VARCHAR(200)
)
BEGIN
    DECLARE v_type VARCHAR(50);

    SET v_type = LOWER(TRIM(COALESCE(p_workstation_type, '')));

    IF v_type NOT IN ('shared_terminal', 'personal_workstation') THEN
        SET v_type = 'personal_workstation';
    END IF;

    INSERT INTO auth_workstation_config (
        workstation_name,
        workstation_type,
        is_active,
        description,
        created_date,
        modified_date
    ) VALUES (
        TRIM(p_workstation_name),
        v_type,
        COALESCE(p_is_active, TRUE),
        NULLIF(TRIM(p_description), ''),
        NOW(),
        NOW()
    )
    ON DUPLICATE KEY UPDATE
        workstation_type = v_type,
        is_active = COALESCE(p_is_active, is_active),
        description = COALESCE(NULLIF(TRIM(p_description), ''), description),
        modified_date = NOW();
END$$

DELIMITER ;

-- ============================================================================
-- Test Query (comment out in production)
-- ============================================================================
-- CALL sp_UpsertWorkstationConfig('MYPC', 'personal_workstation', TRUE, 'Auto-registered on startup');
