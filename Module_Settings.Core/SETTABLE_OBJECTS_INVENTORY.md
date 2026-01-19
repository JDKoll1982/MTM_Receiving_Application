# Settable Objects Inventory - Module_Core

Use this inventory to track all settings/configuration values discovered in `Module_Core`.

| Setting Key | Data Type | Scope | UI Control | Description | Permission | Default | Notes |
| ----------- | --------- | ----- | ---------- | ----------- | ---------- | ------- | ----- |
| Core:Database:MySql:ProductionConnectionString | String | System | (Not Implemented) | MySQL connection string used for production environment. | Developer | `Server=localhost;Port=3306;Database=mtm_receiving_application;Uid=root;Pwd=root;CharSet=utf8mb4;` | Source: `Module_Core/Helpers/Database/Helper_Database_Variables.cs` (`ProductionConnectionString`). Sensitive; should be moved to User Secrets / external configuration. |
| Core:Database:MySql:TestConnectionString | String | System | (Not Implemented) | MySQL connection string used for test/non-production environment. | Developer | `Server=localhost\u003bPort=3306\u003bDatabase=mtm_receiving_application_test\u003bUid=root\u003bPwd=root\u003bCharSet=utf8mb4\u003b` | Source: `Module_Core/Helpers/Database/Helper_Database_Variables.cs` (`TestConnectionString`). Sensitive; should be moved to User Secrets / external configuration. |
| Core:Database:InforVisual:ConnectionString | String | System | (Not Implemented) | Infor Visual SQL Server **READ ONLY** connection string used by integration DAOs/services. | Developer | `Server=VISUAL;Database=MTMFG;User Id=SHOP2;Password=SHOP;TrustServerCertificate=True;ApplicationIntent=ReadOnly;` | Source: `Module_Core/Helpers/Database/Helper_Database_Variables.cs` (`GetInforVisualConnectionString`). Sensitive; should be moved to external configuration. Must remain `ApplicationIntent=ReadOnly`. |
| Core:InforVisual:Defaults:Server | String | System | (Not Implemented) | Default Infor Visual server name. | Developer | `VISUAL` | Source: `Module_Core/Defaults/InforVisualDefaults.cs` (`DefaultServer`). Used as default for `Model_InforVisualConnection.Server`. |
| Core:InforVisual:Defaults:Database | String | System | (Not Implemented) | Default Infor Visual database name. | Developer | `MTMFG` | Source: `Module_Core/Defaults/InforVisualDefaults.cs` (`DefaultDatabase`). Used as default for `Model_InforVisualConnection.Database`. |
| Core:InforVisual:Defaults:SiteId | String | System | (Not Implemented) | Default Site/Warehouse identifier for Infor Visual queries. | Developer | `002` | Source: `Module_Core/Defaults/InforVisualDefaults.cs` (`DefaultSiteId`). Used as defaults on models such as `Model_InforVisualPO.SiteId`. |
| Core:InforVisual:Defaults:Uom | String | System | (Not Implemented) | Default Unit of Measure for Infor Visual models. | Developer | `EA` | Source: `Module_Core/Defaults/InforVisualDefaults.cs` (`DefaultUom`). |
| Core:InforVisual:Defaults:PartStatus | String | System | (Not Implemented) | Default Part Status text for Infor Visual models. | Developer | `ACTIVE` | Source: `Module_Core/Defaults/InforVisualDefaults.cs` (`DefaultPartStatus`). |
| Core:Workstation:Type:SharedTerminal | String | System | (Not Implemented) | Workstation type identifier for shared terminals. | Developer | `shared_terminal` | Source: `Module_Core/Defaults/WorkstationDefaults.cs` (`SharedTerminalWorkstationType`). Used by `Model_WorkstationConfig.IsSharedTerminal`. |
| Core:Workstation:Type:PersonalWorkstation | String | System | (Not Implemented) | Workstation type identifier for personal workstations. | Developer | `personal_workstation` | Source: `Module_Core/Defaults/WorkstationDefaults.cs` (`PersonalWorkstationWorkstationType`). Used by `Model_WorkstationConfig.IsPersonalWorkstation`. |
| Core:Workstation:TimeoutMinutes:SharedTerminal | Int32 | System | (Not Implemented) | Session timeout (minutes) for shared terminals. | Developer | `15` | Source: `Module_Core/Defaults/WorkstationDefaults.cs` (`SharedTerminalTimeoutMinutes`). Used by `Model_WorkstationConfig.TimeoutDuration`. |
| Core:Workstation:TimeoutMinutes:PersonalWorkstation | Int32 | System | (Not Implemented) | Session timeout (minutes) for personal workstations. | Developer | `30` | Source: `Module_Core/Defaults/WorkstationDefaults.cs` (`PersonalWorkstationTimeoutMinutes`). Used by `Model_WorkstationConfig.TimeoutDuration`. |
| Core:AppSettings:UseInforVisualMockData | Boolean | System | (Not Implemented) | Use mock data instead of querying the Infor Visual DB (developer/testing helper). | Developer | `true` | Source: `Module_Core/Models/Systems/Model_AppSettings.cs` (`UseInforVisualMockData`). Documented as loaded from `appsettings.json`. |
| Core:AppSettings:Environment | String | System | (Not Implemented) | Environment name (Development, Production, etc.). | Developer | `Development` | Source: `Module_Core/Models/Systems/Model_AppSettings.cs` (`Environment`). Documented as loaded from `appsettings.json`. |
| Core:Session:FilePath | String | User | (Not Implemented) | Path to persisted session JSON file used for restoring workflow state. | User | `%APPDATA%\\MTM_Receiving_Application\\session.json` | Source: `Module_Core/Contracts/Services/IService_SessionManager.cs` (docstring). Actual implementation/service should confirm final path and behavior. |

