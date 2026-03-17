# Reporting Stored Procedure Validation

Last Updated: 2026-03-17

## Goal

Validate the full Reporting stored procedure pipeline using the workflow in [Database/UseToValidate_App_To_SP_Workflow.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/Database/UseToValidate_App_To_SP_Workflow.md):

- DAO call inventory
- parameter alignment
- result-column and model-mapping validation
- issues found and fixes applied

## Stored Procedure Inventory

| Status | Stored Procedure | DAO Method | Notes |
|---|---|---|---|
| [x] | `sp_Reporting_ReceivingHistory_GetByDateRange` | `Dao_Reporting.GetReceivingHistoryAsync` | Used |
| [x] | `sp_Reporting_DunnageHistory_GetByDateRange` | `Dao_Reporting.GetDunnageHistoryAsync` | Used |
| [x] | `sp_Reporting_VolvoHistory_GetByDateRange` | `Dao_Reporting.GetVolvoHistoryAsync` | Used |
| [x] | `sp_Reporting_Availability_GetByDateRange` | `Dao_Reporting.CheckAvailabilityAsync` | Used |

## Validation Checklist

| Status | Check | Result |
|---|---|---|
| [x] | Inventory all Reporting SP files | 4 SP files found under `Database/StoredProcedures/Reporting/` |
| [x] | Inventory all Reporting DAO SP calls | 4 calls found in `Module_Reporting/Data/Dao_Reporting.cs` |
| [x] | Cross-reference SP files against DAO usage | No unused Reporting SPs |
| [x] | Validate DAO parameter names against SP signatures | All calls use `start_date` and `end_date`, which map to `p_start_date` and `p_end_date` |
| [x] | Validate DAO parameter types against SP signatures | All Reporting calls pass `DateTime.Date`, matching `DATE` parameters |
| [x] | Validate mapper-required columns against Receiving SP | Receiving SP returns all columns used by `MapReportRowFromReader` |
| [x] | Validate mapper-required columns against Dunnage SP | Dunnage SP returns all required columns, including `created_by_username` |
| [x] | Validate mapper-required columns against Volvo SP | Volvo SP returns all required columns for the shared mapper |
| [x] | Validate availability SP result columns | Returns `receiving_count`, `dunnage_count`, and `volvo_count` expected by `MapAvailabilityFromReader` |
| [x] | Review source views behind Reporting SPs | `view_receiving_history`, `view_dunnage_history`, and `view_volvo_history` checked manually |

## Stored Procedure Validation Matrix

| Stored Procedure | Parameters Match | Columns Match | Model Maps | Status | Notes |
|---|---|---|---|---|---|
| `sp_Reporting_ReceivingHistory_GetByDateRange` | [x] | [x] | [x] | Validated | Uses `view_receiving_history`; columns align with `Model_ReportRow` mapper |
| `sp_Reporting_DunnageHistory_GetByDateRange` | [x] | [x] | [x] | Validated | Uses `view_dunnage_history`; surfaces both username and employee number |
| `sp_Reporting_VolvoHistory_GetByDateRange` | [x] | [x] | [x] | Fixed | Date filtering updated to use `DATE(created_date)` so end-date filtering is correct |
| `sp_Reporting_Availability_GetByDateRange` | [x] | [x] | [x] | Fixed | Volvo count subquery updated to use `DATE(created_date)` |

## Issues Found

| Status | Issue | Stored Procedure | Type | Fix Applied |
|---|---|---|---|---|
| [x] | Volvo records created later in the selected end date could be excluded because `view_volvo_history.created_date` is `DATETIME` while Reporting SP parameters are `DATE` | `sp_Reporting_VolvoHistory_GetByDateRange` | Date filter mismatch | Changed filter to `DATE(created_date) BETWEEN p_start_date AND p_end_date` |
| [x] | Volvo availability count had the same end-date filtering defect | `sp_Reporting_Availability_GetByDateRange` | Date filter mismatch | Changed Volvo count subquery to `DATE(created_date) BETWEEN p_start_date AND p_end_date` |
| [~] | Dunnage `specs_combined` currently reflects type-level spec metadata from `view_dunnage_history`, not archived per-load `specs_json`, because the base `dunnage_history` schema does not yet guarantee parity columns | `sp_Reporting_DunnageHistory_GetByDateRange` | Schema limitation | No SP fix needed; view remains deploy-safe until parity migration is applied |

## Deployment Checklist

| Status | Step | Notes |
|---|---|---|
| [ ] | Deploy `sp_Reporting_ReceivingHistory_GetByDateRange` | Needed for runtime validation |
| [ ] | Deploy `sp_Reporting_DunnageHistory_GetByDateRange` | Needed for runtime validation |
| [ ] | Deploy `sp_Reporting_VolvoHistory_GetByDateRange` | Includes Volvo date-filter fix |
| [ ] | Deploy `sp_Reporting_Availability_GetByDateRange` | Includes Volvo availability fix |
| [x] | Build application after source changes | `dotnet build` succeeded |
| [ ] | Validate Reporting procedures against live MySQL | Still pending deployment |

## Notes

- The static audit script referenced in the workflow document is not present in this repository, so validation for Reporting was completed manually.
- All four Reporting stored procedures are used by the Reporting DAO. No orphaned Reporting SPs were found.
- The current shared mapper intentionally tolerates module-specific missing columns by reading nullable fields only when present in each SP result.
