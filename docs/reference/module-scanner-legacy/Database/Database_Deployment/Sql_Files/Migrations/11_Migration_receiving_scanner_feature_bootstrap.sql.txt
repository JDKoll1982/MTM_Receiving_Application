-- ============================================================================
-- Migration: 01_Migration_receiving_scanner_feature_bootstrap
-- Module: Receiving
-- Purpose: Bootstrap all schema, function, view, trigger, and procedure assets for
--          scanner feature support.
-- ============================================================================

USE mtm_receiving_application;

-- NOTE:
-- Execute files in this folder set in the order documented by
-- ../README.md. This migration file is an orchestration marker and
-- deployment checklist entry for release scripts.

SELECT 'Run Schemas, Functions, Views, Triggers, and StoredProcedures from Receiving_ScannerFeature_Requirements in documented order.' AS instructions;