# Persistence And History Service

Last Updated: 2026-07-21

This service coordinates storage of drafts, profiles, send snapshots, and item-level results through DAO and stored-procedure pathways.

## Service Purpose

- keep persistence off the hot input path
- provide reliable save and restore for user-scoped drafts
- record send results for audit and retry support

## Inputs

- session aggregate state changes
- profile updates and default-selection changes
- send result summaries and item results

## Outputs

- Model_Dao_Result and Model_Dao_Result<T> responses
- persisted identifiers for new sessions and profiles
- history query models for reporting view-models

## Required Responsibilities

- persist draft session headers and ordered items
- persist profile records per user
- persist immutable send-history snapshots
- support load-by-user, load-by-status, and load-recent queries
- support resume workflows that restore waiting and failed items accurately
- persist target executable, child screen title, optional class match, and from/to warehouse default variables as part of profile data
- persist item-level from/to warehouse and location values into history records so audit and retry views reflect the exact sent payload

## DAO And Database Expectations

- all MySQL writes go through stored procedures
- no direct SQL command construction in view-models or orchestration service
- expected failures return DAO result models, not thrown control-flow exceptions
- asynchronous operations only

## Recommended DAO Surfaces

- Dao_ScannerBatchSession for session header operations
- Dao_ScannerBatchItem for item operations and reorder persistence
- Dao_ScannerAutomationProfile for profile CRUD and defaults
- Dao_ScannerSendHistory for append-only send outcome recording
- Dao_ScannerTargetProfile or equivalent profile DAO expansion for `VMINVENT.exe`, `Inventory Transfers`, and warehouse-default metadata if the team wants narrower separation
- history row/result retrieval surfaces that project warehouse/location traceability directly for the history screen

## Use Or Modify Guidance

Use existing:

- Helper_Database_StoredProcedure execution patterns
- existing DAO result model conventions
- existing settings-core facade where user-scoped default keys already fit

Modify or extend:

- Infrastructure dependency injection registration for new DAOs and services
- database deployment stored procedures in standard repo deployment paths

Do not modify:

- Infor Visual SQL Server write paths, because they must remain read-only
- scanner execution path to block on non-critical history writes
