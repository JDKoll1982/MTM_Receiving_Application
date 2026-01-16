# Module Core — Settable Objects Report

This report lists **settable objects** (configuration, tunables, and hardcoded values that should be configurable) discovered in `Module_Core`.

## Settable objects

## Dev/Test

| Settable object | Type | Scope (User/System) | Recommended UI control | Description | Recommended permission to set | Current source | Recommendation |
|---|---:|---|---|---|---|---|---|
| `AppSettings:UseInforVisualMockData` | boolean | System | ToggleSwitch | If true, uses mock data instead of querying Infor Visual database. | Developer/QA only | `appsettings.json` + `Model_AppSettings.UseInforVisualMockData` | Force off in production builds/environments. |
| `AppSettings:DefaultMockPONumber` | string | System | TextBox | Default PO number used when mock mode is enabled. | Developer/QA only | `appsettings.json` | Avoid hardcoded fallbacks; require this key when mock mode is enabled. |
| `AppSettings:Environment` | string | System | ComboBox | Environment name (e.g., `Development`, `Production`). | IT/DevOps only | `appsettings.json` + `Model_AppSettings.Environment` | Prefer deployment-time configuration over code defaults. |

## Logging

| Settable object | Type | Scope (User/System) | Recommended UI control | Description | Recommended permission to set | Current source | Recommendation |
|---|---:|---|---|---|---|---|---|
| `Logging:LogLevel:*` | string | System | ComboBox | Logging level configuration (e.g., `Default=Information`, `Microsoft=Warning`). | IT/DevOps only | `appsettings.json` | Externalize per environment; avoid hardcoding levels in code. |

## Database

| Settable object | Type | Scope (User/System) | Recommended UI control | Description | Recommended permission to set | Current source | Recommendation |
|---|---:|---|---|---|---|---|---|
| `ConnectionStrings:MySQL` | string | System | No UI (deployment/secret store) | MySQL connection string for the application database. | IT/DevOps only | `appsettings.json` + `Helper_Database_Variables.ProductionConnectionString` | Remove credentials from source control; load from secrets/environment; keep one source of truth. |
| `Helper_Database_Variables.TestConnectionString` | string | System | No UI (deployment/secret store) | MySQL connection string for test database (`mtm_receiving_application_test`). | IT/DevOps only | `Module_Core/Helpers/Database/Helper_Database_Variables.cs` | Externalize to config; avoid hardcoding `Uid`/`Pwd` and database names. |

## ERP Integration (Infor Visual)

| Settable object | Type | Scope (User/System) | Recommended UI control | Description | Recommended permission to set | Current source | Recommendation |
|---|---:|---|---|---|---|---|---|
| `ConnectionStrings:InforVisual` | string | System | No UI (deployment/secret store) | SQL Server read-only connection string for Infor Visual (`ApplicationIntent=ReadOnly`). | IT/DevOps only | `appsettings.json` + `Helper_Database_Variables.GetInforVisualConnectionString()` | Do not hardcode credentials; load from secure store; enforce read-only intent everywhere. |
| `InforVisual:Server` | string | System | No UI (deployment config) | Infor Visual SQL Server host (default `VISUAL`). | IT/DevOps only | `Model_InforVisualConnection.Server` | Externalize and eliminate duplicated defaults across code/config. |
| `InforVisual:Database` | string | System | No UI (deployment config) | Infor Visual database name (default `MTMFG`). | IT/DevOps only | `Model_InforVisualConnection.Database` | Externalize and eliminate duplicated defaults across code/config. |
| `InforVisual:UserId` | string | System | No UI (deployment/secret store) | Infor Visual credential username (default `SHOP2`). | IT/DevOps only | `Model_InforVisualConnection.UserId` | Do not store in code; load from secure store. |
| `InforVisual:Password` | string | System | No UI (deployment/secret store) | Infor Visual credential password (default `SHOP`). | IT/DevOps only | `Model_InforVisualConnection.Password` | Do not store in code or `appsettings.json` for production. |
| `InforVisual:SiteId` (aka Warehouse) | string | System | TextBox | Default warehouse/site used for filtering (default `002`). | IT/DevOps only | `Model_InforVisualConnection.SiteId`, `Model_InforVisualPart.DefaultSite`, `Model_InforVisualPO.SiteId`, various DAOs | Centralize as a single config value; avoid scattering `"002"` across models/DAOs. |
| `InforVisual:DefaultUom` | string | System | TextBox | Default UOM values used in models/mock generation (e.g., `EA`). | Developer | Infor Visual models + mock generation | Prefer data-driven values from ERP; avoid defaulting unless required by UI. |
| `InforVisual:DefaultPartStatus` | string | System | TextBox | Default part status string (e.g., `ACTIVE`). | Developer | `Model_InforVisualPart.PartStatus` | Prefer actual status from ERP; default only when missing. |

