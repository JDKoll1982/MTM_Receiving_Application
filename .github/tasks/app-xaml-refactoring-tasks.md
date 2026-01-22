# App.xaml.cs Refactoring Task List

## Phase 1: Foundation Setup

### 1.1 Create Infrastructure Folders
- [x] Create `Infrastructure/DependencyInjection/` folder
- [x] Create `Infrastructure/Configuration/` folder
- [x] Create `Infrastructure/Logging/` folder

### 1.2 Create Configuration Models
- [x] Create `Infrastructure/Configuration/DatabaseSettings.cs`
  - [x] Add `MySqlConnectionString` property
  - [x] Add `InforVisualConnectionString` property
  - [x] Add XML documentation
- [x] Create `Infrastructure/Configuration/InforVisualSettings.cs`
  - [x] Add `UseInforVisualMockData` property
  - [x] Add `ConnectionTimeout` property
  - [x] Add XML documentation
- [x] Create `Infrastructure/Configuration/ApplicationSettings.cs`
  - [x] Add general app settings properties
  - [x] Add XML documentation

### 1.3 Create appsettings.json
- [x] Create `appsettings.json` in project root
- [x] Add ConnectionStrings section
- [x] Add Serilog configuration section
- [x] Add InforVisual settings section
- [x] Add module-specific settings sections
- [x] Set "Copy to Output Directory" = "PreserveNewest"

### 1.4 Create Logging Configuration
- [x] Create `Infrastructure/Logging/SerilogConfiguration.cs`
- [x] Move hardcoded Serilog setup to static method
- [x] Support reading from IConfiguration
- [x] Add environment-specific configuration support

## Phase 2: Core Services Module

### 2.1 Create CoreServiceExtensions
- [x] Create `Infrastructure/DependencyInjection/CoreServiceExtensions.cs`
- [x] Add `AddCoreServices` extension method
- [x] Register error handler service (Singleton)
- [x] Register logging utility service (Singleton)
- [x] Register notification service (Singleton)
- [x] Register focus service (Singleton)
- [x] Register dispatcher service (Singleton)
- [x] Register window service (Singleton)
- [x] Register navigation service (Singleton)
- [x] Register help service (Singleton)
- [x] Add XML documentation for each registration
- [x] Add inline comments explaining lifetime choices

### 2.2 Register Authentication Services
- [x] Register User DAO (Singleton)
- [x] Register authentication service (Singleton)
- [x] Register user session manager (Singleton)
- [x] Register user preferences service (Transient)

### 2.3 Register CQRS Infrastructure
- [x] Register MediatR with pipeline behaviors
- [x] Register FluentValidation auto-discovery
- [x] Document pipeline behavior order

## Phase 3: Module Extension Methods

### 3.1 Create ReceivingModuleExtensions
- [x] Create `Module_Receiving/DependencyInjection/ReceivingModuleExtensions.cs` (Consolidated in ModuleServicesExtensions.cs)
- [x] Add `AddReceivingModule` extension method
- [x] Register Receiving DAOs (Singleton)
  - [x] Dao_ReceivingLoad
  - [x] Dao_ReceivingLine
  - [x] Dao_PackageTypePreference
- [x] Register Receiving Services (Singleton)
  - [x] IService_MySQL_Receiving
  - [x] IService_MySQL_ReceivingLine
  - [x] IService_MySQL_PackagePreferences
  - [x] IService_ReceivingValidation
  - [x] IService_ReceivingWorkflow
  - [x] IService_Pagination
  - [x] IService_SessionManager
  - [x] IService_CSVWriter
- [x] Register Receiving Settings Service (Singleton)
- [x] Register Receiving ViewModels (Transient)
  - [x] ViewModel_Receiving_Workflow
  - [x] ViewModel_Receiving_ModeSelection
  - [x] ViewModel_Receiving_ManualEntry
  - [x] ViewModel_Receiving_EditMode
  - [x] ViewModel_Receiving_POEntry
  - [x] ViewModel_Receiving_LoadEntry
  - [x] ViewModel_Receiving_WeightQuantity
  - [x] ViewModel_Receiving_HeatLot
  - [x] ViewModel_Receiving_PackageType
  - [x] ViewModel_Receiving_Review
- [x] Document service lifetimes with XML comments

