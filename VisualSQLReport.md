# Visual SQL Report

Validation Update: 2026-04-19

I re-validated the remediation items in this report against the current codebase.
One recommended fix has been completed: `Database/InforVisualScripts/Queries/20_GetMaterialAvailabilityAssociatedPartRuns.sql` now projects `InputPartNumber`, so the previously reported SQL/DAO contract mismatch is no longer present.
The table below lists the items that still need correction.

| Still needing correction                                      | Current validation result                                                                                                                                                                                                                                                                                                                       | Primary evidence                                                                                                                                                                                      |
| ------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| PO loading N+1 query fan-out                                  | Still open. `ViewModel_Receiving_POEntry.LoadPOAsync` still loads the PO once and then calls `GetRemainingQuantityAsync` once per part. `Service_InforVisualConnect.GetRemainingQuantityAsync` still re-queries the same PO through `_dao.GetPOWithPartsAsync(poNumber)` instead of using the already loaded PO data or a lighter-weight query. | `Module_Receiving/ViewModels/ViewModel_Receiving_POEntry.cs`, `Module_Core/Services/Database/Service_InforVisualConnect.cs`                                                                           |
| Cooperative shutdown cancellation for Infor Visual queries    | Still open. `IService_ApplicationShutdown` exposes `ShutdownToken`, but `IService_InforVisual` still has no `CancellationToken` parameters and `Dao_InforVisualConnection` still uses bare `OpenAsync`, `ExecuteReaderAsync`, and `ExecuteScalarAsync` calls without cancellation token overloads.                                              | `Module_Core/Services/Service_ApplicationShutdown.cs`, `Module_Core/Contracts/Services/IService_InforVisual.cs`, `Module_Core/Data/InforVisual/Dao_InforVisualConnection.cs`                          |
| Applying configured Infor Visual timeouts at runtime          | Still open. `appsettings.json` and `InforVisualSettings` still define `ConnectionTimeoutSeconds` and `QueryTimeoutSeconds`, but DI still constructs `Dao_InforVisualConnection` with only the connection string and logger, and the DAO still does not set `SqlCommand.CommandTimeout` or apply a connection timeout override.                  | `appsettings.json`, `Infrastructure/Configuration/InforVisualSettings.cs`, `Infrastructure/DependencyInjection/CoreServiceExtensions.cs`, `Module_Core/Data/InforVisual/Dao_InforVisualConnection.cs` |
| Shared throttle / bounded concurrency for Infor Visual access | Still open. I did not find a central semaphore, queue, rate limiter, or concurrency wrapper around `IService_InforVisual` in the live Infor Visual path. Callers still issue requests directly.                                                                                                                                                 | `Infrastructure/DependencyInjection/CoreServiceExtensions.cs`, `Module_Core/Contracts/Services/IService_InforVisual.cs`, `Module_Core/Services/Database/Service_InforVisualConnect.cs`                |
| Reconciliation batching or caching                            | Still open. `Service_ReceivingLocationReconciliation.ReconcileRowsAsync` still groups rows and then performs both `GetReceivingLocationEvidenceAsync` and `GetReceivingLocationTransactionHistoryAsync` for each receipt group.                                                                                                                 | `Module_Receiving/Services/Service_ReceivingLocationReconciliation.cs`                                                                                                                                |
| Manual-entry batching or caching                              | Still open. `ViewModel_Receiving_ManualEntry` still follows the same multi-step lookup chain for a row: exact part lookup, optional fuzzy part search, reload selected part, purchase-order lookup, and PO part resolution.                                                                                                                     | `Module_Receiving/ViewModels/ViewModel_Receiving_ManualEntry.cs`                                                                                                                                      |
| Real query-file-to-DAO integration tests                      | Still open. Current coverage remains unit and mock-data oriented. I did not find integration tests exercising `Dao_InforVisualConnection` against the real SQL query files or validating the live query-to-reader contract.                                                                                                                     | `MTM_Receiving_Application.Tests/Unit/Module_Core/Services/Database/Service_InforVisualConnectTests.cs`                                                                                               |

Date: 2026-04-09

Scope: deep audit of all current logic related to connecting to, querying against, queueing requests for, and disconnecting from the Infor Visual SQL Server integration.

Focus:

- connection lifecycle
- request fan-out and queue-spam risk
- shutdown and disconnect behavior
- ways the app could leave work in-flight, flood the server, or create operational damage
- gaps between configured safety expectations and actual runtime behavior

## Executive Summary

The current Infor Visual integration is materially safer than a typical ad hoc SQL integration, but it is not yet hardened against high-volume or shutdown-edge behavior.

