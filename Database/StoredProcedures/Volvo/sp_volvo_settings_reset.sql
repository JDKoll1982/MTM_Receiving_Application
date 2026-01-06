-- =====================================================
-- Stored Procedure: sp_volvo_settings_reset
-- =====================================================
-- Purpose: Reset a setting to its default value
-- Database: mtm_receiving_application
-- =====================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS sp_volvo_settings_reset$$

CREATE PROCEDURE sp_volvo_settings_reset(
  IN p_setting_key VARCHAR(100),
  IN p_modified_by VARCHAR(50)
)
BEGIN
  UPDATE volvo_settings
  SET setting_value = default_value,
      modified_date = CURRENT_TIMESTAMP,
      modified_by = p_modified_by
  WHERE setting_key = p_setting_key;
END$$

DELIMITER ;