### 3.2 Create DunnageModuleExtensions
- [x] Create `Module_Dunnage/DependencyInjection/DunnageModuleExtensions.cs` (Consolidated in ModuleServicesExtensions.cs)
- [x] Add `AddDunnageModule` extension method
- [x] Register Dunnage DAOs (Singleton)
  - [x] Dao_DunnageLoad
  - [x] Dao_DunnageType
  - [x] Dao_DunnagePart
  - [x] Dao_DunnageSpec
  - [x] Dao_InventoriedDunnage
  - [x] Dao_DunnageCustomField
  - [x] Dao_DunnageUserPreference
- [x] Register Dunnage Services (Singleton/Transient)
  - [x] IService_MySQL_Dunnage (Transient)
  - [x] IService_DunnageCSVWriter (Transient)
  - [x] IService_DunnageWorkflow (Singleton)
  - [x] IService_DunnageAdminWorkflow (Singleton)
- [x] Register Dunnage ViewModels (Transient)
  - [x] ViewModel_Dunnage_WorkFlowViewModel
  - [x] ViewModel_Dunnage_ModeSelection
  - [x] ViewModel_dunnage_typeselection
  - [x] ViewModel_Dunnage_PartSelection
  - [x] ViewModel_Dunnage_QuantityEntry
  - [x] ViewModel_Dunnage_DetailsEntry
  - [x] ViewModel_Dunnage_Review
  - [x] ViewModel_Dunnage_ManualEntry
  - [x] ViewModel_Dunnage_EditMode
  - [x] ViewModel_Dunnage_AdminMain
  - [x] ViewModel_Dunnage_AdminTypes
  - [x] ViewModel_Dunnage_AdminParts
  - [x] ViewModel_Dunnage_AdminInventory
  - [x] ViewModel_Dunnage_AddTypeDialog
- [x] Register Dunnage Views (Transient)
- [x] Document service lifetimes

### 3.3 Create RoutingModuleExtensions
- [x] Completed in consolidated ModuleServicesExtensions.cs
- [x] Add `AddRoutingModule` extension method
- [x] Register Routing DAOs (Singleton)
  - [x] Dao_RoutingLabel
  - [x] Dao_RoutingRecipient
  - [x] Dao_RoutingOtherReason
  - [x] Dao_RoutingUsageTracking
  - [x] Dao_RoutingUserPreference
  - [x] Dao_RoutingLabelHistory
  - [x] Dao_InforVisualPO (Routing)
- [x] Register Routing Services (Singleton)
  - [x] IRoutingService
  - [x] IRoutingInforVisualService
  - [x] IRoutingRecipientService
  - [x] IRoutingUsageTrackingService
  - [x] IRoutingUserPreferenceService
- [x] Register Routing ViewModels (Transient/Singleton)
  - [x] RoutingWizardContainerViewModel (Singleton)
  - [x] RoutingWizardStep1ViewModel (Transient)
  - [x] RoutingWizardStep2ViewModel (Transient)
  - [x] RoutingWizardStep3ViewModel (Transient)
  - [x] RoutingManualEntryViewModel (Transient)
  - [x] RoutingEditModeViewModel (Transient)
  - [x] RoutingModeSelectionViewModel (Transient)
- [x] Register Routing Views (Transient)
- [x] Document service lifetimes

### 3.4 Create VolvoModuleExtensions
- [x] Completed in consolidated ModuleServicesExtensions.cs
- [x] Add `AddVolvoModule` extension method
- [x] Register Volvo DAOs (Singleton)
  - [x] Dao_VolvoShipment
  - [x] Dao_VolvoShipmentLine
  - [x] Dao_VolvoPart
  - [x] Dao_VolvoPartComponent
- [x] Register Volvo Services (Singleton)
  - [x] IService_VolvoAuthorization
- [x] Register Volvo ViewModels (Transient)
  - [x] ViewModel_Volvo_ShipmentEntry
  - [x] ViewModel_Volvo_Settings
  - [x] ViewModel_Volvo_History
- [x] Register Volvo Views (Transient)
- [x] Document service lifetimes

### 3.5 Create ReportingModuleExtensions
- [x] Create `Module_Reporting/DependencyInjection/ReportingModuleExtensions.cs` (Consolidated in ModuleServicesExtensions.cs)
- [x] Add `AddReportingModule` extension method
- [x] Register Reporting DAOs (Singleton)
  - [x] Dao_Reporting
