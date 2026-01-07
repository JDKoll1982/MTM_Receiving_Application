-- Test script to examine PO line structure and data
-- Server: VISUAL, Database: MTMFG

USE MTMFG
GO

-- First, verify which database you're connected to
SELECT 
    'Current Database' AS QueryType,
    DB_NAME() AS DatabaseName

-- Check what schemas exist
SELECT 
    'Available Schemas' AS QueryType,
    SCHEMA_NAME
FROM INFORMATION_SCHEMA.SCHEMATA
ORDER BY SCHEMA_NAME

-- List ALL tables in the current database (to see what's actually available)
SELECT 
    'All Tables in Database' AS QueryType,
    TABLE_SCHEMA,
    TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_SCHEMA, TABLE_NAME

-- Try to find purchase-related tables (case insensitive)
SELECT 
    'Purchase-Related Tables' AS QueryType,
    TABLE_SCHEMA,
    TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
    AND (
        LOWER(TABLE_NAME) LIKE '%purchase%' 
        OR LOWER(TABLE_NAME) LIKE '%purc%' 
        OR LOWER(TABLE_NAME) LIKE '%po_%'
        OR TABLE_NAME LIKE 'PO%'
    )
ORDER BY TABLE_NAME

-- Check sys.tables for all purchase-related objects
SELECT 
    'Purchase Tables from sys.tables' AS QueryType,
    SCHEMA_NAME(schema_id) AS SchemaName,
    name AS TableName
FROM sys.tables
WHERE LOWER(name) LIKE '%purchase%' 
    OR LOWER(name) LIKE '%purc%'
    OR LOWER(name) LIKE '%po%'
ORDER BY name

-- Replace 'PO-060535B' with your actual PO number
DECLARE @PONumber NVARCHAR(50) = 'PO-060535B'

-- Check if the PO exists (try different table name variations)
SELECT 
    'PO Exists Check - PURCHASE_ORDER' AS QueryType,
    COUNT(*) AS PO_Count
FROM PURCHASE_ORDER po
WHERE po.ID = @PONumber

-- Show all columns for the first few lines of this PO
SELECT TOP 5
    'Full Line Data' AS QueryType,
    po.ID AS PO_Number,
    pol.LINE_NO,
    pol.PART_ID,
    pol.VENDOR_PART_ID,
    pol.USER_1,
    pol.USER_2,
    pol.USER_3,
    pol.USER_4,
    pol.USER_5,
    pol.USER_6,
    pol.USER_7,
    pol.USER_8,
    pol.USER_9,
    pol.USER_10,
    pol.LINE_STATUS,
    pol.USER_ORDER_QTY,
    pol.PURCHASE_UM,
    pol.TOTAL_RECEIVED_QTY
FROM PURC_ORDER_LINE pol
INNER JOIN PURCHASE_ORDER po ON pol.PURC_ORDER_ID = po.ID
WHERE po.ID = @PONumber
ORDER BY pol.LINE_NO

-- Show all non-NULL USER fields for this PO
SELECT 
    'Non-NULL User Fields' AS QueryType,
    po.ID AS PO_Number,
    pol.LINE_NO,
    pol.PART_ID,
    CASE WHEN pol.USER_1 IS NOT NULL THEN 'USER_1: ' + pol.USER_1 ELSE NULL END AS USER_1_Value,
    CASE WHEN pol.USER_2 IS NOT NULL THEN 'USER_2: ' + pol.USER_2 ELSE NULL END AS USER_2_Value,
    CASE WHEN pol.USER_3 IS NOT NULL THEN 'USER_3: ' + pol.USER_3 ELSE NULL END AS USER_3_Value,
    CASE WHEN pol.USER_4 IS NOT NULL THEN 'USER_4: ' + pol.USER_4 ELSE NULL END AS USER_4_Value,
    CASE WHEN pol.USER_5 IS NOT NULL THEN 'USER_5: ' + pol.USER_5 ELSE NULL END AS USER_5_Value,
    CASE WHEN pol.USER_6 IS NOT NULL THEN 'USER_6: ' + pol.USER_6 ELSE NULL END AS USER_6_Value,
    CASE WHEN pol.USER_7 IS NOT NULL THEN 'USER_7: ' + pol.USER_7 ELSE NULL END AS USER_7_Value,
    CASE WHEN pol.USER_8 IS NOT NULL THEN 'USER_8: ' + pol.USER_8 ELSE NULL END AS USER_8_Value,
    CASE WHEN pol.USER_9 IS NOT NULL THEN 'USER_9: ' + pol.USER_9 ELSE NULL END AS USER_9_Value,
    CASE WHEN pol.USER_10 IS NOT NULL THEN 'USER_10: ' + pol.USER_10 ELSE NULL END AS USER_10_Value
FROM PURC_ORDER_LINE pol
INNER JOIN PURCHASE_ORDER po ON pol.PURC_ORDER_ID = po.ID
WHERE po.ID = @PONumber
ORDER BY pol.LINE_NO

-- Search for 'BATTERY' in any USER field
SELECT 
    'BATTERY Search Results' AS QueryType,
    po.ID AS PO_Number,
    pol.LINE_NO,
    pol.PART_ID,
    CASE 
        WHEN pol.USER_1 LIKE '%BATTERY%' THEN 'Found in USER_1: ' + pol.USER_1
        WHEN pol.USER_2 LIKE '%BATTERY%' THEN 'Found in USER_2: ' + pol.USER_2
        WHEN pol.USER_3 LIKE '%BATTERY%' THEN 'Found in USER_3: ' + pol.USER_3
        WHEN pol.USER_4 LIKE '%BATTERY%' THEN 'Found in USER_4: ' + pol.USER_4
        WHEN pol.USER_5 LIKE '%BATTERY%' THEN 'Found in USER_5: ' + pol.USER_5
        WHEN pol.USER_6 LIKE '%BATTERY%' THEN 'Found in USER_6: ' + pol.USER_6
        WHEN pol.USER_7 LIKE '%BATTERY%' THEN 'Found in USER_7: ' + pol.USER_7
        WHEN pol.USER_8 LIKE '%BATTERY%' THEN 'Found in USER_8: ' + pol.USER_8
        WHEN pol.USER_9 LIKE '%BATTERY%' THEN 'Found in USER_9: ' + pol.USER_9
        WHEN pol.USER_10 LIKE '%BATTERY%' THEN 'Found in USER_10: ' + pol.USER_10
    END AS MatchingField
FROM PURC_ORDER_LINE pol
INNER JOIN PURCHASE_ORDER po ON pol.PURC_ORDER_ID = po.ID
WHERE po.ID = @PONumber
    AND (
        pol.USER_1 LIKE '%BATTERY%'
        OR pol.USER_2 LIKE '%BATTERY%'
        OR pol.USER_3 LIKE '%BATTERY%'
        OR pol.USER_4 LIKE '%BATTERY%'
        OR pol.USER_5 LIKE '%BATTERY%'
        OR pol.USER_6 LIKE '%BATTERY%'
        OR pol.USER_7 LIKE '%BATTERY%'
        OR pol.USER_8 LIKE '%BATTERY%'
        OR pol.USER_9 LIKE '%BATTERY%'
        OR pol.USER_10 LIKE '%BATTERY%'
    )
ORDER BY pol.LINE_NO

-- First, let's see what columns exist in PURC_LINE_DEL
SELECT 
    'PURC_LINE_DEL Columns' AS QueryType,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'PURC_LINE_DEL'
ORDER BY ORDINAL_POSITION

-- Show what columns exist in PURC_LINE_BINARY
SELECT 
    'PURC_LINE_BINARY Columns' AS QueryType,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'PURC_LINE_BINARY'
ORDER BY ORDINAL_POSITION

-- Show all PURC_LINE_DEL records for this PO (all columns)
SELECT 
    'PURC_LINE_DEL - All Records' AS QueryType,
    *
FROM PURC_LINE_DEL
WHERE PURC_ORDER_ID = @PONumber

-- Show all PURC_LINE_BINARY records for this PO (all columns)
SELECT 
    'PURC_LINE_BINARY - All Records' AS QueryType,
    *
FROM PURC_LINE_BINARY
WHERE PURC_ORDER_ID = @PONumber
