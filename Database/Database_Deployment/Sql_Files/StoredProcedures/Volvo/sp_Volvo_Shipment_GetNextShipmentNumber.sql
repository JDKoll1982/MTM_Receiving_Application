-- =====================================================
-- Stored Procedure: sp_Volvo_Shipment_GetNextShipmentNumber
-- =====================================================
-- Purpose: Get the next available shipment number
-- Database: mtm_receiving_application
-- =====================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Volvo_Shipment_GetNextShipmentNumber`$$

CREATE PROCEDURE `sp_Volvo_Shipment_GetNextShipmentNumber`()
BEGIN
  SELECT COALESCE(MAX(shipment_number), 0) + 1 AS next_shipment_number
  FROM volvo_label_data;
END $$

DELIMITER ;