- [x] Register Reporting Services (Singleton)
  - [x] IService_Reporting
- [x] Register Reporting ViewModels (Transient)
  - [x] ViewModel_Reporting_Main
- [x] Register Reporting Views (Transient)
  - [x] View_Reporting_Main
- [x] Document service lifetimes

### 3.6 Create SettingsModuleExtensions
- [x] Create `Module_Settings/DependencyInjection/SettingsModuleExtensions.cs` (Consolidated in ModuleServicesExtensions.cs)
- [x] Add `AddSettingsModule` extension method
- [x] Register Settings Core DAOs (Singleton)
  - [x] Dao_SettingsCoreSystem
  - [x] Dao_SettingsCoreUser
  - [x] Dao_SettingsCoreAudit
  - [x] Dao_SettingsCoreRoles
  - [x] Dao_SettingsCoreUserRoles
  - [x] Dao_SettingsDiagnostics
- [x] Register Settings Core Services (Singleton)
  - [x] ISettingsManifestProvider
  - [x] ISettingsMetadataRegistry
  - [x] ISettingsCache
  - [x] ISettingsEncryptionService
  - [x] IService_SettingsCoreFacade
  - [x] IService_SettingsWindowHost
  - [x] IService_ViewModelRegistry
  - [x] IService_SettingsPagination
- [x] Register Settings ViewModels (Transient) - All navigation hubs and feature pages
- [x] Register Settings Views (Transient) - All navigation hubs and feature pages
- [x] Document service lifetimes

### 3.7 Create SharedModuleExtensions
- [x] Create `Module_Shared/DependencyInjection/SharedModuleExtensions.cs` - Consolidated into ModuleServicesExtensions
- [x] Add `AddSharedModule` extension method
- [x] Register Shared ViewModels (Transient)
  - [x] ViewModel_Shared_MainWindow
  - [x] ViewModel_Shared_SplashScreen
  - [x] ViewModel_Shared_SharedTerminalLogin
  - [x] ViewModel_Shared_NewUserSetup
  - [x] ViewModel_Shared_HelpDialog
- [x] Register Shared Views (Transient/Singleton)
  - [x] MainWindow (Singleton)
  - [x] View_Shared_SplashScreenWindow (Transient) - **FIXED 2026-01-21**
  - [x] View_Shared_HelpDialog (Transient) - **ADDED 2026-01-21**
  - [x] View_Shared_SharedTerminalLoginDialog (Transient) - **ADDED 2026-01-21**
  - [x] View_Shared_NewUserSetupDialog (Transient) - **ADDED 2026-01-21**
- [x] Document service lifetimes

### 3.8 Create InforVisualExtensions
- [x] Create `Infrastructure/DependencyInjection/InforVisualExtensions.cs` (Merged into CoreServiceExtensions.cs)
- [x] Add `AddInforVisualServices` extension method
- [x] Register Infor Visual DAOs (Singleton)
  - [x] Dao_InforVisualConnection
  - [x] Dao_InforVisualPart
- [x] Register Infor Visual Services (Singleton)
  - [x] IService_InforVisual
- [x] Use IOptions<InforVisualSettings> for configuration
- [x] Document service lifetimes

## Phase 4: Service Locator Removal

### 4.1 Find Service Locator Usages
- [x] NOTE: Service locator method `GetService<T>()` was not found in current codebase
- [x] Already removed or never implemented
- [x] Pattern successfully avoided in refactored code

### 4.2 Refactor Service Locator Calls
- [x] N/A - No service locator calls found in codebase

### 4.3 Delete Service Locator Method
- [x] Verified no `GetService<T>()` method exists in App.xaml.cs
- [x] Constructor injection used throughout codebase

## Phase 5: App.xaml.cs Refactoring

### STATUS: ✅ COMPLETED

All refactoring objectives achieved:
- [x] App.xaml.cs reduced to 105 lines (78% reduction)
- [x] Serilog configured from appsettings.json
- [x] All services use modular extension methods
- [x] Configuration externalized
- [x] Service lifetimes documented
- [x] appsettings.json created with all sections

### 5.1 Update App Constructor
- [x] Move Serilog configuration to helper method
- [x] Use `SerilogConfiguration.Configure()`
- [x] Remove hardcoded logging setup

