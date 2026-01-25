# MTM Receiving Application - Source Tree Analysis

## Project Root Structure

```
MTM_Receiving_Application/
├── App.xaml.cs                          # Application entry point, DI container setup
├── MainWindow.xaml                      # Root window
├── appsettings.json                     # Configuration settings
├── Package.appxmanifest                 # Windows App packaging manifest
│
├── Module_Core/                         # Core infrastructure and shared services
│   ├── Contracts/                       # Service interfaces
│   │   └── Services/                    # IService_* interfaces
│   ├── Data/                            # Core DAOs
│   │   ├── Authentication/              # User authentication DAOs
│   │   └── InforVisual/                 # ERP integration DAOs (READ-ONLY)
│   ├── Helpers/                         # Utility classes
│   │   └── Database/                    # Database connection helpers
│   ├── Models/                          # Core domain models
│   │   └── Systems/                     # System-level models
│   ├── Services/                        # Core service implementations
│   │   ├── Authentication/              # Auth services
│   │   ├── Database/                    # Database services
│   │   ├── Help/                        # Help system
│   │   ├── Navigation/                  # Navigation service
│   │   ├── Startup/                     # Application lifecycle
│   │   └── UI/                          # UI utilities
│   ├── ViewModels/                      # Core ViewModels
│   │   └── Main/                        # Main window ViewModels
│   ├── Views/                           # Core Views
│   │   └── Main/                        # Main pages (Receiving/Dunnage/Carrier labels)
│   ├── Themes/                          # XAML themes and styles
│   └── Converters/                      # XAML value converters
│
├── Module_Receiving/                    # Receiving workflow module
│   ├── Data/                            # Receiving DAOs
│   │   ├── Dao_ReceivingLoad.cs         # Load transactions
│   │   ├── Dao_ReceivingLine.cs         # Line items
│   │   └── Dao_PackageTypePreference.cs # User preferences
│   ├── Enums/                           # Receiving-specific enumerations
│   ├── Interfaces/                      # Receiving service contracts
│   ├── Models/                          # Receiving domain models
│   │   ├── Model_ReceivingLoad.cs       # Header model
│   │   ├── Model_ReceivingLine.cs       # Line model
│   │   ├── Model_ReceivingSession.cs    # Workflow state
│   │   ├── Model_InforVisualPO.cs       # ERP PO data
│   │   └── Model_InforVisualPart.cs     # ERP part data
│   ├── Services/                        # Receiving business logic
│   │   ├── Service_ReceivingWorkflow.cs # Main workflow orchestration
│   │   ├── Service_ReceivingValidation.cs
│   │   ├── Service_MySQL_Receiving.cs
│   │   └── Service_CSVWriter.cs         # Label generation
│   ├── ViewModels/                      # Receiving ViewModels (15 workflow steps)
│   │   ├── ViewModel_Receiving_Workflow.cs
│   │   ├── ViewModel_Receiving_ModeSelection_Display_ModeSelection.cs
│   │   ├── Old_ViewModel_Receiving_Wizard_Display_PoEntry.cs
│   │   ├── ViewModel_Receiving_LoadEntry.cs
│   │   ├── ViewModel_Receiving_WeightQuantity.cs
│   │   ├── ViewModel_Receiving_HeatLot.cs
│   │   ├── ViewModel_Receiving_PackageType.cs
│   │   ├── ViewModel_Receiving_Review.cs
│   │   ├── ViewModel_Receiving_Manual_Display_DataEntry.cs
│   │   └── ViewModel_Receiving_EditMode_Interaction_EditHandler.cs
│   └── Views/                           # Receiving XAML Views
│       ├── View_Receiving_Workflow.xaml
│       ├── View_Receiving_ModeSelection_Display_ModeSelection.xaml
│       ├── Old_View_Receiving_Wizard_Display_PoEntry.xaml
│       ├── View_Receiving_LoadEntry.xaml
│       ├── View_Receiving_WeightQuantity.xaml
│       ├── View_Receiving_HeatLot.xaml
│       ├── View_Receiving_PackageType.xaml
│       ├── View_Receiving_Review.xaml
│       ├── View_Receiving_Manual_Display_DataEntry.xaml
│       └── View_Receiving_EditMode.xaml
│
├── Module_Dunnage/                      # Dunnage (returnable packaging) module
│   ├── Data/                            # Dunnage DAOs
│   │   ├── Dao_DunnageLoad.cs           # Transaction log
│   │   ├── Dao_DunnageType.cs           # Packaging types
│   │   ├── Dao_DunnagePart.cs           # Part numbers
│   │   ├── Dao_DunnageSpec.cs           # Specifications
│   │   ├── Dao_InventoriedDunnage.cs    # Inventory tracking
│   │   └── Dao_DunnageCustomField.cs    # Extensible metadata
│   ├── Enums/                           # Dunnage enumerations
│   ├── Interfaces/                      # Dunnage service contracts
│   ├── Models/                          # Dunnage domain models
│   │   ├── Model_DunnageLoad.cs
│   │   ├── Model_DunnageLine.cs
│   │   ├── Model_DunnageType.cs
│   │   ├── Model_DunnagePart.cs
│   │   ├── Model_DunnageSpec.cs
│   │   ├── Model_InventoriedDunnage.cs
│   │   └── Model_DunnageSession.cs      # Workflow state
│   ├── Services/                        # Dunnage business logic
│   │   ├── Service_DunnageWorkflow.cs   # User workflow
│   │   ├── Service_DunnageAdminWorkflow.cs # Admin operations
│   │   ├── Service_MySQL_Dunnage.cs
│   │   └── Service_DunnageCSVWriter.cs
│   ├── ViewModels/                      # Dunnage ViewModels (20+ views)
│   │   ├── ViewModel_Dunnage_ModeSelection.cs
│   │   ├── ViewModel_dunnage_typeselection.cs
│   │   ├── ViewModel_Dunnage_PartSelection.cs
│   │   ├── ViewModel_Dunnage_QuantityEntry.cs
│   │   ├── ViewModel_Dunnage_DetailsEntry.cs
│   │   ├── ViewModel_Dunnage_Review.cs
│   │   ├── ViewModel_Dunnage_AdminMain.cs
│   │   ├── ViewModel_Dunnage_AdminTypes.cs
│   │   ├── ViewModel_Dunnage_AdminParts.cs
│   │   └── ViewModel_Dunnage_AdminInventory.cs
│   └── Views/                           # Dunnage XAML Views
│       ├── View_Dunnage_WorkflowView.xaml
│       ├── View_Dunnage_ModeSelectionView.xaml
│       ├── View_dunnage_typeselectionView.xaml
│       ├── View_Dunnage_PartSelectionView.xaml
│       ├── View_Dunnage_QuantityEntryView.xaml
│       ├── View_Dunnage_DetailsEntryView.xaml
│       ├── View_Dunnage_ReviewView.xaml
│       ├── View_Dunnage_AdminMainView.xaml
│       ├── View_Dunnage_AdminTypesView.xaml
│       ├── View_Dunnage_AdminPartsView.xaml
│       ├── View_Dunnage_AdminInventoryView.xaml
│       └── View_Dunnage_Dialog_*.xaml   # Various dialogs
│
├── Module_Shared/                       # Shared UI components
│   ├── Enums/                           # Shared enumerations
│   ├── Interfaces/                      # Shared contracts
│   ├── Models/                          # Shared models
│   ├── Services/                        # Shared services
│   ├── ViewModels/                      # Shared ViewModels
│   │   ├── ViewModel_Shared_MainWindow.cs
│   │   ├── ViewModel_Shared_SplashScreen.cs
│   │   ├── ViewModel_Shared_SharedTerminalLogin.cs
│   │   └── ViewModel_Shared_NewUserSetup.cs
│   └── Views/                           # Shared XAML components
│       ├── View_Shared_SplashScreenWindow.xaml
│       ├── View_Shared_SharedTerminalLoginDialog.xaml
│       ├── View_Shared_NewUserSetupDialog.xaml
│       ├── View_Shared_HelpDialog.xaml
│       └── View_Shared_IconSelectorWindow.xaml
│
├── Module_Settings/                     # Application settings module
│   ├── Enums/                           # Settings enumerations
│   ├── Interfaces/                      # Settings service contracts
│   ├── Models/                          # Settings models
│   ├── Services/                        # Settings services
│   ├── ViewModels/                      # Settings ViewModels
│   │   ├── ViewModel_Settings_Workflow.cs
│   │   ├── ViewModel_Settings_ModeSelection.cs
│   │   └── ViewModel_Settings_DunnageMode.cs
│   └── Views/                           # Settings XAML Views
│       ├── View_Settings_Workflow.xaml
│       ├── View_Settings_ModeSelection.xaml
│       └── View_Settings_DunnageMode.xaml
│
├── Module_Routing/                      # Routing module
│   ├── Archived_Code_Reviews/           # Code review archives
│   ├── Constants/                       # Routing constants
│   ├── Converters/                      # Routing-specific converters
│   ├── Data/                            # Routing DAOs
│   │   ├── Dao_InforVisualPO.cs         # Shared PO access
│   │   ├── Dao_RoutingLabel.cs          # Label data access
│   │   ├── Dao_RoutingLabelHistory.cs   # Label history
│   │   ├── Dao_RoutingOtherReason.cs    # Reason codes
│   │   ├── Dao_RoutingRecipient.cs      # Recipients
│   │   ├── Dao_RoutingUsageTracking.cs  # Usage tracking
│   │   └── Dao_RoutingUserPreference.cs # User preferences
│   ├── Database/                        # Routing database scripts
│   ├── Enums/                           # Routing enumerations
│   ├── Interfaces/                      # Routing contracts
│   ├── Models/                          # Routing models
│   ├── Services/                        # Routing services
│   │   ├── IRoutingService.cs
│   │   └── RoutingService.cs
│   ├── ViewModels/                      # Routing ViewModels
│   │   ├── RoutingWizardContainerViewModel.cs
│   │   └── RoutingManualEntryViewModel.cs
│   └── Views/                           # Routing Views
│       ├── RoutingWizardContainerView.xaml
│       └── RoutingManualEntryView.xaml
│
├── Module_Volvo/                        # Volvo integration module
│   ├── Data/                            # Volvo DAOs
│   │   ├── Dao_VolvoPart.cs
│   │   ├── Dao_VolvoPartComponent.cs
│   │   ├── Dao_VolvoSettings.cs
│   │   ├── Dao_VolvoShipment.cs
│   │   └── Dao_VolvoShipmentLine.cs
│   ├── Models/                          # Volvo models
│   ├── Interfaces/                      # Volvo contracts
│   ├── Services/                        # Volvo services
│   │   ├── Service_Volvo.cs
│   │   ├── Service_VolvoAuthorization.cs
│   │   └── Service_VolvoMasterData.cs
│   ├── ViewModels/                      # Volvo ViewModels
│   │   ├── ViewModel_Volvo_ShipmentEntry.cs
│   │   ├── ViewModel_Volvo_History.cs
│   │   └── ViewModel_Volvo_Settings.cs
│   └── Views/                           # Volvo Views
│       ├── View_Volvo_ShipmentEntry.xaml
│       ├── View_Volvo_History.xaml
│       └── View_Volvo_Settings.xaml
│
├── Module_Reporting/                    # Reporting module
│   ├── Data/                            # Reporting DAOs
│   │   └── Dao_Reporting.cs
│   ├── Services/                        # Reporting services
│   │   └── Service_Reporting.cs
│   ├── ViewModels/                      # Reporting ViewModels
│   │   └── ViewModel_Reporting_Main.cs
│   └── Views/                           # Reporting Views
│       └── View_Reporting_Main.xaml
│
├── Database/                            # SQL schemas and stored procedures
│   ├── Schemas/                         # Table definitions (MySQL)
│   │   ├── 01_create_receiving_tables.sql
│   │   ├── 02_create_authentication_tables.sql
│   │   ├── 03_create_receiving_tables.sql
│   │   ├── 04_create_package_preferences.sql
│   │   ├── 05_add_default_mode_to_users.sql
│   │   ├── 06_create_dunnage_tables.sql
│   │   ├── 06_seed_dunnage_data.sql
│   │   ├── 07_create_dunnage_tables_v2.sql
│   │   ├── 08_add_icon_to_dunnage_types.sql
│   │   └── 09_fix_bad_icon_data.sql
│   ├── StoredProcedures/                # MySQL stored procedures
│   │   ├── Authentication/              # User auth procedures
│   │   │   ├── sp_Auth_User_GetByWindowsUsername.sql
│   │   │   ├── sp_Auth_User_ValidatePin.sql
│   │   │   ├── sp_Auth_User_Create.sql
│   │   │   └── sp_update_user_default_*.sql
│   │   ├── Receiving/                   # Receiving procedures
│   │   │   ├── sp_receiving_history_*.sql
│   │   │   ├── sp_receiving_lines_*.sql
│   │   │   └── sp_package_preferences_*.sql
│   │   └── Dunnage/                     # Dunnage procedures
│   │       ├── sp_dunnage_history_*.sql
│   │       ├── sp_dunnage_parts_*.sql
│   │       ├── sp_dunnage_types_*.sql
│   │       └── sp_dunnage_requires_inventory_*.sql
│   ├── Migrations/                      # Migration scripts
│   │   ├── 010-dunnage-complete-schema.sql
│   │   ├── 011_migrate_icons_to_material.sql
│   │   └── 012_add_home_location_to_dunnage_parts.sql
│   ├── InforVisualScripts/              # ERP integration (READ-ONLY)
│   │   ├── README.md
│   │   └── Queries/                     # Sample SQL Server queries
│   │       ├── 01_GetPOWithParts.sql
│   │       ├── 02_ValidatePONumber.sql
│   │       ├── 03_GetPartByNumber.sql
│   │       └── 04_SearchPartsByDescription.sql
│   ├── InforVisualTest/                 # Test queries for ERP
│   ├── TestData/                        # Sample data for development
│   │   ├── insert_test_data.sql
│   │   ├── configure_shared_terminal.sql
│   │   └── 010_seed_dunnage_complete.sql
│   └── Deploy/                          # Deployment scripts
│       └── Deploy-Database.ps1
│
├── Assets/                              # Application assets
│   ├── Fonts/                           # Custom fonts
│   ├── app-icon.ico                     # Application icon
│   └── *.png                            # Splash/logo images
│
├── Scripts/                             # PowerShell utility scripts
│   └── Visualize-Build.ps1
│
├── specs/                               # Feature specifications (unimplemented)
│   ├── 001-routing-module/              # Routing feature spec
│   ├── 002-volvo-module/                # Volvo-specific workflow
│   └── 003-reporting-module/            # Reporting feature spec
│
├── _bmad/                               # BMad methodology files (AI-assisted dev)
├── _bmad-output/                        # Generated planning artifacts
├── docs/                                # Generated project documentation
│
├── .github/                             # GitHub configuration
│   └── copilot-instructions.md          # AI coding assistant rules
│
└── bin/                                 # Build output (excluded from repo)
```

