# MTM Receiving Application - Modularization Workflow Guide

**Date Created:** 2026-01-03  
**Purpose:** Complete guide for restructuring the application into self-contained workflow modules  
**Target Structure:** 3 independent modules (Receiving, Dunnage, RoutingLabels)

---

## 📋 Table of Contents

1. [Overview & Goals](#overview--goals)
2. [Current Architecture Analysis](#current-architecture-analysis)
3. [Target Module Structure](#target-module-structure)
4. [Step-by-Step Implementation Workflow](#step-by-step-implementation-workflow)
5. [User Stories](#user-stories)
6. [Implementation Prompts](#implementation-prompts)
7. [File Mapping & Migration Plan](#file-mapping--migration-plan)
8. [Testing Strategy](#testing-strategy)

---

## 🎯 Overview & Goals

### Current Problems
- **Tight Coupling:** Receiving and Dunnage workflows share services and are intermingled in the codebase
- **Scattered Files:** ViewModels, Views, Services, DAOs spread across multiple directories
- **Hard to Maintain:** Changes to one workflow risk breaking the other
- **No Routing Labels:** Third workflow (Carrier Delivery) is a placeholder

### Modularization Goals
✅ **Self-Contained Modules:** Each workflow operates independently with its own folder structure  
✅ **Clear Boundaries:** Shared services clearly identified and isolated  
✅ **Consistent Structure:** All three modules follow identical patterns  
✅ **Easy Navigation:** Developers find all related files in one place  
✅ **Simplified Testing:** Each module can be tested in isolation

---

## 🔍 Current Architecture Analysis

### Receiving Workflow Files (Identified via Serena)

**ViewModels:** (11 files in `ViewModels/Receiving/`)
```
Receiving_WorkflowViewModel.cs           # Main workflow orchestrator
Receiving_ModeSelectionViewModel.cs      # Guided/Manual/Edit mode selection
Receiving_POEntryViewModel.cs            # PO number entry & validation
Receiving_PackageTypeViewModel.cs        # Package type selection
Receiving_LoadEntryViewModel.cs          # Number of loads input
Receiving_WeightQuantityViewModel.cs     # Weight/quantity per load
Receiving_HeatLotViewModel.cs            # Heat/Lot number entry
Receiving_ReviewGridViewModel.cs         # Review & save screen
Receiving_ManualEntryViewModel.cs        # Manual bulk entry mode
Receiving_EditModeViewModel.cs           # Historical data editing
Main_ReceivingLabelViewModel.cs          # (in ViewModels/Main/)
```

**Views:** (11 files in `Views/Receiving/`)
```
Receiving_WorkflowView.xaml/.cs          # Main container
Receiving_ModeSelectionView.xaml/.cs
Receiving_POEntryView.xaml/.cs
Receiving_PackageTypeView.xaml/.cs
Receiving_LoadEntryView.xaml/.cs
Receiving_WeightQuantityView.xaml/.cs
Receiving_HeatLotView.xaml/.cs
Receiving_ReviewGridView.xaml/.cs
Receiving_ManualEntryView.xaml/.cs
Receiving_EditModeView.xaml/.cs
```

**Services:** (4 files in `Services/`)
```
Services/Receiving/Service_ReceivingWorkflow.cs      # Core workflow logic
Services/Receiving/Service_ReceivingValidation.cs    # Validation rules
Services/Receiving/Service_MySQL_ReceivingLine.cs    # Database operations
Services/Database/Service_MySQL_Receiving.cs         # DAO layer
```

**Data Access:** (5 files in `Data/Receiving/`)
```
Dao_ReceivingLoad.cs
Dao_ReceivingLine.cs
Dao_PackageTypePreference.cs
Dao_DunnageLine.cs
Dao_CarrierDeliveryLabel.cs
```

**Models:** (in `Models/Receiving/`)
```
Model_ReceivingLoad.cs
Model_ReceivingSession.cs
Model_SaveResult.cs
Model_PackageTypePreference.cs
```

**Enums:**
```
Enum_ReceivingWorkflowStep.cs (11 steps: ModeSelection → Complete)
```

**Database:** (in `Database/StoredProcedures/Receiving/`)
```
sp_SaveReceivingLoads.sql
sp_SavePackageTypePreference.sql
sp_GetReceivingLoads.sql
(+ others)
```

### Dunnage Workflow Files (Identified via Serena)

**ViewModels:** (14 files in `ViewModels/Dunnage/`)
```
Dunnage_ModeSelectionViewModel.cs        # Guided/Manual/Edit mode
Dunnage_TypeSelectionViewModel.cs        # Dunnage type selection
Dunnage_PartSelectionViewModel.cs        # Part/spec selection
Dunnage_DetailsEntryViewModel.cs         # PO, location, custom fields
Dunnage_QuantityEntryViewModel.cs        # Quantity input
Dunnage_ReviewViewModel.cs               # Review & save
Dunnage_ManualEntryViewModel.cs          # Bulk manual entry
Dunnage_EditModeViewModel.cs             # Historical editing
Dunnage_AdminMainViewModel.cs            # Admin menu
Dunnage_AdminTypesViewModel.cs           # Type management
Dunnage_AdminPartsViewModel.cs           # Part management
Dunnage_AdminInventoryViewModel.cs       # Inventoried parts
Dunnage_AddTypeDialogViewModel.cs        # Quick add dialog
Main_DunnageLabelViewModel.cs            # (in ViewModels/Main/)
```

**Views:** (14+ files in `Views/Dunnage/`)
```
Dunnage_WorkflowView.xaml/.cs            # Main container
Dunnage_ModeSelectionView.xaml/.cs
Dunnage_TypeSelectionView.xaml/.cs
Dunnage_PartSelectionView.xaml/.cs
Dunnage_DetailsEntryView.xaml/.cs
Dunnage_QuantityEntryView.xaml/.cs
Dunnage_ReviewView.xaml/.cs
Dunnage_ManualEntryView.xaml/.cs
Dunnage_EditModeView.xaml/.cs
(+ 6 Admin views)
```

**Services:** (3 files)
```
Services/Receiving/Service_DunnageWorkflow.cs        # Core workflow
Services/Receiving/Service_DunnageAdminWorkflow.cs   # Admin operations
Services/Receiving/Service_DunnageCSVWriter.cs       # CSV export
Services/Database/Service_MySQL_Dunnage.cs           # Database layer
```

**Data Access:** (7 files in `Data/Dunnage/`)
```
Dao_DunnageLoad.cs
Dao_DunnageType.cs
Dao_DunnagePart.cs
Dao_DunnageSpec.cs
Dao_DunnageCustomField.cs
Dao_DunnageUserPreference.cs
Dao_InventoriedDunnage.cs
```

**Models:** (in `Models/Dunnage/`)
```
Model_DunnageLoad.cs
Model_DunnageSession.cs
Model_DunnageType.cs
Model_DunnagePart.cs
Model_DunnageSpec.cs
Model_SpecInput.cs
```

**Enums:**
```
Enum_DunnageWorkflowStep.cs (8 steps: ModeSelection → EditMode)
```

### Shared Infrastructure (DO NOT MOVE)

**Core Services:** (Used by all modules)
```
Services/Service_ErrorHandler.cs
Services/Service_LoggingUtility.cs
Services/Service_Window.cs
Services/Service_Dispatcher.cs
Services/Service_Pagination.cs
Services/Help/Service_Help.cs
Services/Database/Service_SessionManager.cs
Services/Authentication/*
Services/Startup/*
```

**Helpers:**
```
Helpers/Database/*
Helpers/UI/*
```

**Converters:** (All in `Converters/`)

**Shared Models:**
```
Models/Core/Model_Dao_Result.cs
Models/Core/Model_Dao_Result_Factory.cs
Models/Systems/*
Models/Enums/Enum_ErrorSeverity.cs
```

---

## 🏗️ Target Module Structure

Each module will have identical structure in its own root folder:

```
/ReceivingModule/
├── ViewModels/
│   ├── ReceivingWorkflowViewModel.cs          # Main orchestrator
│   ├── ModeSelectionViewModel.cs              # Mode selection
│   ├── POEntryViewModel.cs                    # Step 1
│   ├── PackageTypeViewModel.cs                # Step 2
│   ├── LoadEntryViewModel.cs                  # Step 3
│   ├── WeightQuantityViewModel.cs             # Step 4
│   ├── HeatLotViewModel.cs                    # Step 5
│   ├── ReviewGridViewModel.cs                 # Review
│   ├── ManualEntryViewModel.cs                # Manual mode
│   └── EditModeViewModel.cs                   # Edit mode
├── Views/
│   ├── ReceivingWorkflowView.xaml/.cs         # Container
│   ├── ModeSelectionView.xaml/.cs
│   ├── POEntryView.xaml/.cs
│   ├── PackageTypeView.xaml/.cs
│   ├── LoadEntryView.xaml/.cs
│   ├── WeightQuantityView.xaml/.cs
│   ├── HeatLotView.xaml/.cs
│   ├── ReviewGridView.xaml/.cs
│   ├── ManualEntryView.xaml/.cs
│   └── EditModeView.xaml/.cs
├── Services/
│   ├── ReceivingWorkflowService.cs            # Workflow orchestration
│   ├── ReceivingValidationService.cs          # Business rules
│   ├── ReceivingCSVWriterService.cs           # CSV export
│   └── ReceivingDatabaseService.cs            # Database facade
├── Data/
│   ├── Dao_ReceivingLoad.cs
│   ├── Dao_ReceivingLine.cs
│   └── Dao_PackageTypePreference.cs
├── Models/
│   ├── ReceivingLoad.cs                       # Domain model
│   ├── ReceivingSession.cs                    # Session state
│   ├── SaveResult.cs                          # Operation result
│   └── PackageTypePreference.cs
├── Enums/
│   └── ReceivingWorkflowStep.cs
├── Contracts/
│   ├── IReceivingWorkflowService.cs
│   ├── IReceivingValidationService.cs
│   └── IReceivingDatabaseService.cs
├── Database/
│   ├── StoredProcedures/
│   │   ├── sp_SaveReceivingLoads.sql
│   │   ├── sp_GetReceivingLoads.sql
│   │   └── sp_SavePackageTypePreference.sql
│   └── Schemas/
│       └── receiving_loads.sql
└── README.md                                  # Module documentation

/DunnageModule/
├── ViewModels/
│   ├── DunnageWorkflowViewModel.cs
│   ├── ModeSelectionViewModel.cs
│   ├── TypeSelectionViewModel.cs
│   ├── PartSelectionViewModel.cs
│   ├── DetailsEntryViewModel.cs
│   ├── QuantityEntryViewModel.cs
│   ├── ReviewViewModel.cs
│   ├── ManualEntryViewModel.cs
│   ├── EditModeViewModel.cs
│   ├── AdminMainViewModel.cs
│   ├── AdminTypesViewModel.cs
│   ├── AdminPartsViewModel.cs
│   └── AdminInventoryViewModel.cs
├── Views/
│   ├── DunnageWorkflowView.xaml/.cs
│   ├── ModeSelectionView.xaml/.cs
│   ├── TypeSelectionView.xaml/.cs
│   ├── PartSelectionView.xaml/.cs
│   ├── DetailsEntryView.xaml/.cs
│   ├── QuantityEntryView.xaml/.cs
│   ├── ReviewView.xaml/.cs
│   ├── ManualEntryView.xaml/.cs
│   ├── EditModeView.xaml/.cs
│   └── Admin/
│       ├── AdminMainView.xaml/.cs
│       ├── AdminTypesView.xaml/.cs
│       ├── AdminPartsView.xaml/.cs
│       └── AdminInventoryView.xaml/.cs
├── Services/
│   ├── DunnageWorkflowService.cs
│   ├── DunnageAdminWorkflowService.cs
│   ├── DunnageCSVWriterService.cs
│   └── DunnageDatabaseService.cs
├── Data/
│   ├── Dao_DunnageLoad.cs
│   ├── Dao_DunnageType.cs
│   ├── Dao_DunnagePart.cs
│   ├── Dao_DunnageSpec.cs
│   └── Dao_InventoriedDunnage.cs
├── Models/
│   ├── DunnageLoad.cs
│   ├── DunnageSession.cs
│   ├── DunnageType.cs
│   ├── DunnagePart.cs
│   └── DunnageSpec.cs
├── Enums/
│   └── DunnageWorkflowStep.cs
├── Contracts/
│   ├── IDunnageWorkflowService.cs
│   ├── IDunnageAdminWorkflowService.cs
│   └── IDunnageDatabaseService.cs
├── Database/
│   ├── StoredProcedures/
│   └── Schemas/
└── README.md

/RoutingLabelsModule/
├── ViewModels/
│   ├── RoutingWorkflowViewModel.cs
│   ├── ModeSelectionViewModel.cs
│   ├── CarrierSelectionViewModel.cs
│   ├── DestinationEntryViewModel.cs
│   ├── ShipmentDetailsViewModel.cs
│   ├── ReviewViewModel.cs
│   └── ManualEntryViewModel.cs
├── Views/
│   ├── RoutingWorkflowView.xaml/.cs
│   ├── ModeSelectionView.xaml/.cs
│   ├── CarrierSelectionView.xaml/.cs
│   ├── DestinationEntryView.xaml/.cs
│   ├── ShipmentDetailsView.xaml/.cs
│   ├── ReviewView.xaml/.cs
│   └── ManualEntryView.xaml/.cs
├── Services/
│   ├── RoutingWorkflowService.cs
│   └── RoutingDatabaseService.cs
├── Data/
│   ├── Dao_RoutingLabel.cs
│   └── Dao_CarrierDelivery.cs
├── Models/
│   ├── RoutingLabel.cs
│   ├── RoutingSession.cs
│   └── CarrierDelivery.cs
├── Enums/
│   └── RoutingWorkflowStep.cs
├── Contracts/
│   ├── IRoutingWorkflowService.cs
│   └── IRoutingDatabaseService.cs
├── Database/
│   ├── StoredProcedures/
│   └── Schemas/
└── README.md
```

---

## 📝 Step-by-Step Implementation Workflow

### Phase 1: Preparation & Branch Setup

**Step 1.1: Create Feature Branch**
```bash
git checkout master
git pull origin master
git checkout -b feature/modularize-workflows
```

**Step 1.2: Install .specify Tools** (if not already installed)
```powershell
# Verify .specify configuration
Get-Content .specify/config.json
```

**Step 1.3: Create User Stories** (See detailed user stories below)
- Create `ReceivingModule/USER_STORIES.md`
- Create `DunnageModule/USER_STORIES.md`
- Create `RoutingLabelsModule/USER_STORIES.md`

### Phase 2: Receiving Module Migration

**Step 2.1: Create Module Structure**
```bash
# Create directory structure
mkdir -p ReceivingModule/{ViewModels,Views,Services,Data,Models,Enums,Contracts,Database/{StoredProcedures,Schemas}}
```

**Step 2.2: Use .specify to Generate Plan**
```powershell
.\.specify\scripts\powershell\create-new-feature.ps1 -FeatureName "ReceivingModule"
```

**Step 2.3: Move Files Systematically**
Use Serena to identify and move files:
1. Move ViewModels (remove `Receiving_` prefix)
2. Move Views (remove `Receiving_` prefix)
3. Move Services
4. Move DAOs
5. Move Models
6. Move Enums
7. Move Database scripts

**Step 2.4: Update Namespaces**
- From: `MTM_Receiving_Application.ViewModels.Receiving`
- To: `MTM_Receiving_Application.ReceivingModule.ViewModels`

**Step 2.5: Update DI Registration in App.xaml.cs**
```csharp
// Receiving Module Services
services.AddSingleton<IReceivingWorkflowService, ReceivingWorkflowService>();
services.AddSingleton<IReceivingValidationService, ReceivingValidationService>();

// Receiving Module ViewModels
services.AddTransient<ReceivingWorkflowViewModel>();
services.AddTransient<POEntryViewModel>();
// etc.
```

### Phase 3: Dunnage Module Migration

(Same process as Receiving, using Dunnage files)

### Phase 4: Routing Labels Module Creation

**Step 4.1: Use ReceivingModule as Template**
Copy structure and adapt:
- Carrier selection instead of PO entry
- Destination entry instead of package type
- Shipment details instead of weight/quantity

**Step 4.2: Create Database Schema**
```sql
CREATE TABLE routing_labels (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    CarrierId INT NOT NULL,
    DestinationAddress VARCHAR(500),
    ShipmentDate DATE,
    TrackingNumber VARCHAR(100),
    -- etc.
);
```

### Phase 5: Integration & Testing

**Step 5.1: Update MainWindow Navigation**
```xaml
<NavigationViewItem Content="Receiving Labels" Tag="ReceivingModule.ReceivingWorkflowView">
<NavigationViewItem Content="Dunnage Labels" Tag="DunnageModule.DunnageWorkflowView">
<NavigationViewItem Content="Routing Labels" Tag="RoutingLabelsModule.RoutingWorkflowView">
```

**Step 5.2: Run Integration Tests**
```bash
dotnet test --filter "FullyQualifiedName~ReceivingModule"
dotnet test --filter "FullyQualifiedName~DunnageModule"
dotnet test --filter "FullyQualifiedName~RoutingLabelsModule"
```

**Step 5.3: Update Documentation**
- Update README.md with new structure
- Create module-specific README files
- Update AGENTS.md with module guidelines

---

## 📖 User Stories

### Receiving Module User Stories

**File:** `ReceivingModule/USER_STORIES.md`

```markdown
# Receiving Module - User Stories

## Epic: Material Receiving Label Generation
As a receiving clerk, I need to generate accurate receiving labels for incoming materials so that inventory can be properly tracked in the warehouse.

---

### US-R001: Mode Selection
**As a** receiving clerk  
**I want to** choose between Guided, Manual, or Edit mode  
**So that** I can use the workflow that best fits my current task

**Acceptance Criteria:**
- [ ] Display three mode options: Guided Workflow, Manual Entry, Edit Mode
- [ ] Show confirmation dialog if switching modes with unsaved data
- [ ] Guided mode starts at PO Entry step
- [ ] Manual mode opens bulk grid entry
- [ ] Edit mode loads historical data
- [ ] Default mode preference saved per user

**Technical Notes:**
- ViewModel: `ModeSelectionViewModel.cs`
- Enum value: `ReceivingWorkflowStep.ModeSelection`
- Session property: Not applicable (pre-workflow)

---

### US-R002: PO Number Entry
**As a** receiving clerk  
**I want to** enter and validate purchase order numbers  
**So that** I can pull correct part information from the ERP system

**Acceptance Criteria:**
- [ ] Accept PO numbers in format: PO-NNNNNN (6 digits)
- [ ] Auto-format input (e.g., "63150" → "PO-063150")
- [ ] Validate PO exists in Infor Visual ERP
- [ ] Display PO status (Open, Closed, etc.)
- [ ] Load available parts from PO
- [ ] Allow Non-PO item toggle
- [ ] Show validation errors inline

**Technical Notes:**
- ViewModel: `POEntryViewModel.cs`
- Service: `ReceivingWorkflowService.cs`
- Infor Visual: Read-only SELECT from `po_detail` table
- Session property: `CurrentPONumber`, `IsNonPOItem`

---

### US-R003: Package Type Selection
**As a** receiving clerk  
**I want to** select or customize package types  
**So that** labels accurately reflect how materials are packaged

**Acceptance Criteria:**
- [ ] Display preset types: Coils, Sheets, Skids, Custom
- [ ] Allow custom package type name entry
- [ ] Save package type preference per part
- [ ] Retrieve saved preferences automatically
- [ ] Validate custom names (max 50 characters)

**Technical Notes:**
- ViewModel: `PackageTypeViewModel.cs`
- DAO: `Dao_PackageTypePreference.cs`
- Stored Procedure: `sp_SavePackageTypePreference`
- Session property: `SelectedPackageType`

---

### US-R004: Load Quantity Entry
**As a** receiving clerk  
**I want to** specify how many loads I'm receiving  
**So that** the correct number of labels are generated

**Acceptance Criteria:**
- [ ] Accept integer input (1-999)
- [ ] Default to 1 load
- [ ] Validate positive numbers only
- [ ] Generate load placeholders in session

**Technical Notes:**
- ViewModel: `LoadEntryViewModel.cs`
- Service: `ReceivingWorkflowService.GenerateLoads()`
- Session property: `NumberOfLoads`

---

### US-R005: Weight/Quantity Entry
**As a** receiving clerk  
**I want to** enter weight or quantity for each load  
**So that** inventory counts are accurate

**Acceptance Criteria:**
- [ ] Display grid of all loads
- [ ] Accept decimal input for weight/quantity
- [ ] Validate positive numbers
- [ ] Allow tab navigation between fields
- [ ] Show load number for each row

**Technical Notes:**
- ViewModel: `WeightQuantityViewModel.cs`
- Model: `ReceivingLoad.WeightQuantity` property
- Session: `CurrentSession.Loads` collection

---

### US-R006: Heat/Lot Number Entry
**As a** receiving clerk  
**I want to** enter heat or lot numbers for materials  
**So that** materials can be traced back to their manufacturing batch

**Acceptance Criteria:**
- [ ] Display grid of all loads
- [ ] Accept alphanumeric input (max 50 chars)
- [ ] Provide checkbox list of previously used numbers
- [ ] Auto-fill selected heat numbers to all loads
- [ ] Default to "Nothing Entered" if blank

**Technical Notes:**
- ViewModel: `HeatLotViewModel.cs`
- Model: `ReceivingLoad.HeatLotNumber` property
- Method: `PrepareHeatLotFields()` sets defaults

---

### US-R007: Review & Save
**As a** receiving clerk  
**I want to** review all entered data before saving  
**So that** I can catch errors before labels are printed

**Acceptance Criteria:**
- [ ] Display all loads with entered data
- [ ] Show PO number, Part ID, Package Type, Weight, Heat/Lot
- [ ] Provide "Add Another Part/PO" button
- [ ] Provide "Save to Database" button
- [ ] Validate all required fields filled
- [ ] Save to MySQL database via stored procedure
- [ ] Export to CSV (local + network)
- [ ] Clear session after successful save

**Technical Notes:**
- ViewModel: `ReviewGridViewModel.cs`
- Service: `ReceivingWorkflowService.SaveSessionAsync()`
- DAO: `Dao_ReceivingLoad.SaveReceivingLoadsAsync()`
- CSV: `Service_CSVWriter.WriteToCSVAsync()`

---

### US-R008: Add Another Part
**As a** receiving clerk  
**I want to** quickly add another part without re-entering the PO  
**So that** I can efficiently receive multiple parts from one PO

**Acceptance Criteria:**
- [ ] Show confirmation dialog before clearing
- [ ] Preserve reviewed loads in session
- [ ] Clear form inputs (PO, Package Type, etc.)
- [ ] Return to PO Entry step
- [ ] Maintain session until final save

**Technical Notes:**
- ViewModel: `ReviewGridViewModel.AddAnotherPartAsync()`
- Service: `ReceivingWorkflowService.AddCurrentPartToSessionAsync()`
- Clears: `CurrentPONumber`, `CurrentPart`, form ViewModels

---

### US-R009: Manual Bulk Entry
**As a** receiving clerk  
**I want to** use a grid to enter multiple loads at once  
**So that** I can work faster for large shipments

**Acceptance Criteria:**
- [ ] Display editable data grid
- [ ] Columns: PO, Part, Package Type, Weight, Heat/Lot
- [ ] Allow inline editing
- [ ] Validate on row change
- [ ] Provide "Add Row" button
- [ ] Save all rows to database

**Technical Notes:**
- ViewModel: `ManualEntryViewModel.cs`
- Grid binding: `ObservableCollection<ReceivingLoad>`

---

### US-R010: Edit Historical Data
**As a** supervisor  
**I want to** edit previously saved receiving records  
**So that** I can correct errors after labels are printed

**Acceptance Criteria:**
- [ ] Load historical data from database
- [ ] Filter by date range
- [ ] Display in editable grid
- [ ] Update database on save
- [ ] Log audit trail of changes

**Technical Notes:**
- ViewModel: `EditModeViewModel.cs`
- DAO: `Dao_ReceivingLoad.GetReceivingLoads()`
- Audit: Track `LastModified` timestamp
```

---

### Dunnage Module User Stories

**File:** `DunnageModule/USER_STORIES.md`

```markdown
# Dunnage Module - User Stories

## Epic: Dunnage Material Tracking
As a warehouse associate, I need to track reusable dunnage materials (pallets, dividers, etc.) so that we can manage inventory and reuse costs.

---

### US-D001: Mode Selection
**As a** warehouse associate  
**I want to** choose between Guided, Manual, or Edit mode  
**So that** I can select the best workflow for my task

**Acceptance Criteria:**
- [ ] Same as Receiving (US-R001)
- [ ] Guided starts at Type Selection
- [ ] Manual opens bulk grid
- [ ] Edit loads historical dunnage data

**Technical Notes:**
- ViewModel: `ModeSelectionViewModel.cs`
- Enum: `DunnageWorkflowStep.ModeSelection`

---

### US-D002: Dunnage Type Selection
**As a** warehouse associate  
**I want to** select the type of dunnage being received  
**So that** tracking is categorized correctly

**Acceptance Criteria:**
- [ ] Display list of dunnage types (Pallet, Divider, Skid, etc.)
- [ ] Show type icons (Material.Icons.WinUI3)
- [ ] Support pagination (10 items per page)
- [ ] Allow search/filter by type name
- [ ] Provide "Add New Type" button (admin)

**Technical Notes:**
- ViewModel: `TypeSelectionViewModel.cs`
- DAO: `Dao_DunnageType.GetAllTypesAsync()`
- Session: `CurrentSession.SelectedTypeId`

---

### US-D003: Part Selection
**As a** warehouse associate  
**I want to** select the specific part or spec within a type  
**So that** the exact dunnage item is identified

**Acceptance Criteria:**
- [ ] Display parts for selected type
- [ ] Show part ID, description, specs
- [ ] Allow quick add new part
- [ ] Filter by part ID or description

**Technical Notes:**
- ViewModel: `PartSelectionViewModel.cs`
- DAO: `Dao_DunnagePart.GetPartsByTypeAsync()`
- Session: `CurrentSession.SelectedPart`

---

### US-D004: Details Entry
**As a** warehouse associate  
**I want to** enter additional details like PO number and location  
**So that** dunnage can be traced and located

**Acceptance Criteria:**
- [ ] Optional PO number entry
- [ ] Location field (max 100 chars)
- [ ] Dynamic custom fields based on type
- [ ] Support text, number, and boolean spec inputs

**Technical Notes:**
- ViewModel: `DetailsEntryViewModel.cs`
- Model: `DunnageSession.PONumber`, `Location`
- Specs: `ObservableCollection<SpecInput>`

---

### US-D005: Quantity Entry
**As a** warehouse associate  
**I want to** specify the quantity received  
**So that** inventory counts are accurate

**Acceptance Criteria:**
- [ ] Accept integer input (default 1)
- [ ] Display selected type and part info
- [ ] Validate positive numbers

**Technical Notes:**
- ViewModel: `QuantityEntryViewModel.cs`
- Session: `CurrentSession.Quantity`

---

### US-D006: Review & Save
**As a** warehouse associate  
**I want to** review entered data before saving  
**So that** dunnage records are correct

**Acceptance Criteria:**
- [ ] Display summary of type, part, quantity, specs
- [ ] Provide "Add Another" button
- [ ] Provide "Save to Database" button
- [ ] Save to MySQL via stored procedure
- [ ] Export to CSV

**Technical Notes:**
- ViewModel: `ReviewViewModel.cs`
- Service: `DunnageWorkflowService.SaveSessionAsync()`
- DAO: `Dao_DunnageLoad.SaveLoadsAsync()`

---

### US-D007: Admin Type Management
**As a** dunnage administrator  
**I want to** create, edit, and delete dunnage types  
**So that** the system reflects our current dunnage inventory

**Acceptance Criteria:**
- [ ] CRUD operations for types
- [ ] Assign Material Design icons
- [ ] Set active/inactive status
- [ ] Prevent deletion if parts exist

**Technical Notes:**
- ViewModel: `AdminTypesViewModel.cs`
- DAO: `Dao_DunnageType.cs`

---

### US-D008: Admin Part Management
**As a** dunnage administrator  
**I want to** manage parts within each type  
**So that** warehouse associates have accurate options

**Acceptance Criteria:**
- [ ] CRUD for parts
- [ ] Associate parts with types
- [ ] Define specs per part
- [ ] Search and filter

**Technical Notes:**
- ViewModel: `AdminPartsViewModel.cs`
- DAO: `Dao_DunnagePart.cs`, `Dao_DunnageSpec.cs`

---

### US-D009: Inventoried Parts List
**As a** dunnage administrator  
**I want to** mark parts as "inventoried"  
**So that** they appear in standard selection lists

**Acceptance Criteria:**
- [ ] Toggle inventoried status
- [ ] Filter by inventoried/non-inventoried
- [ ] Only show inventoried in standard workflow

**Technical Notes:**
- ViewModel: `AdminInventoryViewModel.cs`
- DAO: `Dao_InventoriedDunnage.cs`
```

---

### Routing Labels Module User Stories

**File:** `RoutingLabelsModule/USER_STORIES.md`

```markdown
# Routing Labels Module - User Stories

## Epic: Carrier Delivery Label Generation
As a shipping clerk, I need to generate routing labels for outbound shipments so that carriers can deliver materials to the correct destination.

---

### US-RL001: Mode Selection
**As a** shipping clerk  
**I want to** choose between Guided or Manual entry mode  
**So that** I can match my workflow to the shipment type

**Acceptance Criteria:**
- [ ] Display Guided and Manual options
- [ ] Guided starts at Carrier Selection
- [ ] Manual opens bulk grid entry

**Technical Notes:**
- ViewModel: `ModeSelectionViewModel.cs`
- Enum: `RoutingWorkflowStep.ModeSelection`

---

### US-RL002: Carrier Selection
**As a** shipping clerk  
**I want to** select the delivery carrier  
**So that** routing information is carrier-specific

**Acceptance Criteria:**
- [ ] Display list of carriers (UPS, FedEx, USPS, LTL, Local Delivery)
- [ ] Support custom carrier entry
- [ ] Store carrier preferences

**Technical Notes:**
- ViewModel: `CarrierSelectionViewModel.cs`
- Session: `CurrentSession.SelectedCarrier`

---

### US-RL003: Destination Entry
**As a** shipping clerk  
**I want to** enter destination address information  
**So that** carriers know where to deliver

**Acceptance Criteria:**
- [ ] Fields: Address Line 1, Line 2, City, State, ZIP
- [ ] Validate ZIP code format
- [ ] Support address book lookup
- [ ] Save frequently used addresses

**Technical Notes:**
- ViewModel: `DestinationEntryViewModel.cs`
- Model: `RoutingLabel.DestinationAddress`

---

### US-RL004: Shipment Details
**As a** shipping clerk  
**I want to** enter shipment details like weight and tracking number  
**So that** the label contains all required carrier information

**Acceptance Criteria:**
- [ ] Fields: Weight, Dimensions, Tracking Number, Special Instructions
- [ ] Validate tracking number format per carrier
- [ ] Calculate shipping cost (if integrated)

**Technical Notes:**
- ViewModel: `ShipmentDetailsViewModel.cs`
- Model: `RoutingLabel.Weight`, `TrackingNumber`

---

### US-RL005: Review & Print
**As a** shipping clerk  
**I want to** review and print routing labels  
**So that** shipments are correctly labeled

**Acceptance Criteria:**
- [ ] Display label preview
- [ ] Include: Carrier, Destination, Tracking, Barcode
- [ ] Print to network printer
- [ ] Save to database
- [ ] Export to CSV

**Technical Notes:**
- ViewModel: `ReviewViewModel.cs`
- Service: `RoutingWorkflowService.SaveSessionAsync()`
- Printing: Use existing print infrastructure

---

### US-RL006: Manual Bulk Entry
**As a** shipping clerk  
**I want to** enter multiple routing labels in a grid  
**So that** I can process batch shipments quickly

**Acceptance Criteria:**
- [ ] Grid columns: Carrier, Destination, Tracking, Weight
- [ ] Inline editing
- [ ] Batch print
- [ ] Batch save

**Technical Notes:**
- ViewModel: `ManualEntryViewModel.cs`
- Grid: `ObservableCollection<RoutingLabel>`
```

---

## 🚀 Implementation Prompts

### Prompt 1: Receiving Module Implementation

```
You are implementing the Receiving Module as a self-contained, modular component of the MTM Receiving Application.

CONTEXT:
- Framework: WinUI 3 on .NET 8
- Architecture: Strict MVVM with CommunityToolkit.Mvvm
- Database: MySQL (stored procedures only)
- Pattern: All modules follow identical structure

REQUIREMENTS:
1. Create folder structure in /ReceivingModule/
2. Migrate existing Receiving workflow files from:
   - ViewModels/Receiving/* → ReceivingModule/ViewModels/*
   - Views/Receiving/* → ReceivingModule/Views/*
   - Services/Receiving/Service_Receiving* → ReceivingModule/Services/*
   - Data/Receiving/Dao_Receiving* → ReceivingModule/Data/*
   - Models/Receiving/* → ReceivingModule/Models/*

3. Update ALL namespaces:
   - FROM: MTM_Receiving_Application.ViewModels.Receiving
   - TO: MTM_Receiving_Application.ReceivingModule.ViewModels

4. Remove "Receiving_" prefix from all file names:
   - Receiving_POEntryViewModel.cs → POEntryViewModel.cs
   - Receiving_POEntryView.xaml → POEntryView.xaml

5. Update DI registration in App.xaml.cs for new namespaces

6. Create ReceivingModule/README.md documenting:
   - Module purpose and scope
   - File structure
   - Dependencies on core services
   - How to extend the module

7. Implement all 10 user stories from USER_STORIES.md

CONSTRAINTS:
- DO NOT modify shared infrastructure (error handling, logging, window services)
- DO NOT write to Infor Visual database (read-only SELECT only)
- ALL database operations use stored procedures
- ViewModels MUST be partial classes
- Views MUST use x:Bind
- Follow existing MVVM patterns

DELIVERABLES:
- Complete /ReceivingModule/ folder with all files
- Updated App.xaml.cs with new registrations
- Unit tests for all ViewModels
- Integration tests for workflow service
- README.md documentation

Use Serena tools to:
1. Find and list all files to migrate
2. Verify namespace consistency
3. Check for remaining references to old paths
```

---

### Prompt 2: Dunnage Module Implementation

```
You are implementing the Dunnage Module as a self-contained, modular component of the MTM Receiving Application.

CONTEXT:
Same as Receiving Module (WinUI 3, MVVM, MySQL)

REQUIREMENTS:
1. Create folder structure in /DunnageModule/ (identical to ReceivingModule)
2. Migrate existing Dunnage workflow files from:
   - ViewModels/Dunnage/* → DunnageModule/ViewModels/*
   - Views/Dunnage/* → DunnageModule/Views/*
   - Services/Receiving/Service_Dunnage* → DunnageModule/Services/*
   - Data/Dunnage/* → DunnageModule/Data/*
   - Models/Dunnage/* → DunnageModule/Models/*

3. Update ALL namespaces:
   - FROM: MTM_Receiving_Application.ViewModels.Dunnage
   - TO: MTM_Receiving_Application.DunnageModule.ViewModels

4. Remove "Dunnage_" prefix from file names:
   - Dunnage_TypeSelectionViewModel.cs → TypeSelectionViewModel.cs
   - Dunnage_TypeSelectionView.xaml → TypeSelectionView.xaml

5. Update DI registration in App.xaml.cs

6. Create DunnageModule/README.md

7. Implement all 9 user stories from USER_STORIES.md

ADDITIONAL FEATURES:
- Admin workflow for type/part management
- Inventoried parts filtering
- Material.Icons.WinUI3 for type icons

DELIVERABLES:
- Complete /DunnageModule/ folder
- Updated App.xaml.cs
- Unit + Integration tests
- README.md documentation

Use Serena tools to ensure clean migration.
```

---

### Prompt 3: Routing Labels Module Implementation

```
You are implementing the Routing Labels Module as a NEW self-contained component for the MTM Receiving Application. This module does not currently exist and must be created from scratch.

CONTEXT:
- Framework: WinUI 3, .NET 8, MVVM, MySQL
- Replaces: "Carrier Delivery" placeholder in MainWindow.xaml
- Pattern: Mirror ReceivingModule structure exactly

REQUIREMENTS:
1. Create NEW /RoutingLabelsModule/ folder structure:
   /RoutingLabelsModule/
   ├── ViewModels/
   ├── Views/
   ├── Services/
   ├── Data/
   ├── Models/
   ├── Enums/
   ├── Contracts/
   ├── Database/
   └── README.md

2. Create NEW workflow steps (enum):
   - ModeSelection
   - CarrierSelection
   - DestinationEntry
   - ShipmentDetails
   - Review
   - ManualEntry

3. Create ViewModels for each step:
   - RoutingWorkflowViewModel (main orchestrator)
   - ModeSelectionViewModel
   - CarrierSelectionViewModel
   - DestinationEntryViewModel
   - ShipmentDetailsViewModel
   - ReviewViewModel
   - ManualEntryViewModel

4. Create corresponding XAML Views for each ViewModel

5. Create Services:
   - RoutingWorkflowService (workflow orchestration)
   - RoutingDatabaseService (database operations)

6. Create Models:
   - RoutingLabel (main domain model)
   - RoutingSession (workflow state)
   - CarrierDelivery (carrier info)

7. Create Database schema and stored procedures:
   - routing_labels table
   - sp_SaveRoutingLabels
   - sp_GetRoutingLabels

8. Update MainWindow.xaml navigation:
   - Change "Carrier Delivery" Tag to "RoutingLabelsModule.RoutingWorkflowView"

9. Implement all 6 user stories from USER_STORIES.md

DESIGN DECISIONS:
- Carriers: UPS, FedEx, USPS, LTL Freight, Local Delivery, Custom
- Label includes: Barcode, Tracking Number, Destination, Weight
- Print to network printer using existing infrastructure
- Export to CSV format

DELIVERABLES:
- Complete /RoutingLabelsModule/ folder (all files NEW)
- Database schema SQL scripts
- Unit + Integration tests
- README.md documentation
- Updated MainWindow.xaml

NOTE: This is a greenfield implementation. Use ReceivingModule as a reference template but adapt for routing-specific logic.
```

---

## 📊 File Mapping & Migration Plan

### Files to MOVE (Receiving)

| Current Path | New Path | Action |
|--------------|----------|--------|
| `ViewModels/Receiving/Receiving_WorkflowViewModel.cs` | `ReceivingModule/ViewModels/ReceivingWorkflowViewModel.cs` | Move + Rename |
| `ViewModels/Receiving/Receiving_POEntryViewModel.cs` | `ReceivingModule/ViewModels/POEntryViewModel.cs` | Move + Rename |
| `ViewModels/Receiving/Receiving_PackageTypeViewModel.cs` | `ReceivingModule/ViewModels/PackageTypeViewModel.cs` | Move + Rename |
| `ViewModels/Receiving/Receiving_LoadEntryViewModel.cs` | `ReceivingModule/ViewModels/LoadEntryViewModel.cs` | Move + Rename |
| `ViewModels/Receiving/Receiving_WeightQuantityViewModel.cs` | `ReceivingModule/ViewModels/WeightQuantityViewModel.cs` | Move + Rename |
| `ViewModels/Receiving/Receiving_HeatLotViewModel.cs` | `ReceivingModule/ViewModels/HeatLotViewModel.cs` | Move + Rename |
| `ViewModels/Receiving/Receiving_ReviewGridViewModel.cs` | `ReceivingModule/ViewModels/ReviewGridViewModel.cs` | Move + Rename |
| `ViewModels/Receiving/Receiving_ManualEntryViewModel.cs` | `ReceivingModule/ViewModels/ManualEntryViewModel.cs` | Move + Rename |
| `ViewModels/Receiving/Receiving_EditModeViewModel.cs` | `ReceivingModule/ViewModels/EditModeViewModel.cs` | Move + Rename |
| `ViewModels/Receiving/Receiving_ModeSelectionViewModel.cs` | `ReceivingModule/ViewModels/ModeSelectionViewModel.cs` | Move + Rename |
| (Similar for all Views, Services, DAOs, Models) | | |

### Files to MOVE (Dunnage)

| Current Path | New Path | Action |
|--------------|----------|--------|
| `ViewModels/Dunnage/Dunnage_ModeSelectionViewModel.cs` | `DunnageModule/ViewModels/ModeSelectionViewModel.cs` | Move + Rename |
| `ViewModels/Dunnage/Dunnage_TypeSelectionViewModel.cs` | `DunnageModule/ViewModels/TypeSelectionViewModel.cs` | Move + Rename |
| (14 ViewModels total) | | |
| (14 Views total) | | |
| (Services, DAOs, Models) | | |

### Files to DELETE (after successful migration)

```
ViewModels/Receiving/          # Empty folder - DELETE
ViewModels/Dunnage/            # Empty folder - DELETE
Views/Receiving/               # Empty folder - DELETE
Views/Dunnage/                 # Empty folder - DELETE
ViewModels/Main/Main_ReceivingLabelViewModel.cs  # Moved to ReceivingModule
ViewModels/Main/Main_DunnageLabelViewModel.cs    # Moved to DunnageModule
```

### Files to KEEP (Shared Infrastructure)

```
Services/Service_ErrorHandler.cs
Services/Service_LoggingUtility.cs
Services/Service_Window.cs
Services/Service_Dispatcher.cs
Services/Help/Service_Help.cs
Services/Database/Service_SessionManager.cs
Services/Authentication/*
Helpers/Database/*
Helpers/UI/*
Converters/*
Models/Core/*
Models/Systems/*
MainWindow.xaml
App.xaml.cs
```

---

## 🧪 Testing Strategy

### Unit Tests Structure

```
/ReceivingModule.Tests/
├── ViewModels/
│   ├── POEntryViewModelTests.cs
│   ├── PackageTypeViewModelTests.cs
│   ├── ReviewGridViewModelTests.cs
│   └── ...
├── Services/
│   ├── ReceivingWorkflowServiceTests.cs
│   └── ReceivingValidationServiceTests.cs
└── Data/
    └── Dao_ReceivingLoadTests.cs

/DunnageModule.Tests/
└── (Same structure as Receiving)

/RoutingLabelsModule.Tests/
└── (Same structure as Receiving)
```

### Integration Tests

```csharp
// Example: Receiving Module Integration Test
[Fact]
public async Task ReceivingWorkflow_CompleteFlow_SavesSuccessfully()
{
    // Arrange
    var workflow = GetService<IReceivingWorkflowService>();
    await workflow.StartWorkflowAsync();
    
    // Act: Simulate complete workflow
    workflow.CurrentPONumber = "PO-063150";
    await workflow.AdvanceToNextStepAsync(); // → PackageType
    
    // ... continue through all steps
    
    var result = await workflow.SaveSessionAsync();
    
    // Assert
    Assert.True(result.Success);
    Assert.True(result.DatabaseSuccess);
    Assert.True(result.LocalCSVSuccess);
}
```

---

## 📚 Additional Resources

### .specify Tool Usage

**Create Feature Plan:**
```powershell
.\.specify\scripts\powershell\create-new-feature.ps1 -FeatureName "ReceivingModule"
```

**Update Agent Context:**
```powershell
.\.specify\scripts\powershell\update-agent-context.ps1
```

**Check Prerequisites:**
```powershell
.\.specify\scripts\powershell\check-prerequisites.ps1
```

### Serena Commands for Migration

```powershell
# Find all Receiving files
Get-ChildItem -Recurse -Filter "*Receiving*.cs" | Select-Object FullName

# Check namespace usage
Select-String -Path "**/*.cs" -Pattern "MTM_Receiving_Application.ViewModels.Receiving"

# Verify no old references remain
Select-String -Path "**/*.cs" -Pattern "Receiving_.*ViewModel" -Exclude "ReceivingModule/*"
```

---

## ✅ Completion Checklist

### Pre-Implementation
- [ ] Create feature branch: `feature/modularize-workflows`
- [ ] Review all user stories
- [ ] Understand shared vs module-specific services
- [ ] Backup database

### Receiving Module
- [ ] Create folder structure
- [ ] Move and rename all files
- [ ] Update namespaces in all .cs files
- [ ] Update XAML namespaces in all .xaml files
- [ ] Update DI registration in App.xaml.cs
- [ ] Create README.md
- [ ] Write unit tests
- [ ] Write integration tests
- [ ] Verify build succeeds
- [ ] Manual UI testing

### Dunnage Module
- [ ] (Same checklist as Receiving)

### Routing Labels Module
- [ ] Create NEW folder structure
- [ ] Implement all ViewModels
- [ ] Implement all Views
- [ ] Implement Services
- [ ] Implement DAOs
- [ ] Create Models
- [ ] Create Database schema
- [ ] Create stored procedures
- [ ] Update MainWindow.xaml
- [ ] Register in App.xaml.cs
- [ ] Write tests
- [ ] Manual UI testing

### Final Integration
- [ ] Update MainWindow navigation
- [ ] Verify all three modules work independently
- [ ] Verify shared services work correctly
- [ ] Run full test suite
- [ ] Update documentation (README.md, AGENTS.md)
- [ ] Code review
- [ ] Merge to master

---

## 📌 Important Notes

1. **Use Serena Extensively:** For finding files, checking namespaces, verifying migrations
2. **Test Incrementally:** Don't move all files at once - migrate one module completely before starting the next
3. **Preserve Git History:** Use `git mv` instead of delete/create to maintain file history
4. **Update Documentation First:** Write user stories BEFORE implementation
5. **Follow .specify Workflow:** Use the specified tools for consistency

---

**END OF WORKFLOW GUIDE**