| Core:Logging:Directory | String | User | (Not Implemented) | Root directory where application logs are written. | User | `%APPDATA%\\MTM_Receiving_Application\\Logs` | Source: `Module_Core/Services/Database/Service_LoggingUtility.cs` (`_logDirectory`). |
| Core:Logging:RetentionDays | Int32 | User | (Not Implemented) | How many days of logs to keep before archiving. | Supervisor | `30` | Source: `Module_Core/Contracts/Services/IService_LoggingUtility.cs` and `Module_Core/Services/Database/Service_LoggingUtility.cs` (`ArchiveOldLogs(int daysToKeep = 30)`). |
| Core:Database:MySql:StoredProcedureRetries:MaxRetries | Int32 | System | (Not Implemented) | Maximum retry attempts for transient MySQL stored procedure failures. | Developer | `3` | Source: `Module_Core/Helpers/Database/Helper_Database_StoredProcedure.cs` (`MaxRetries`). |
| Core:Database:MySql:StoredProcedureRetries:RetryDelaysMs | Int32[] | System | (Not Implemented) | Backoff delays (ms) between transient retry attempts for stored procedure calls. | Developer | `[100, 200, 400]` | Source: `Module_Core/Helpers/Database/Helper_Database_StoredProcedure.cs` (`RetryDelaysMs`). |
| Core:Notifications:AutoDismissMs | Int32 | User | (Not Implemented) | Auto-dismiss duration for informational/success status messages. | User | `5000` | Source: `Module_Core/Services/Service_Notification.cs` (`Task.Delay(5000)`). |
| Core:Session:TimeoutMonitorIntervalSeconds | Int32 | System | (Not Implemented) | How frequently the app checks for a timed-out user session. | Developer | `60` | Source: `Module_Core/Services/Authentication/Service_UserSessionManager.cs` (`TimerIntervalSeconds`). |

## Notes

- Keys must be unique within the module.
- Scope values: System or User.
- Permission levels: User, Supervisor, Admin, Developer.
- Sensitive values must be masked in UI and changed via dedicated dialog.

## Inventory Method

This inventory was generated by scanning `Module_Core` for:
- Defaults in `Module_Core/Defaults/*.cs`
- Configuration models like `Model_AppSettings`
- Helper methods that provide connection strings and other environment-dependent values
- Service contracts that document persistence locations