## Entry Points

### Application Entry

- **App.xaml.cs**: Application startup, DI container configuration
- **MainWindow.xaml**: Root window, hosts navigation frame

### Module Entry Points

- **Main_ReceivingLabelPage**: Entry to receiving workflow
- **Main_DunnageLabelPage**: Entry to dunnage workflow
- **Main_CarrierDeliveryLabelPage**: Entry to carrier label workflow

## Critical Directories

### Core Infrastructure

- **Module_Core/Services/**: Foundation services (auth, error handling, logging, navigation)
- **Module_Core/Data/**: Core DAOs including ERP integration (READ-ONLY)
- **Module_Core/Helpers/Database/**: Connection string management

### Business Logic

- **Module_Receiving/Services/**: Receiving workflow orchestration
- **Module_Dunnage/Services/**: Dunnage workflow and admin operations
- **Database/StoredProcedures/**: All MySQL operations (NO raw SQL in C#)

### User Interface

- **Module_Receiving/Views/**: 10 workflow views + 2 alternate modes
- **Module_Dunnage/Views/**: 13+ views including admin dashboard
- **Module_Shared/Views/**: 5 shared UI components (login, splash, help)

### Data Layer

- **Database/Schemas/**: MySQL table definitions (versioned)
- **Database/InforVisualScripts/**: SQL Server integration examples (READ-ONLY)
- **Module_*/Data/**: Instance-based DAO classes

## Modular Architecture Pattern

Each business module follows the same structure:

```
Module_{Name}/
├── Data/               # DAOs (database access)
├── Enums/              # Module-specific enumerations
├── Interfaces/         # Service contracts
├── Models/             # Domain models
├── Services/           # Business logic services
├── ViewModels/         # MVVM ViewModels
└── Views/              # XAML UI views
```

This strict separation ensures:

- **Testability**: Services can be mocked via interfaces
- **Maintainability**: Clear boundaries between layers
- **Scalability**: New modules follow established patterns
- **Reusability**: Shared components in Module_Shared

## Integration Points

### Database Access

- **MySQL**: Via stored procedures only (Helper_Database_StoredProcedure)
- **SQL Server**: Via Dao_InforVisualConnection with READ-ONLY intent
- **Connection Strings**: Managed in Helper_Database_Variables

### Navigation

- **IService_Navigation**: Centralized navigation service
- **Workflow Containers**: Each module has a workflow coordinator
- **View Registration**: ViewModels registered in App.xaml.cs DI

### State Management

- **Session Services**: IService_SessionManager, IService_UserSessionManager
- **Workflow State**: Model_ReceivingSession, Model_DunnageSession
- **User Preferences**: Stored in MySQL users table