### 5.2 Update ConfigureServices Method
- [x] Replace all service registrations with extension method calls
- [x] Add configuration registration (IOptions)
  - [x] `services.Configure<DatabaseSettings>(...)`
  - [x] `services.Configure<InforVisualSettings>(...)`
  - [x] `services.Configure<ApplicationSettings>(...)`
- [x] Call module extension methods
  - [x] `services.AddCoreServices()`
  - [x] `services.AddReceivingModule()`
  - [x] `services.AddDunnageModule()`
  - [x] `services.AddRoutingModule()`
  - [x] `services.AddVolvoModule()`
  - [x] `services.AddReportingModule()`
  - [x] `services.AddSettingsModule()`
  - [x] `services.AddSharedModule()`
  - [x] `services.AddInforVisualServices()`
- [x] Verify line count is < 150 lines

### 5.3 Update Connection String Retrieval
- [x] Replace `Helper_Database_Variables.GetConnectionString()` calls
- [x] Use `IConfiguration.GetConnectionString("MySql")`
- [x] Update all DAOs to receive connection string from configuration
- [x] Update extension methods to use IConfiguration

### 5.4 Add XML Documentation
- [x] Add summary to App class
- [x] Add summary to constructor
- [x] Add summary to OnLaunched method
- [x] Add summary to event handlers

## Phase 6: Configuration Files

### 6.1 Create appsettings.json
- [x] Create file with proper structure
- [x] Add ConnectionStrings section
- [x] Add Serilog configuration
- [x] Add InforVisual settings
- [x] Add module-specific settings
- [x] Mark as "Copy to Output Directory"

### 6.2 Create appsettings.Development.json
- [x] Override Production settings
- [x] Add development-specific connection strings
- [x] Add development-specific logging
- [x] Mark as "Copy to Output Directory"

### 6.3 Update .csproj
- [x] Add appsettings.json as Content
- [x] Add appsettings.Development.json as Content
- [x] Set CopyToOutputDirectory for both

## Phase 7: Build and Validation

### 7.1 Incremental Build Testing
- [x] Build after Phase 1 completion
- [x] Build after Phase 2 completion
- [x] Build after each module extension (Phase 3)
- [x] Build after Service Locator removal (Phase 4)
- [x] Build after App.xaml.cs refactoring (Phase 5)

### 7.2 Runtime Testing
- [ ] Start application
- [ ] Verify splash screen shows
- [ ] Verify login works
- [ ] Verify main window loads
- [ ] Test Receiving workflow
- [ ] Test Dunnage workflow
- [ ] Test Routing workflow
- [ ] Test Settings window
- [ ] Verify logging works
- [ ] Verify configuration loads correctly

### 7.3 Final Verification
- [x] App.xaml.cs is < 150 lines
- [x] No service locator calls remain
- [x] All services documented with XML comments
- [x] Configuration externalized to appsettings.json
- [x] All modules have extension methods
- [x] Build succeeds with zero warnings (in refactored code)

## Phase 8: Documentation and Cleanup

### 8.1 Update README
- [ ] Document new DI organization
- [ ] Document configuration file structure
- [ ] Document how to add new services
- [ ] Document service lifetime guidelines

### 8.2 Create Developer Guide
- [ ] Create `docs/DependencyInjection.md`
- [ ] Document extension method pattern
- [ ] Document Options Pattern usage
- [ ] Document service lifetime choices
- [ ] Provide examples

### 8.3 Git Commit Strategy
- [ ] Commit after Phase 1 (Foundation)
- [ ] Commit after Phase 2 (Core Services)
- [ ] Commit after each module extension
- [ ] Commit after Service Locator removal
- [ ] Commit after App.xaml.cs refactoring
- [ ] Final commit with documentation

## Success Metrics

- [ ] **Line Count**: App.xaml.cs reduced from 481 to < 150 lines
- [ ] **Modularity**: 8+ extension method files created
- [ ] **Anti-patterns**: Zero service locator calls
- [ ] **Configuration**: 100% externalized to JSON
- [ ] **Documentation**: All services have lifetime justification
- [ ] **Build**: Clean build with no new warnings
- [ ] **Tests**: Application runs without errors

---

**Total Tasks**: 150+  
**Estimated Time**: 2-3 hours  
**Priority**: Critical  
**Status**: Ready to Execute

**Next Action**: Start Phase 1, Task 1.1
