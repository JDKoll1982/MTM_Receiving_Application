-- ==============================================================================
-- IMPORTANT: Database Project Deployment Instructions
-- ==============================================================================
--
-- For SQL Server Database Projects (.sqlproj), DO NOT use this migration script.
-- Instead, add the individual table files directly to your project:
--
-- 1. In Visual Studio, right-click the Database Project
-- 2. Add > Existing Item
-- 3. Navigate to Module_Databases/Module_Receiving_Database/Tables/
-- 4. Select all .sql files and add them
-- 5. Build and Publish the project
--
-- The Database Project will automatically determine the correct deployment order
-- based on dependencies (foreign keys, etc.)
--
-- ==============================================================================
-- For Direct SSMS/SQLCMD Deployment (Non-Database Project)
-- ==============================================================================
--
-- If you want to deploy without a database project, use SQLCMD mode:
--
-- 1. Open SQL Server Management Studio (SSMS)
-- 2. Query Menu > SQLCMD Mode (enable it)
-- 3. Run this script
--
-- Or use SQLCMD command line:
-- sqlcmd -S localhost -d MTM_Receiving -i 001_InitialSchema.sql
--
-- ==============================================================================

-- Enable SQLCMD mode check
:setvar DatabaseName "MTM_Receiving"
GO

SET NOCOUNT ON;
GO

PRINT '======================================================================';
PRINT 'Module_Receiving Database - Schema Creation';
PRINT 'Using SQLCMD mode to include individual table files';
PRINT 'Started: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '======================================================================';
GO

-- ==============================================================================
-- Reference Tables (must be created first - no dependencies)
-- ==============================================================================

PRINT 'Creating Reference Tables...';
GO

:r ../Tables/tbl_Receiving_PartType.sql
:r ../Tables/tbl_Receiving_PackageType.sql
:r ../Tables/tbl_Receiving_Location.sql
:r ../Tables/tbl_Receiving_Settings.sql

PRINT 'Reference tables created.';
GO

-- ==============================================================================
-- Transaction Tables (depend on reference tables)
-- ==============================================================================

PRINT 'Creating Transaction Tables...';
GO

:r ../Tables/tbl_Receiving_Transaction.sql
:r ../Tables/tbl_Receiving_Line.sql
:r ../Tables/tbl_Receiving_WorkflowSession.sql
:r ../Tables/tbl_Receiving_PartPreference.sql

PRINT 'Transaction tables created.';
GO

-- ==============================================================================
-- Audit Tables (no dependencies)
-- ==============================================================================

PRINT 'Creating Audit Tables...';
GO

:r ../Tables/tbl_Receiving_AuditLog.sql
:r ../Tables/tbl_Receiving_CompletedTransaction.sql

PRINT 'Audit tables created.';
GO

-- ==============================================================================
-- Verification
-- ==============================================================================

PRINT '';
PRINT '======================================================================';
PRINT 'Schema Creation Completed!';
PRINT '----------------------------------------------------------------------';

SELECT 
    'Tables' AS ObjectType,
    COUNT(*) AS Created
FROM sys.tables
WHERE name LIKE 'tbl_Receiving_%';

PRINT '';
PRINT 'Next Steps:';
PRINT '  1. Run Scripts/Seed/SeedPartTypes.sql';
PRINT '  2. Run Scripts/Seed/SeedPackageTypes.sql';
PRINT '  3. Run Scripts/Seed/SeedDefaultSettings.sql';
PRINT '';
PRINT 'Completed: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '======================================================================';
GO
