-- =============================================
-- Stored Procedure: sp_Part_Settings_SelectByPartId
-- Description: Retrieve part-specific settings
-- Used by: Auto-fill defaults in Wizard Step 2
-- =============================================
CREATE PROCEDURE [dbo].[sp_Part_Settings_SelectByPartId]
    @PartId NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT
        setting_id,
        part_id,
        part_type,
        default_package_type,
        default_receiving_location,
        quality_hold_required,
        packages_per_load_default,
        weight_per_package_default,
        notes,
        created_at,
        updated_at,
        updated_by
    FROM part_settings
    WHERE part_id = @PartId;
END
GO
