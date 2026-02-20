-- ============================================================================
-- Script: receiving_label_history_reconciliation.sql
-- Module: Receiving
-- Purpose: Reconcile active label queue vs archived history after Clear Label Data
-- Usage:
--   Run after clear-to-history operations to verify no orphaned or lost rows.
-- ============================================================================

USE mtm_receiving_application;

-- 1) Current queue row count (should be 0 immediately after successful clear)
SELECT COUNT(*) AS active_queue_count
FROM receiving_label_data;

-- 2) Latest archive batch summary
SELECT
    ArchiveBatchID,
    ArchivedBy,
    MIN(ArchivedAt) AS batch_started_at,
    MAX(ArchivedAt) AS batch_finished_at,
    COUNT(*) AS rows_archived
FROM receiving_history
WHERE ArchiveBatchID IS NOT NULL
GROUP BY ArchiveBatchID, ArchivedBy
ORDER BY batch_finished_at DESC
LIMIT 10;

-- 3) Potential data quality issues in history
SELECT
    COUNT(*) AS missing_load_id
FROM receiving_history
WHERE LoadID IS NULL OR LoadID = '';

SELECT
    COUNT(*) AS missing_part_id
FROM receiving_history
WHERE PartID IS NULL OR PartID = '';

SELECT
    COUNT(*) AS missing_received_date
FROM receiving_history
WHERE ReceivedDate IS NULL;

-- 4) Duplicate LoadID check in history (should normally be 0)
SELECT
    LoadID,
    COUNT(*) AS duplicate_count
FROM receiving_history
GROUP BY LoadID
HAVING COUNT(*) > 1
ORDER BY duplicate_count DESC, LoadID
LIMIT 50;

-- 5) Recent archive rows with full context spot-check
SELECT
    LoadID,
    PartID,
    PONumber,
    LoadNumber,
    WeightQuantity,
    HeatLotNumber,
    PackagesPerLoad,
    PackageTypeName,
    ReceivedDate,
    UserID,
    EmployeeNumber,
    ArchivedAt,
    ArchivedBy,
    ArchiveBatchID
FROM receiving_history
ORDER BY ArchivedAt DESC, ReceivedDate DESC
LIMIT 100;

-- ============================================================================
