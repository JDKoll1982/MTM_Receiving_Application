-- =====================================================
-- Stored Procedure: sp_settings_module_volvo_reset
-- =====================================================
-- Purpose: Reset a setting to its default value
-- Database: mtm_receiving_application
-- =====================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS sp_settings_module_volvo_reset$$

CREATE PROCEDURE sp_settings_module_volvo_reset(
  IN p_setting_key VARCHAR(100),
  IN p_modified_by VARCHAR(50)
)
BEGIN
  UPDATE settings_module_volvo
  SET setting_value = default_value,
      modified_date = CURRENT_TIMESTAMP,
      modified_by = p_modified_by
  WHERE setting_key = p_setting_key;
END$$

DELIMITER ;