What is good:

- All live Infor Visual access currently routes through read-only SQL Server connection strings using `ApplicationIntent=ReadOnly`.
- The main live DAO, `Dao_InforVisualConnection`, validates read-only intent in its constructor.
- Every inspected live Infor Visual DAO method opens a fresh `SqlConnection` with `await using`, then disposes the connection, command, and reader on the normal path.
- I found no live C# path that performs `INSERT`, `UPDATE`, `DELETE`, `MERGE`, or DDL against MTMFG.

What is still high risk:

- There is no application-level request throttle, semaphore, debounce, or queue for Infor Visual calls. Every caller is responsible for behaving well.
- The app now has a shared shutdown token service, but none of the Infor Visual DAO calls consume cancellation tokens. In-flight queries are not cooperatively cancelled during shutdown.
- Several user workflows create avoidable query fan-out. The most expensive is PO loading, which does one PO query and then re-queries the same PO once per part line to compute remaining quantity.
- The receiving reconciliation workflow performs two Infor Visual queries per receipt group, which can become a large round-trip count when scanning history.
- Configured Infor Visual timeout settings exist in configuration, but they are not applied to the actual SQL connections or commands.

Bottom line:

- I did not find a live write path that can damage the Infor Visual database directly.
- I did find multiple ways to create excess read load, repeated connection churn, long shutdown waits, and misleading operational assumptions.

## Audit Method

I audited:

- DI registration and connection string sourcing
- all Infor Visual DAOs and the main service wrapper
- the app shutdown path
- UI call sites that trigger Infor Visual lookups
- receiving, dunnage, outside service, ship/rec tools, and Volvo entry points that call `IService_InforVisual`
- SQL files used by those DAOs
- unit tests covering the service and reconciliation logic

Primary files reviewed:

- `App.xaml.cs`
- `Infrastructure/DependencyInjection/CoreServiceExtensions.cs`
- `Module_Core/Data/InforVisual/Dao_InforVisualConnection.cs`
- `Module_Core/Services/Database/Service_InforVisualConnect.cs`
- `Module_Core/Services/Service_ApplicationShutdown.cs`
- `Module_Receiving/Services/Service_ReceivingLocationReconciliation.cs`
- `Module_Receiving/ViewModels/ViewModel_Receiving_POEntry.cs`
- `Module_Receiving/ViewModels/ViewModel_Receiving_ManualEntry.cs`
- `Module_Receiving/ViewModels/ViewModel_Receiving_LoadEntry.cs`
- `Module_Dunnage/ViewModels/ViewModel_Dunnage_DetailsEntryViewModel.cs`
- `Module_Dunnage/ViewModels/ViewModel_Dunnage_EditModeViewModel.cs`
- `Module_ShipRec_Tools/Services/Service_Tool_MaterialAvailabilityBoard.cs`
- `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_MaterialAvailabilityBoard.cs`
- `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_OutsideServiceHistory.cs`
- `Module_OutsideService/Services/Service_OutsideService.cs`
- `Database/InforVisualScripts/Queries/*.sql`

## Connection Architecture

### Live connection path

The active runtime path is:

View / ViewModel / Feature Service -> `IService_InforVisual` -> `Service_InforVisualConnect` -> `Dao_InforVisualConnection` -> `SqlConnection`

Important detail:

- `IService_InforVisual` is registered as a singleton.
- `Dao_InforVisualConnection` is also registered as a singleton.
- This does not mean one long-lived `SqlConnection` is shared.
- Each DAO method creates a new `SqlConnection`, opens it, runs the query, then disposes it.

That means the runtime behavior is pooled short-lived connections, not one permanent open session.

### Read-only enforcement

Positive controls found:

- `ConnectionStrings:InforVisual` in `appsettings.json` contains `ApplicationIntent=ReadOnly`.
- `Dao_InforVisualConnection` rejects non-read-only connection strings.
- The older specialized DAOs `Dao_InforVisualPO` and `Dao_InforVisualPart` also validate read-only intent.

Operational caveat:

- The SQL login in `appsettings.json` is a shared account: `SHOP2`.
- This means server-side auditing cannot distinguish one app user from another based on SQL login identity alone.

## Disconnect and Shutdown Behavior

### What happens today

The application shutdown path is materially better than the older audit in `.github/audits/03262026-ShutdownAndConnectionCleanup-Audit.md`.

Current positives:

- `App.OnLaunched` calls `_host.StartAsync()`.
- Shutdown requests are coordinated through `IService_ApplicationShutdown`.
- `App.ShutdownCoreAsync` ends the user session, attempts `MySqlConnection.ClearAllPools()`, attempts `SqlConnection.ClearAllPools()`, calls `_host.StopAsync(...)`, and disposes the host.

