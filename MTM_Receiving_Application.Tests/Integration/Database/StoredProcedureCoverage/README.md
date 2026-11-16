# Stored Procedure Coverage Tests

Last Updated: 2026-11-15

Every stored procedure the repository ships is exercised from this folder. The suite deploys the
whole SQL tree into a throwaway schema (`mtm_sptest_<8 hex chars>`), runs against it, and drops it
afterwards, so no existing database is read or written and the tests can insert and delete freely.

## What Runs

| File | Guarantee |
| --- | --- |
| `StoredProcedureCoverageTests` | the tree declares 232 procedures under unique `sp_` names, and every one of them is named by at least one test |
| `StoredProcedureExecutionContractTests` | the tree deploys without a failure, the deployed set matches the tree, and each procedure is called with sample arguments without returning a broken-SQL error |
| `DunnageStoredProcedureTests` | custom-field choices, the no-label history write and flag, changing a part's type, both delete-impact reports, dunnage reprint search |
| `LotMarkingStoredProcedureTests` | enabled-part list, marking ledger, heat keys, realign and clear on receiving rows, unmarked lot fill-in |
| `ReceivingScannerStoredProcedureTests` | scanner profile lifecycle, session header, ordered items, clearing the list |
| `ReceivingStoredProcedureTests` | receiving reprint search and the vendor variable mapping |
| `ReconciliationStoredProcedureTests` | activity calendar, one-day row read, the single location correction |
| `ReportingStoredProcedureTests` | analytics coverage figure, received-part cohort, Volvo reprint search |
| `SettingsCoreStoredProcedureTests` | metadata listings, system and user value tables, audit trail, role assignment |
| `WeldedCoilsStoredProcedureTests` | welded-coil settings lifecycle and duplicate rejection |

## Running

The suite needs one MySQL account that may create databases. Set the connection string and run the
coverage filter:

```powershell
$env:MTM_TEST_MYSQL_CONNECTION_STRING='Server=127.0.0.1;Port=3306;User ID=root;Password=root;AllowUserVariables=true;'
dotnet test MTM_Receiving_Application.Tests/MTM_Receiving_Application.Tests.csproj `
  --filter "FullyQualifiedName~StoredProcedureCoverage"
```

Without the variable the fixture falls back to `MTM_Database_Deployment/deployment.settings.json`
and probes its preferred then fallback server. When no server can be used, the database tests report
as skipped with the reason; the five tree-shape checks still run and still fail if a procedure is
added without a test.

## Adding A Procedure

1. Add the script under `Database/Database_Deployment/Sql_Files/StoredProcedures/<Module>/`.
2. Add a test that calls it by name in the module's test class here.
3. Run the coverage filter. `EveryProcedure_ShouldBeNamedByATest` fails and lists the name until
   step 2 is done.

## Notes For Whoever Runs This Next

- Each script deploys on its own connection, because two procedure scripts run
  `SET NAMES utf8mb4 COLLATE utf8mb4_general_ci` and would otherwise change how every later
  procedure is created.
- Schema scripts are deployed with `FOREIGN_KEY_CHECKS = 0` re-asserted per statement; several table
  scripts turn it back on in their last line.
- `MySql.Data` cannot resolve a `JSON` parameter for `sp_Receiving_ScannerItem_Upsert`, so that call
  is built by hand through `ExecuteProcedureByPositionAsync`.
