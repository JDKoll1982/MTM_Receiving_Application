-- =====================================================
-- Stored Procedure: sp_settings_module_volvo_get
-- =====================================================
-- Purpose: Get a specific Volvo setting by key
-- Database: mtm_receiving_application
-- =====================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS sp_settings_module_volvo_get$$

CREATE PROCEDURE sp_settings_module_volvo_get(
  IN p_setting_key VARCHAR(100)
)
BEGIN
  SELECT
    setting_key,
    setting_value,
    setting_type,
    category,
    description,
    default_value,
    min_value,
    max_value,
    modified_date,
    modified_by
  FROM settings_module_volvo
  WHERE setting_key = p_setting_key;
END$$

DELIMITER ;