### Remaining shutdown gap

The application has a shared shutdown token, but the Infor Visual SQL calls do not use it.

I found:

- no `CancellationToken` parameter on `IService_InforVisual`
- no `CancellationToken` passed into `SqlConnection.OpenAsync(...)`
- no `CancellationToken` passed into `ExecuteReaderAsync(...)` or `ExecuteScalarAsync(...)`

Practical effect:

- if the app begins shutdown while an Infor Visual query is already running, that query is not cooperatively cancelled by app intent
- cleanup depends on the query completing, timing out, or the process finishing shutdown

Risk level: medium

I did not find evidence that this will leave persistent SQL connections open forever under normal app shutdown. I did find evidence that shutdown is not able to actively stop work that is already underway.

## Timeout and Pooling Findings

### Config/runtime mismatch

Configured but not applied:

- `InforVisual.ConnectionTimeoutSeconds = 30`
- `InforVisual.QueryTimeoutSeconds = 120`

Actual runtime behavior today:

- the connection string does not include a connection timeout override, so `SqlConnection` uses the SQL client default connect timeout
- no `SqlCommand.CommandTimeout` is set, so commands use the provider default command timeout

Operational impact:

- production operators reading config will believe open timeout is 30 seconds and query timeout is 120 seconds
- the code is not actually honoring those values
- troubleshooting slow or hung server behavior will be harder because the observed timeout behavior will not match the configured values

Risk level: medium

### Pooling model

The app uses normal SQL client pooling semantics.

Important implications:

- disposing a `SqlConnection` normally returns it to the pool, not necessarily to a hard server disconnect
- the singleton service/DAO layer shares one connection string, so all calls share the same underlying pool
- there is no explicit per-feature throttling of pool consumption

Shutdown behavior:

- `SqlConnection.ClearAllPools()` is called during app shutdown
- this is good hygiene, but it still does not solve the lack of cooperative cancellation for in-flight operations

## Request Queueing and Fan-Out Risk

## No central request queue

I found no application-wide protection such as:

- `SemaphoreSlim` around Infor Visual access
- request deduplication
- debounce logic for repeated lookups
- a bounded work queue
- a shared concurrency limiter per feature or per app

That means all load protection depends on each caller's individual behavior.

## Caller-by-caller behavior

### Low risk: explicit-search tools

These only fire on explicit user action, not on every keypress:

- `View_Tool_MaterialAvailabilityBoard`
- `View_Tool_OutsideServiceHistory`

These are relatively safe from spam because the user must press Enter or invoke Search.

### Medium risk: lost-focus validation flows

These call Infor Visual when a field loses focus:

- `View_Receiving_LoadEntry.LocationTextBox_LostFocus`
- `View_Dunnage_DetailsEntryView.LocationTextBox_LostFocus`
- `View_Settings_Dunnage_UserPreferences.DefaultLocationTextBox_LostFocus`
- `View_Settings_Receiving_UserPreferences.IgnoredLocationTextBox_LostFocus`
- `View_Receiving_ManualEntry.LocationTextBox_LostFocus`

Pattern:

- validate exact location first
- if exact validation fails, issue a fuzzy search for suggestions

That means one blur can become two SQL round-trips:

- `LocationExistsAsync(...)`
- `FuzzySearchLocationsAsync(...)`

This is not keypress-spam, but it is easy for users to trigger repeatedly while tabbing through grids or revisiting fields.

### Medium risk: outside service modal double-validation

`View_OutsideService_AddLineModal` validates part existence on lost focus, and if `_isPartValidated` is still false, validates again when the primary button is clicked.

This can re-run the same validation in a short window.

Risk level: low to medium

### High risk: PO load N+1 query pattern

`ViewModel_Receiving_POEntry.LoadPOAsync` does this:

- call `GetPOWithPartsAsync(PoNumber)` once
- iterate every returned part line
- for each part line, call `GetRemainingQuantityAsync(PoNumber, part.PartID)`

`GetRemainingQuantityAsync` does not use a lightweight scalar query. It calls `Dao_InforVisualConnection.GetPOWithPartsAsync(poNumber)` again, then finds the matching line in memory.

Practical effect:

- one PO with 1 line -> 2 PO queries
- one PO with 10 lines -> 11 PO queries
- one PO with 50 lines -> 51 PO queries

This is the single most obvious avoidable server-load multiplier I found in the live code.

Risk level: high

### High risk: manual entry resolution can chain multiple server calls per row

