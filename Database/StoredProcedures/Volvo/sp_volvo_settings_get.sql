-- =====================================================
-- Stored Procedure: sp_volvo_settings_get
-- =====================================================
-- Purpose: Get a specific Volvo setting by key
-- Database: mtm_receiving_application
-- =====================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS sp_volvo_settings_get$$

CREATE PROCEDURE sp_volvo_settings_get(
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
  FROM volvo_settings
  WHERE setting_key = p_setting_key;
END$$

DELIMITER ;