## Security / Session

| Settable object | Type | Scope (User/System) | Recommended UI control | Description | Recommended permission to set | Current source | Recommendation |
|---|---:|---|---|---|---|---|---|
| `WorkstationConfig:SharedTerminalTimeoutMinutes` | int | System | NumberBox | Session timeout for shared terminals (currently 15 minutes). | Admin (Security) | `Model_WorkstationConfig.TimeoutDuration` | Store in DB/table or config instead of hardcoding. |
| `WorkstationConfig:PersonalWorkstationTimeoutMinutes` | int | System | NumberBox | Session timeout for personal workstations (currently 30 minutes). | Admin (Security) | `Model_WorkstationConfig.TimeoutDuration` | Store in DB/table or config instead of hardcoding. |
| `WorkstationConfig:WorkstationTypeValues` | string | System | No UI (internal constants) | Workstation type discriminators (`personal_workstation`, `shared_terminal`). | Developer (with Admin review) | `Model_WorkstationConfig.IsSharedTerminal/IsPersonalWorkstation` | Prefer enum or centrally-defined constants; validate values at ingestion. |
| `SessionMonitoring:TimerIntervalSeconds` | int | System | NumberBox | How often session timeout is checked (currently 60 seconds). | Admin (Security) | `Service_UserSessionManager.TimerIntervalSeconds` | Make configurable so high-traffic terminals can reduce interval. |

## Database Resilience

| Settable object | Type | Scope (User/System) | Recommended UI control | Description | Recommended permission to set | Current source | Recommendation |
|---|---:|---|---|---|---|---|---|
| `Database:StoredProcedureMaxRetries` | int | System | NumberBox | Retry count for transient MySQL stored procedure failures (currently 3). | Developer/DBA | `Helper_Database_StoredProcedure.MaxRetries` | Externalize; consider per-environment tuning. |
| `Database:StoredProcedureRetryDelaysMs` | int | System | TextBox | Backoff delays in ms (currently 100/200/400). | Developer/DBA | `Helper_Database_StoredProcedure.RetryDelaysMs` | Externalize; consider jitter and max delay settings. |

## UI/UX

| Settable object | Type | Scope (User/System) | Recommended UI control | Description | Recommended permission to set | Current source | Recommendation |
|---|---:|---|---|---|---|---|---|
| `Notifications:AutoDismissMs` | int | System | NumberBox | Auto-dismiss duration for informational/success status messages (currently 5000ms). | Admin (UI) | `Service_Notification.ShowStatus` | Externalize to config or theme constants. |
| `Startup:DelaysMs` | int | System | TextBox | Startup delays used for sequencing (e.g., 100/300/500ms). | Developer | `Service_OnStartup_AppLifecycle` | Replace with awaited signals where possible; if delays remain, make them configurable. |

## Hardcoded values that should not be hardcoded (high priority)

- Credentials and connection strings embedded in code (`Helper_Database_Variables`, `Model_InforVisualConnection`).
- Infor Visual site/warehouse value `"002"` duplicated across multiple models/DAOs.
- Retry/backoff constants for database calls (`MaxRetries`, `RetryDelaysMs`).