`ViewModel_Receiving_ManualEntry.ResolveManualEntryRowAsync` and related helpers can produce a multi-step chain for one row:

- `GetPartByIDAsync(...)`
- if not found, `FuzzySearchPartsAsync(...)`
- then `GetPartByIDAsync(...)` again after selection
- `GetPurchaseOrdersByPartAsync(...)`
- `GetPOWithPartsAsync(...)`
- in other flows, `GetRemainingQuantityAsync(...)`

This is not uncontrolled parallel spam, but it is a high round-trip workflow and can be triggered repeatedly when the user tabs through many rows.

Risk level: high

### High risk: reconciliation preview is a round-trip multiplier

`Service_ReceivingLocationReconciliation.PreviewLocationsAsync` scans current rows and history rows, groups by `(PO, Part, PO line, ReceivedDate)`, and for each unique group does:

- `GetReceivingLocationEvidenceAsync(...)`
- `GetReceivingLocationTransactionHistoryAsync(...)`

This is two Infor Visual queries per unique receipt group.

Important nuance:

- this logic is sequential, not parallel, so it does not create a connection storm in the strict sense
- it absolutely can create a heavy server load over time when `includeAllHistory` is enabled and the receiving history is large

This is the second biggest operational read-load risk I found.

Risk level: high

### Medium risk: material availability board uses multiple large queries per search

`Service_Tool_MaterialAvailabilityBoard` does:

For location mode:

- `GetMaterialAvailabilityCurrentStockAsync(...)`
- `GetMaterialAvailabilityIncomingSupplyAsync(...)`
- `GetMaterialAvailabilityAssociatedPartRunsAsync(...)`

For part mode:

- `GetPartByIDAsync(...)`
- the same three board queries above

This is acceptable for an explicit search tool, but the underlying SQL for current stock and incoming supply is not row-capped with `TOP`, which means dense locations or broad part histories can return large result sets.

Risk level: medium

### Medium risk: intentional empty-term location pull

`ViewModel_Dunnage_EditModeViewModel.SelectLocationAsync` calls:

- `FuzzySearchLocationsAsync(string.Empty, "002")`

Because the service allows an empty term for locations, the DAO generates a `LIKE '%%'` style location search and returns the first 50 locations in warehouse `002`.

This appears intentional, but it means the app will hit the server even when no search term exists.

Risk level: medium

## SQL Query-Level Risks

### Good protections

These query types are capped:

- fuzzy part search: `TOP (@MaxResults)`
- fuzzy location search: `TOP (@MaxResults)`
- fuzzy PO-by-part search: `TOP (@MaxResults)`
- associated part runs: `TOP (@MaxResults)`

### Expensive query pattern in location fuzzy search

`11_FuzzySearchLocationsByWarehouse.sql` contains this predicate:

- `l.ID LIKE @Term`
- `OR REPLACE(REPLACE(UPPER(l.ID), '-', ''), ' ', '') LIKE @NormalizedTerm`

The second branch applies functions to the column, which usually prevents efficient index usage for that branch.

This is not a database corruption risk, but it is a performance risk if location suggestion traffic increases.

Risk level: medium

### Large-result queries without row caps

These queries do not use `TOP`:

- `18_GetMaterialAvailabilityCurrentStock.sql`
- `19_GetMaterialAvailabilityIncomingSupply.sql`
- the reconciliation evidence/history queries also appear intended to return all qualifying evidence rows for their group

This is acceptable only if callers remain narrow and infrequent.

Operational risk:

- the location-based board search and reconciliation workflows can become expensive on large locations or large histories

Risk level: medium to high, depending on dataset size

### Query helper is looser than the architecture rules

`Helper_SqlQueryLoader.ExtractQueryFromFile(...)` is willing to treat `SELECT`, `WITH`, `INSERT`, `UPDATE`, and `DELETE` as executable starts.

That does not create a live write path by itself, but it means the helper does not enforce the project rule that Infor Visual access must be read-only.

Today this is mitigated by:

- code review discipline
- DAO conventions
- read-only connection intent

But it is still a weak guardrail.

Risk level: medium

### Non-persistent temp-table activity exists in one SQL file

I found temp-table DDL and inserts in:

- `Database/InforVisualScripts/Queries/15_FindNonPartUserActivityByDate.sql`

Important distinction:

- this does not write to MTMFG user tables
- it does create tempdb work
- I did not find this query wired into the live `IService_InforVisual` call graph reviewed above

Risk level for MTMFG data damage: low

## Correctness Defects That Increase Operational Risk

### Broken associated-part-runs SQL/DAO contract

