# 05 Dunnage Edit Mode Load Number Showing Zeros

Last Updated: 2026-03-31

## Change

Fix Dunnage Edit Mode so Load # displays real values instead of `0`.

## Likely Files

- `Module_Dunnage/Data/Dao_DunnageLoad.cs`
- `Module_Dunnage/Services/Service_MySQL_Dunnage.cs`
- `Module_Dunnage/ViewModels/ViewModel_Dunnage_EditMode.cs`

## Implementation Instructions

1. Inspect the Dunnage Edit Mode query and row mapping for the Load # field.
2. Confirm whether the data source column is null, missing from the query, or incorrectly mapped to an integer default.
3. Fix the SELECT, stored procedure, or row mapper so the real Load # value is returned.
4. If Load # comes from Infor Visual or related PO data, preserve read-only access and null-safe mapping.
5. Add a regression check for rows with known non-zero load numbers.

## Acceptance Checks

- Existing rows with real Load # values no longer show `0`.
- Blank values only appear when the source data is genuinely blank.
