# MTM Application - Module Reimplementation Guide

**Date:** 2026-01-03  
**Approach:** Clean reimplementation using Serena semantic tools  
**Strategy:** Build 3 self-contained modules from scratch (no file moving)

---

## 📋 Quick Navigation

1. [Why Reimplementation?](#why-reimplementation)
2. [Module Architecture](#module-architecture)
3. [Naming Standards](#naming-standards)
4. [Implementation Strategy](#implementation-strategy)
5. [User Stories](#user-stories)
6. [Serena-Powered Prompts](#serena-powered-prompts)

---

## 🎯 Why Reimplementation?

### Current State (Problems)
- Files scattered across 8+ directories
- Inconsistent naming (`Receiving_POEntryViewModel` vs `Dunnage_TypeSelectionViewModel`)
- Tight coupling between Receiving and Dunnage
- No clear module boundaries

### New Approach (Solution)
✅ **Build from scratch** - Cleaner than migrating legacy patterns  
✅ **Use existing logic** - Reference current files with Serena, copy proven patterns  
✅ **Consistent naming** - All modules use identical conventions  
✅ **Module isolation** - Each module is self-sufficient

---

## 🏗️ Module Architecture

### Folder Structure (Identical for All 3 Modules)

```
/{ModuleName}/
├── ViewModels/              # ViewModel_{Module}_{Name}.cs
├── Views/                   # View_{Module}_{Name}.xaml/.cs
├── Services/                # Service_{Module}_{Name}.cs
├── Data/                    # Dao_{Module}_{Name}.cs
├── Models/                  # Model_{Module}_{Name}.cs
├── Enums/                   # Enum_{Module}_{Name}.cs
├── Interfaces/              # I{ServiceName}.cs
├── Database/
│   ├── StoredProcedures/
│   └── Schemas/
└── README.md
```

**Example: ReceivingModule**
```
/ReceivingModule/
├── ViewModels/
│   ├── ViewModel_Receiving_Workflow.cs
│   ├── ViewModel_Receiving_ModeSelection.cs
│   ├── ViewModel_Receiving_POEntry.cs
│   ├── ViewModel_Receiving_PackageType.cs
│   ├── ViewModel_Receiving_LoadEntry.cs
│   ├── ViewModel_Receiving_WeightQuantity.cs
│   ├── ViewModel_Receiving_HeatLot.cs
│   ├── ViewModel_Receiving_Review.cs
│   ├── ViewModel_Receiving_ManualEntry.cs
│   └── ViewModel_Receiving_EditMode.cs

├── Views/
│   ├── View_Receiving_Workflow.xaml/.cs
│   ├── View_Receiving_ModeSelection.xaml/.cs
│   ├── View_Receiving_POEntry.xaml/.cs
│   ├── View_Receiving_PackageType.xaml/.cs
│   ├── View_Receiving_LoadEntry.xaml/.cs
│   ├── View_Receiving_WeightQuantity.xaml/.cs
│   ├── View_Receiving_HeatLot.xaml/.cs
│   ├── View_Receiving_Review.xaml/.cs
│   ├── View_Receiving_ManualEntry.xaml/.cs
│   └── View_Receiving_EditMode.xaml/.cs
├── Services/
│   ├── Service_Receiving_Workflow.cs
│   ├── Service_Receiving_Validation.cs
│   ├── Service_Receiving_CSVWriter.cs
│   └── Service_Receiving_Database.cs
├── Data/
│   ├── Dao_Receiving_Load.cs
│   ├── Dao_Receiving_Line.cs
│   └── Dao_Receiving_PackagePreference.cs
├── Models/
│   ├── Model_Receiving_Load.cs
│   ├── Model_Receiving_Session.cs
│   └── Model_Receiving_SaveResult.cs
├── Enums/
│   └── Enum_Receiving_WorkflowStep.cs
├── Interfaces/
│   ├── IReceivingWorkflowService.cs
│   ├── IReceivingValidationService.cs
│   └── IReceivingDatabaseService.cs
└── Database/
    ├── StoredProcedures/
    │   ├── sp_receiving_saveloads.sql
    │   └── sp_receiving_getloads.sql
    └── Schemas/
        └── receiving_loads.sql
```

---

## 📐 Naming Standards

### Universal Pattern

All modules follow this exact naming convention:

| Component | Pattern | Example |
|-----------|---------|---------|
| **ViewModels** | `ViewModel_{Module}_{Name}.cs` | `ViewModel_Receiving_POEntry.cs` |
| **Views** | `View_{Module}_{Name}.xaml` | `View_Receiving_POEntry.xaml` |
| **Services** | `Service_{Module}_{Name}.cs` | `Service_Receiving_Workflow.cs` |
| **DAOs** | `Dao_{Module}_{Name}.cs` | `Dao_Receiving_Load.cs` |
| **Models** | `Model_{Module}_{Name}.cs` | `Model_Receiving_Load.cs` |
| **Enums** | `Enum_{Module}_{Name}.cs` | `Enum_Receiving_WorkflowStep.cs` |
| **Interfaces** | `I{ServiceName}.cs` | `IReceivingWorkflowService.cs` |
| **Database** | `sp_{Module}_{Name}` | `sp_receiving_saveloads.sql` |

### Namespace Convention

```csharp
// Old (inconsistent)
namespace MTM_Receiving_Application.ViewModels.Receiving { }
namespace MTM_Receiving_Application.ViewModels.Dunnage { }

// New (consistent)
namespace MTM_Receiving_Application.ReceivingModule.ViewModels { }
namespace MTM_Receiving_Application.DunnageModule.ViewModels { }
namespace MTM_Receiving_Application.RoutingModule.ViewModels { }
```

---

## 🚀 Implementation Strategy

### Phase 1: Analyze Existing Code with Serena

Use Serena to extract logic WITHOUT reading entire files:

```powershell
# 1. Get overview of existing ViewModels (no body, just structure)
find_symbol(
    name_path_pattern="Receiving_POEntryViewModel",
    relative_path="ViewModels/Receiving",
    include_body=False,
    depth=1  # Include methods, but not their implementations
)

# 2. Read specific method implementations only when needed
find_symbol(
    name_path_pattern="Receiving_POEntryViewModel/LoadPOAsync",
    include_body=True
)

# 3. Find all references to understand dependencies
find_referencing_symbols(
    name_path="LoadPOAsync",
    relative_path="ViewModels/Receiving/Receiving_POEntryViewModel.cs"
)
```

**Token Efficiency:**
- Reading entire ViewModel file: ~5,000 tokens
- Getting symbol overview: ~500 tokens
- **Savings: 90%**

### Phase 2: Create Module from Scratch

**DO NOT move/rename files.** Create new files with clean implementations:

```csharp
// ReceivingModule/ViewModels/ViewModel_Receiving_POEntry.cs
using MTM_Receiving_Application.ReceivingModule.Interfaces;
using MTM_Receiving_Application.ReceivingModule.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MTM_Receiving_Application.ReceivingModule.ViewModels;

/// <summary>
/// Handles PO number entry and validation for Receiving workflow
/// </summary>
public partial class ViewModel_Receiving_POEntry : BaseViewModel
{
    private readonly IReceivingWorkflowService _workflowService;
    
    [ObservableProperty]
    private string _poNumber = string.Empty;
    
    public ViewModel_Receiving_POEntry(
        IReceivingWorkflowService workflowService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger) : base(errorHandler, logger)
    {
        _workflowService = workflowService;
    }
    
    [RelayCommand]
    private async Task LoadPOAsync()
    {
        // Copy proven logic from old Receiving_POEntryViewModel
        // using Serena to read only the LoadPOAsync method
    }
}
```

### Phase 3: Delete Old Files (After Testing)

Once ALL modules are working:
1. Run full test suite on new modules
2. Verify UI navigation
3. Delete old directories:
   - `ViewModels/Receiving/`
   - `ViewModels/Dunnage/`
   - `Views/Receiving/`
   - `Views/Dunnage/`
   - `Services/Receiving/`
   - `Data/Receiving/`
   - `Data/Dunnage/`

---

## 📖 User Stories

### Receiving Module (10 Stories)

**US-R001: Mode Selection**
```
As a receiving clerk
I want to choose between Guided, Manual, or Edit mode
So that I can use the workflow that fits my task

Acceptance Criteria:
✓ Display 3 mode cards with icons
✓ Show confirmation if switching with unsaved data (only if data exists)
✓ Guided → POEntry step
✓ Manual → Bulk grid
✓ Edit → Historical data loader

Implementation:
- ViewModel: ViewModel_Receiving_ModeSelection
- View: View_Receiving_ModeSelection
- Enum Value: Enum_Receiving_WorkflowStep.ModeSelection
```

**US-R002: PO Number Entry**
```
As a receiving clerk
I want to enter and validate PO numbers
So that I can pull correct parts from Infor Visual ERP

Acceptance Criteria:
✓ Format: PO-NNNNNN (auto-format "63150" → "PO-063150")
✓ Validate PO exists in Infor Visual (READ ONLY)
✓ Display PO status (Open/Closed)
✓ Load parts for PO
✓ Toggle "Non-PO Item" mode

Implementation:
- ViewModel: ViewModel_Receiving_POEntry
- View: View_Receiving_POEntry
- Service: Service_Receiving_Validation (PO format check)
- Infor Visual: SELECT from po_detail (ApplicationIntent=ReadOnly)
```

**US-R003: Package Type Selection**
```
As a receiving clerk
I want to select package types (Coils/Sheets/Skids/Custom)
So that labels reflect actual packaging

Acceptance Criteria:
✓ 4 preset options + custom entry
✓ Save preference per part
✓ Auto-load saved preferences

Implementation:
- ViewModel: ViewModel_Receiving_PackageType
- DAO: Dao_Receiving_PackagePreference
- SP: sp_SavePackageTypePreference
```

**US-R004: Load Quantity Entry**
```
As a receiving clerk
I want to specify number of loads (1-999)
So that correct number of labels generate

Implementation:
- ViewModel: ViewModel_Receiving_LoadEntry
- Service: Service_Receiving_Workflow.GenerateLoads()
```

**US-R005: Weight/Quantity Entry**
```
As a receiving clerk
I want to enter weight per load
So that inventory is accurate

Implementation:
- ViewModel: ViewModel_Receiving_WeightQuantity
- Model: Model_Receiving_Load.WeightQuantity
```

**US-R006: Heat/Lot Number Entry**
```
As a receiving clerk
I want to enter heat/lot numbers
So that materials are traceable

Implementation:
- ViewModel: ViewModel_Receiving_HeatLot
- Model: Model_Receiving_Load.HeatLotNumber
```

**US-R007: Review & Save**
```
As a receiving clerk
I want to review all data before saving
So that I catch errors

Acceptance Criteria:
✓ Grid display of all loads
✓ "Add Another Part" button (clears form, keeps session)
✓ "Save to Database" button
✓ Save to MySQL + Export CSV

Implementation:
- ViewModel: ViewModel_Receiving_Review
- Service: Service_Receiving_Database.SaveLoadsAsync()
- CSV: Service_Receiving_CSVWriter
```

**US-R008: Add Another Part**
```
As a receiving clerk
I want to add another part without re-entering PO
So that multi-part POs are efficient

Acceptance Criteria:
✓ Confirmation dialog (only if data exists)
✓ Clear form inputs BEFORE navigation
✓ Preserve reviewed loads in session

Implementation:
- Method: ViewModel_Receiving_Review.AddAnotherPartAsync()
- Fix: Clear session BEFORE AddCurrentPartToSessionAsync()
```

**US-R009: Manual Bulk Entry**
```
As a receiving clerk
I want to use a grid for multiple loads
So that large shipments are faster

Implementation:
- ViewModel: ViewModel_Receiving_ManualEntry
- Grid: ObservableCollection<Model_Receiving_Load>
```

**US-R010: Edit Historical Data**
```
As a supervisor
I want to edit past receiving records
So that errors can be corrected

Implementation:
- ViewModel: ViewModel_Receiving_EditMode
- DAO: Dao_Receiving_Load.GetLoads(dateRange)
```

### Dunnage Module (9 Stories)

**US-D001 through US-D009** (Similar structure to Receiving)
- ModeSelection, TypeSelection, PartSelection, DetailsEntry
- QuantityEntry, Review, AdminTypes, AdminParts, AdminInventory

### Routing Module (6 Stories - NEW)

**US-RL001: Mode Selection**
**US-RL002: Carrier Selection** (UPS, FedEx, USPS, LTL, Local)
**US-RL003: Destination Entry** (Address, City, State, ZIP)
**US-RL004: Shipment Details** (Weight, Tracking Number)
**US-RL005: Review & Print** (Label preview, Barcode)
**US-RL006: Manual Bulk Entry** (Grid for batch shipments)

---

## 🤖 Implementation Prompts

### Prompt 0: Pre-Implementation Setup (REQUIRED FIRST STEP)

```
TASK: Prepare codebase for modularization by archiving old files and resolving compilation errors.

CONTEXT:
- Use .specify tools for project planning
- Archive (rename to .md) all old Receiving/Dunnage files
- Fix compilation errors with placeholders or deletions
- Ensure project builds before module implementation begins

PHASE 1: Initialize .specify Workflow

1. Check prerequisites:
   .\.specify\scripts\powershell\check-prerequisites.ps1

2. Create feature plan:
   .\.specify\scripts\powershell\create-new-feature.ps1 -FeatureName "ModuleReimplementation"

3. This creates:
   - .specify/plans/ModuleReimplementation/
   - plan.md (task breakdown)
   - tasks.md (checklist)

PHASE 2: Use Serena to Identify Files to Archive

// Find all Receiving ViewModels
file_search(query="ViewModels/Receiving/*.cs")

// Find all Receiving Views
file_search(query="Views/Receiving/*.xaml*")

// Find all Receiving Services
grep_search(
    query="class.*Receiving.*Service",
    includePattern="Services/**/*.cs",
    isRegexp=True
)

// Find all Receiving DAOs
file_search(query="Data/Receiving/*.cs")

// Find all Dunnage ViewModels
file_search(query="ViewModels/Dunnage/*.cs")

// Find all Dunnage Views
file_search(query="Views/Dunnage/*.xaml*")

// Find all Dunnage Services
grep_search(
    query="class.*Dunnage.*Service",
    includePattern="Services/**/*.cs",
    isRegexp=True
)

// Find all Dunnage DAOs
file_search(query="Data/Dunnage/*.cs")

PHASE 3: Archive Files (Rename to .md)

For EACH file identified above, rename from .cs/.xaml to .md:

PowerShell script:
```powershell
# Archive Receiving files
Get-ChildItem -Path "ViewModels/Receiving" -Filter "*.cs" | ForEach-Object {
    Rename-Item $_.FullName -NewName "$($_.BaseName).cs.md"
}

Get-ChildItem -Path "Views/Receiving" -Filter "*.xaml*" | ForEach-Object {
    Rename-Item $_.FullName -NewName "$($_.Name).md"
}

Get-ChildItem -Path "Data/Receiving" -Filter "*.cs" | ForEach-Object {
    Rename-Item $_.FullName -NewName "$($_.BaseName).cs.md"
}

# Archive specific Services
Rename-Item "Services/Receiving/Service_ReceivingWorkflow.cs" -NewName "Service_ReceivingWorkflow.cs.md"
Rename-Item "Services/Receiving/Service_ReceivingValidation.cs" -NewName "Service_ReceivingValidation.cs.md"
Rename-Item "Services/Receiving/Service_MySQL_ReceivingLine.cs" -NewName "Service_MySQL_ReceivingLine.cs.md"
Rename-Item "Services/Database/Service_MySQL_Receiving.cs" -NewName "Service_MySQL_Receiving.cs.md"

# Archive Dunnage files
Get-ChildItem -Path "ViewModels/Dunnage" -Filter "*.cs" | ForEach-Object {
    Rename-Item $_.FullName -NewName "$($_.BaseName).cs.md"
}

Get-ChildItem -Path "Views/Dunnage" -Filter "*.xaml*" | ForEach-Object {
    Rename-Item $_.FullName -NewName "$($_.Name).md"
}

Get-ChildItem -Path "Data/Dunnage" -Filter "*.cs" | ForEach-Object {
    Rename-Item $_.FullName -NewName "$($_.BaseName).cs.md"
}

# Archive Dunnage Services
Rename-Item "Services/Receiving/Service_DunnageWorkflow.cs" -NewName "Service_DunnageWorkflow.cs.md"
Rename-Item "Services/Receiving/Service_DunnageAdminWorkflow.cs" -NewName "Service_DunnageAdminWorkflow.cs.md"
Rename-Item "Services/Receiving/Service_DunnageCSVWriter.cs" -NewName "Service_DunnageCSVWriter.cs.md"
Rename-Item "Services/Database/Service_MySQL_Dunnage.cs" -NewName "Service_MySQL_Dunnage.cs.md"

# Archive Main ViewModels
Rename-Item "ViewModels/Main/Main_ReceivingLabelViewModel.cs" -NewName "Main_ReceivingLabelViewModel.cs.md"
Rename-Item "ViewModels/Main/Main_DunnageLabelViewModel.cs" -NewName "Main_DunnageLabelViewModel.cs.md"
```

PHASE 4: Attempt Build and Identify Errors

dotnet build /p:Platform=x64 /p:Configuration=Debug > build_errors.txt

Expected errors:
1. Missing type references in App.xaml.cs
2. Missing using statements
3. Navigation tag references in MainWindow.xaml

PHASE 5: Fix Errors with Placeholders or Deletions

Strategy A: Remove DI Registrations (App.xaml.cs)

// Use Serena to find all registrations
grep_search(
    query="Receiving.*ViewModel|Dunnage.*ViewModel|Service_Receiving|Service_Dunnage",
    includePattern="App.xaml.cs",
    isRegexp=True
)

// Comment out or remove these lines:
// services.AddTransient<Receiving_POEntryViewModel>();
// services.AddSingleton<IService_ReceivingWorkflow, Service_ReceivingWorkflow>();
// etc.

Strategy B: Remove Navigation Items (MainWindow.xaml)

<NavigationView.MenuItems>
    <!-- COMMENT OUT old navigation items -->
    <!-- <NavigationViewItem Content="Receiving Labels" Tag="ReceivingWorkflowView"> -->
    <!-- <NavigationViewItem Content="Dunnage Labels" Tag="DunnageLabelPage"> -->
    
    <!-- Keep Carrier Delivery for future Routing Module -->
    <NavigationViewItem Content="Carrier Delivery" Tag="CarrierDeliveryLabelPage">
        <NavigationViewItem.Icon>
            <FontIcon Glyph="&#xE707;" />
        </NavigationViewItem.Icon>
    </NavigationViewItem>
</NavigationView.MenuItems>

Strategy C: Create Placeholder Navigation Handler (MainWindow.xaml.cs)

private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
{
    if (args.SelectedItemContainer?.Tag?.ToString() == "CarrierDeliveryLabelPage")
    {
        // Placeholder for Routing Module
        ContentFrame.Content = new TextBlock 
        { 
            Text = "Routing Module - Coming Soon",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 24
        };
        return;
    }
    
    // Old navigation code removed
}

Strategy D: Remove Settings References (if any)

// Use Serena to find Settings ViewModels that reference Receiving/Dunnage
grep_search(
    query="Receiving|Dunnage",
    includePattern="ViewModels/Settings/*.cs",
    isRegexp=True
)

// Comment out or remove affected sections

PHASE 6: Verify Clean Build

dotnet build /p:Platform=x64 /p:Configuration=Debug

Expected output: Build succeeded with 0 errors

PHASE 7: Update .specify Plan

Update .specify/plans/ModuleReimplementation/tasks.md:

- [x] Archive old Receiving files (renamed to .md)
- [x] Archive old Dunnage files (renamed to .md)
- [x] Remove DI registrations
- [x] Update MainWindow navigation
- [x] Verify clean build
- [ ] Implement ReceivingModule (Prompt 1)
- [ ] Implement DunnageModule (Prompt 2)
- [ ] Implement RoutingModule (Prompt 3)

DELIVERABLES:
✓ All old .cs/.xaml files renamed to .md (archived, not deleted)
✓ App.xaml.cs cleaned (no references to old files)
✓ MainWindow.xaml simplified (placeholder navigation only)
✓ Project builds with 0 errors
✓ .specify plan updated with progress
✓ build_errors.txt (for reference)

NEXT STEP: After successful build, proceed to Prompt 1 for ReceivingModule implementation.

WHY THIS APPROACH?
- Archives old code for reference (can use Serena to read .md files)
- Clean slate for new implementation
- Ensures no lingering dependencies
- .specify tracks progress systematically
- Can rollback by renaming .md back to original extensions
```

### Prompt 1: Receiving Module Implementation

```
PREREQUISITE: Complete Prompt 0 (Setup) first. Verify clean build.

TASK: Implement complete Receiving Module using Serena for efficient code reference.

CONTEXT:
- Framework: WinUI 3, .NET 8, MVVM (CommunityToolkit.Mvvm)
- Database: MySQL (stored procedures ONLY), Infor Visual (READ ONLY)
- Approach: Build NEW from scratch, reference archived .md files with Serena
- Naming: ViewModel_Receiving_*, View_Receiving_*, Service_Receiving_*, etc.

STEP 1: Reference Archived Code with Serena (Token Efficient)

// Even though files are .md now, Serena can still read their structure
read_file(
    filePath="ViewModels/Receiving/Receiving_POEntryViewModel.cs.md",
    startLine=1,
    endLine=100
)

// Get method implementations to copy proven logic
grep_search(
    query="LoadPOAsync|ValidatePoNumber|ToggleNonPOCommand",
    includePattern="ViewModels/Receiving/*.md",
    isRegexp=True
)

// Find anti-patterns to avoid in new code
search_for_pattern(
    substring_pattern="MessageBox\\.Show",
    relative_path="ViewModels/Receiving"
    // Note: Will find in .md files, showing what NOT to do
)

STEP 2: Create Module Structure

mkdir -p ReceivingModule/{ViewModels,Views,Services,Data,Models,Enums,Interfaces,Database/{StoredProcedures,Schemas}}

STEP 3: Implement 10 ViewModels (See User Stories US-R001 to US-R010)

Create each with new naming convention:
- ReceivingModule/ViewModels/ViewModel_Receiving_ModeSelection.cs
- ReceivingModule/ViewModels/ViewModel_Receiving_POEntry.cs
- ReceivingModule/ViewModels/ViewModel_Receiving_PackageType.cs
- ReceivingModule/ViewModels/ViewModel_Receiving_LoadEntry.cs
- ReceivingModule/ViewModels/ViewModel_Receiving_WeightQuantity.cs
- ReceivingModule/ViewModels/ViewModel_Receiving_HeatLot.cs
- ReceivingModule/ViewModels/ViewModel_Receiving_Review.cs
- ReceivingModule/ViewModels/ViewModel_Receiving_ManualEntry.cs
- ReceivingModule/ViewModels/ViewModel_Receiving_EditMode.cs
- ReceivingModule/ViewModels/ViewModel_Receiving_Workflow.cs

Namespace: MTM_Receiving_Application.ReceivingModule.ViewModels

STEP 4: Implement 10 Views

Create XAML + code-behind:
- ReceivingModule/Views/View_Receiving_ModeSelection.xaml/.cs
- (Same pattern for all 10)

Namespace: MTM_Receiving_Application.ReceivingModule.Views

STEP 5: Implement Services

- Service_Receiving_Workflow.cs (orchestration, step management)
- Service_Receiving_Validation.cs (PO format, business rules)
- Service_Receiving_Database.cs (facade over DAOs)
- Service_Receiving_CSVWriter.cs (export logic)

STEP 6: Implement DAOs

- Dao_Receiving_Load.cs
- Dao_Receiving_Line.cs
- Dao_Receiving_PackagePreference.cs

STEP 7: Implement Models

- Model_Receiving_Load.cs
- Model_Receiving_Session.cs
- Model_Receiving_SaveResult.cs

STEP 8: Create Interfaces

- IReceivingWorkflowService.cs
- IReceivingValidationService.cs
- IReceivingDatabaseService.cs

STEP 9: Register in DI (App.xaml.cs)

// Singletons
services.AddSingleton<IReceivingWorkflowService, Service_Receiving_Workflow>();
services.AddSingleton<IReceivingValidationService, Service_Receiving_Validation>();
services.AddSingleton<IReceivingDatabaseService, Service_Receiving_Database>();

// Transient ViewModels
services.AddTransient<ViewModel_Receiving_Workflow>();
services.AddTransient<ViewModel_Receiving_ModeSelection>();
services.AddTransient<ViewModel_Receiving_POEntry>();
services.AddTransient<ViewModel_Receiving_PackageType>();
services.AddTransient<ViewModel_Receiving_LoadEntry>();
services.AddTransient<ViewModel_Receiving_WeightQuantity>();
services.AddTransient<ViewModel_Receiving_HeatLot>();
services.AddTransient<ViewModel_Receiving_Review>();
services.AddTransient<ViewModel_Receiving_ManualEntry>();
services.AddTransient<ViewModel_Receiving_EditMode>();

STEP 10: Update Navigation (MainWindow.xaml)

<NavigationViewItem Content="Receiving Labels" Tag="ReceivingModule.View_Receiving_Workflow">
    <NavigationViewItem.Icon>
        <FontIcon Glyph="&#xE74C;" />
    </NavigationViewItem.Icon>
</NavigationViewItem>

STEP 11: Build and Test

dotnet build /p:Platform=x64 /p:Configuration=Debug
dotnet test --filter "FullyQualifiedName~ReceivingModule"

DELIVERABLES:
✓ Complete ReceivingModule folder with all files
✓ Updated App.xaml.cs
✓ Updated MainWindow.xaml
✓ Unit tests
✓ ReceivingModule/README.md
✓ .specify tasks.md updated

CONSTRAINTS:
✗ NO Infor Visual writes (SELECT only, ApplicationIntent=ReadOnly)
✗ NO raw SQL (stored procedures via Helper_Database_StoredProcedure)
✗ NO MessageBox.Show (use IService_ErrorHandler)
✓ ALL ViewModels are partial classes
✓ ALL Views use x:Bind
✓ Fix known bug: Clear session data BEFORE AddCurrentPartToSessionAsync()

SERENA EFFICIENCY:
- Read archived .md files selectively (~90% token savings vs full reads)
- Copy proven patterns, improve known issues
- Reference instead of rewrite
```

### Prompt 2: Dunnage Module Reimplementation

```
TASK: Reimplement Dunnage Module (same approach as Receiving)

SERENA ANALYSIS:

// Get Dunnage ViewModel structure
find_symbol(
    name_path_pattern="Dunnage_TypeSelectionViewModel",
    relative_path="ViewModels/Dunnage",
    include_body=False,
    depth=1
)

// Read specific methods
find_symbol(
    name_path_pattern="Dunnage_TypeSelectionViewModel/LoadTypesAsync",
    include_body=True
)

// Find Material.Icons usage patterns
search_for_pattern(
    substring_pattern="MaterialIconKind",
    relative_path="ViewModels/Dunnage"
)

IMPLEMENTATION:

Create: /DunnageModule/ with same structure as Receiving

ViewModels:
- ViewModel_Dunnage_Workflow
- ViewModel_Dunnage_ModeSelection
- ViewModel_Dunnage_TypeSelection
- ViewModel_Dunnage_PartSelection
- ViewModel_Dunnage_DetailsEntry
- ViewModel_Dunnage_QuantityEntry
- ViewModel_Dunnage_Review
- ViewModel_Dunnage_ManualEntry
- ViewModel_Dunnage_EditMode
- ViewModel_Dunnage_AdminMain
- ViewModel_Dunnage_AdminTypes
- ViewModel_Dunnage_AdminParts
- ViewModel_Dunnage_AdminInventory

UNIQUE FEATURES:
- Material.Icons.WinUI3 for type icons
- Admin workflow for type/part management
- Inventoried parts filtering
- Dynamic spec inputs (text, number, boolean)

DELIVERABLES: Same as Receiving Module
```

### Prompt 3: Routing Module Creation (NEW)

```
TASK: Create Routing Module from scratch (no existing code)

APPROACH: Use Receiving as template, adapt for routing workflow

SERENA REFERENCE:

// Study Receiving structure
get_symbols_overview(
    relative_path="ReceivingModule/ViewModels/ViewModel_Receiving_Workflow.cs",
    depth=1
)

// Copy workflow orchestration pattern
find_symbol(
    name_path_pattern="ViewModel_Receiving_Workflow/AdvanceToNextStepAsync",
    include_body=True
)

WORKFLOW STEPS (Enum_Routing_WorkflowStep):
1. ModeSelection
2. CarrierSelection
3. DestinationEntry
4. ShipmentDetails
5. Review
6. ManualEntry

ViewModels:
- ViewModel_Routing_Workflow
- ViewModel_Routing_ModeSelection
- ViewModel_Routing_CarrierSelection
- ViewModel_Routing_DestinationEntry
- ViewModel_Routing_ShipmentDetails
- ViewModel_Routing_Review
- ViewModel_Routing_ManualEntry

Models:
- Model_Routing_Label
- Model_Routing_Session
- Model_Routing_Carrier

Database:
CREATE TABLE routing_labels (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    CarrierId INT,
    DestinationAddress VARCHAR(500),
    TrackingNumber VARCHAR(100),
    ShipmentDate DATE,
    Weight DECIMAL(10,2),
    -- etc.
);

DELIVERABLES: Same as other modules + new database schema
```

---

## ✅ Implementation Checklist

### Pre-Flight
- [ ] Create branch: `git checkout -b feature/module-reimplementation`
- [ ] Ensure Serena is installed and project indexed
- [ ] Review all user stories
- [ ] Backup database

### Receiving Module
- [ ] Create /ReceivingModule/ folder structure
- [ ] Use Serena to analyze old ViewModels (90% token savings)
- [ ] Implement 10 ViewModels (new naming)
- [ ] Implement 10 Views (new naming)
- [ ] Implement 4 Services
- [ ] Implement 3 DAOs
- [ ] Create 3 Interfaces
- [ ] Update App.xaml.cs DI
- [ ] Update MainWindow.xaml
- [ ] Manual UI testing

### Dunnage Module
- [ ] (Same as Receiving, 14 ViewModels + Admin features)

### Routing Module
- [ ] Create /RoutingModule/ folder structure
- [ ] Implement 7 ViewModels (new)
- [ ] Implement 7 Views (new)
- [ ] Create database schema
- [ ] Create stored procedures
- [ ] Update App.xaml.cs DI
- [ ] Update MainWindow.xaml (change "Carrier Delivery" tag)

### Final Integration
- [ ] All 3 modules working independently
- [ ] Navigation between modules works
- [ ] Shared services (error handling, logging) work
- [ ] Full test suite passes
- [ ] Delete old files/folders
- [ ] Update AGENTS.md and README.md
- [ ] Merge to master

---

## 🔧 Serena Best Practices for This Project

### Token Efficiency
```
❌ BAD: Read entire 500-line ViewModel
read_file("ViewModels/Receiving/Receiving_POEntryViewModel.cs", 1, 500)
Tokens: ~5,000

✅ GOOD: Get overview, then read specific method
find_symbol("Receiving_POEntryViewModel", include_body=False, depth=1)
Tokens: ~500 (90% savings)

find_symbol("Receiving_POEntryViewModel/LoadPOAsync", include_body=True)
Tokens: ~200 (just the method you need)
```

### Finding Anti-Patterns
```
// Find all MessageBox.Show violations
search_for_pattern(
    substring_pattern="MessageBox\\.Show",
    restrict_search_to_code_files=True
)

// Find direct SQL (should use stored procedures)
search_for_pattern(
    substring_pattern="new SqlCommand|new MySqlCommand",
    restrict_search_to_code_files=True
)
```

### Understanding Dependencies
```
// Before refactoring a method, find all usages
find_referencing_symbols(
    name_path="AddAnotherPartAsync",
    relative_path="ViewModels/Receiving/Receiving_ReviewGridViewModel.cs"
)
```

---

**Ready to implement? Start with Prompt 1 for Receiving Module using .specify tools!**