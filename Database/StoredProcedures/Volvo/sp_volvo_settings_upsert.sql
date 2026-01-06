-- =====================================================
-- Stored Procedure: sp_volvo_settings_upsert
-- =====================================================
-- Purpose: Insert or update a Volvo setting
-- Database: mtm_receiving_application
-- =====================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS sp_volvo_settings_upsert$$

CREATE PROCEDURE sp_volvo_settings_upsert(
  IN p_setting_key VARCHAR(100),
  IN p_setting_value TEXT,
  IN p_modified_by VARCHAR(50)
)
BEGIN
  INSERT INTO volvo_settings (setting_key, setting_value, setting_type, category, description, default_value, modified_by)
  SELECT 
    p_setting_key,
    p_setting_value,
    setting_type,
    category,
    description,
    default_value,
    p_modified_by
  FROM volvo_settings
  WHERE setting_key = p_setting_key
  ON DUPLICATE KEY UPDATE
    setting_value = p_setting_value,
    modified_date = CURRENT_TIMESTAMP,
    modified_by = p_modified_by;
END$$

DELIMITER ;