`Dao_InforVisualConnection.GetMaterialAvailabilityAssociatedPartRunsAsync(...)` reads:

- `reader["InputPartNumber"]`

But `20_GetMaterialAvailabilityAssociatedPartRuns.sql` does not project `InputPartNumber` in the final `SELECT`.

Impact:

- live execution of that DAO method against SQL Server should throw at runtime when not using mock data
- this is likely masked because current unit tests cover the mock-data path, not the real DAO/query contract
- repeated user retries would create repeated failing connections and log spam

Risk level: high for reliability, low for database damage

### Duplicate specialized DAOs are registered but not part of the live path

`Dao_InforVisualPO` and `Dao_InforVisualPart` exist and are registered, but I found no live consumers of them in the current codebase.

Operational concern:

- fixes to timeouts, cancellation, or connection policy can drift between unused and live DAOs
- maintainers may change the wrong DAO and believe the issue is solved

Risk level: medium for maintenance drift

## Database Damage Assessment

### Direct damage risk to MTMFG

Current direct damage risk appears low.

Why:

- live query paths are read-only
- connection strings use `ApplicationIntent=ReadOnly`
- no live write SQL path was found in the audited Infor Visual service stack

### Server health risk

Current server-health risk is not low.

The main ways the app can still hurt the server operationally are:

- repeated N+1 PO loading
- repeated manual-entry row resolution chains
- reconciliation preview over large histories
- location suggestion traffic with function-wrapped predicates
- explicit empty-term location pulls
- lack of central request throttling

This is read-load damage, not write corruption.

## Risk Ranking

### Critical

I did not find a confirmed critical live write path into MTMFG.

### High

1. PO loading N+1 pattern in `ViewModel_Receiving_POEntry.LoadPOAsync` plus `GetRemainingQuantityAsync`.
2. Receiving manual-entry row resolution creating multi-call chains per row.
3. Receiving reconciliation issuing two Infor Visual queries per unique receipt group.
4. `20_GetMaterialAvailabilityAssociatedPartRuns.sql` not matching the DAO reader contract.

### Medium

1. Shutdown token exists but is not wired into Infor Visual SQL calls.
2. Configured Infor Visual timeout settings are not applied.
3. No central request throttle or concurrency limiter.
4. Fuzzy location search uses a function-wrapped column branch that can degrade index use.
5. Material availability board queries can return large unbounded result sets.
6. Empty-term location lookup is allowed and used intentionally in Dunnage edit mode.
7. Shared SQL login hides user-level attribution.
8. Unused duplicate DAOs increase maintenance drift risk.

### Low

1. Temporary-table usage in query 15 affects tempdb, not MTMFG user data, and does not appear to be in the live audited path.
2. Explicit-search tools in Ship/Rec Tools are user-submitted, not keypress-driven.

## Testing Gaps

The current test suite is heavily mock-driven around `IService_InforVisual`.

What is missing:

- integration tests for the real `Dao_InforVisualConnection` against real SQL query files
- tests that assert configured timeout values are actually applied to commands
- tests that verify no live caller creates excess duplicate PO queries
- tests for shutdown during an in-flight Infor Visual query
- tests for the real `20_GetMaterialAvailabilityAssociatedPartRuns.sql` column contract

Practical consequence:

- several of the highest operational risks would not be caught by the current unit tests

## Final Assessment

If the question is, "Can the current code directly damage the Infor Visual database by writing to it?"

Answer: I found no evidence of that in the live path.

If the question is, "Can the current code create operational damage to the SQL Server by excessive reads, avoidable reconnection churn, or shutdown-edge behavior?"

Answer: yes.

The most important issues are not unclosed connections on the normal path. The code is generally good there.

The real risks are:

- too many queries for one user action
- no global concurrency/rate control
- no cooperative cancellation of in-flight SQL work during shutdown
- large workflows that multiply round-trips over history or many PO lines
- at least one broken SQL/DAO contract that will drive repeated failures in real mode

## Recommended Next Remediation Order

1. Fix the PO N+1 pattern first.
2. Fix the missing `InputPartNumber` projection in `20_GetMaterialAvailabilityAssociatedPartRuns.sql`.
3. Thread `IService_ApplicationShutdown.ShutdownToken` down into `IService_InforVisual`, `Service_InforVisualConnect`, and `Dao_InforVisualConnection`.
4. Actually apply configured Infor Visual connection and command timeouts.
5. Add a bounded concurrency strategy for the Infor Visual integration.
6. Consider batching or caching in reconciliation and manual-entry workflows.
7. Add integration tests for real query-file-to-DAO contracts.
