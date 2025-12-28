# Quickstart: Dunnage Wizard Workflow UI Implementation

**Feature**: `008-dunnage-ui`  
**Branch**: `008-dunnage-ui`  
**Prerequisites**: Feature `007-architecture-compliance` complete (Services, DAOs, Models exist)

---

## Development Environment Setup

### 1. Clone Repository and Switch to Feature Branch

```powershell
git clone https://github.com/JDKoll1982/MTM_Receiving_Application.git
cd MTM_Receiving_Application
git checkout 008-dunnage-ui
```

### 2. Verify Prerequisites

**Required Tools**:
- Visual Studio 2022 (17.8+) with:
  - .NET Desktop Development workload
  - Windows App SDK components
  - C# 12 support
- .NET 8 SDK (8.0.416 or later)
- MySQL Server 8.0+ (local or remote access to `mtm_receiving_application` database)
- SQL Server (access to Infor Visual `MTMFG` database - READ ONLY)

**Verify Installation**:
```powershell
dotnet --version  # Should show 8.0.x
```

### 3. Restore NuGet Packages

```powershell
dotnet restore MTM_Receiving_Application.slnx
```

### 4. Database Connection Configuration

Update `Helpers/Database/Helper_Database_Variables.cs` with your connection strings:

```csharp
// MySQL (Application Database)
private const string MySQLServer = "localhost";
private const string MySQLDatabase = "mtm_receiving_application";
private const string MySQLUser = "root";
private const string MySQLPassword = "your_password";

// SQL Server (Infor Visual - READ ONLY)
private const string SQLServer = "VISUAL";
private const string SQLDatabase = "MTMFG";
private const string SQLUser = "SHOP2";
private const string SQLPassword = "SHOP";
```

### 5. Verify Database Schema

Ensure database was created from feature `007-architecture-compliance`:

```sql
-- MySQL - Check tables exist
USE mtm_receiving_application;
SHOW TABLES LIKE 'dunnage_%';
-- Should show: dunnage_types, dunnage_parts, dunnage_loads

-- Check stored procedures exist
SHOW PROCEDURE STATUS WHERE Db = 'mtm_receiving_application' AND Name LIKE 'sp_dunnage%';
```

---

## Build and Run

### Option 1: Visual Studio (Recommended)

1. Open `MTM_Receiving_Application.slnx` in Visual Studio 2022
2. Set build configuration: **Debug | x64**
3. Build solution: `Ctrl+Shift+B`
4. Run: `F5` (with debugging) or `Ctrl+F5` (without debugging)

### Option 2: Command Line

```powershell
# Build
dotnet build MTM_Receiving_Application.slnx /p:Platform=x64 /p:Configuration=Debug

# Run (after successful build)
# Navigate to output directory and run .exe
cd bin\x64\Debug\net8.0-windows10.0.19041.0\
.\MTM_Receiving_Application.exe
```

### Option 3: VS Code Task

```powershell
# Use predefined build task
Ctrl+Shift+B → Select "build-x64"
```

---

## Project Structure Overview

```text
MTM_Receiving_Application/
├── Views/
│   ├── Main/Main_DunnageLabelPage.xaml         ✅ EXISTING - Orchestrator page
│   └── Dunnage/                                 📁 EMPTY - Add step views here
│       ├── Dunnage_ModeSelectionView.xaml       🆕 TO CREATE
│       ├── Dunnage_TypeSelectionView.xaml       🆕 TO CREATE
│       ├── Dunnage_PartSelectionView.xaml       🆕 TO CREATE
│       ├── Dunnage_QuantityEntryView.xaml       🆕 TO CREATE
│       ├── Dunnage_DetailsEntryView.xaml        🆕 TO CREATE
│       ├── Dunnage_ReviewView.xaml              🆕 TO CREATE
│       ├── Dunnage_ManualEntryView.xaml         🆕 TO CREATE
│       └── Dunnage_EditModeView.xaml            🆕 TO CREATE
├── ViewModels/
│   ├── Main/Main_DunnageLabelViewModel.cs       ✅ EXISTING - Orchestrator ViewModel
│   └── Dunnage/                                 📁 EMPTY - Add step ViewModels here
│       ├── Dunnage_ModeSelectionViewModel.cs    🆕 TO CREATE
│       ├── Dunnage_TypeSelectionViewModel.cs    🆕 TO CREATE
│       ├── Dunnage_PartSelectionViewModel.cs    🆕 TO CREATE
│       ├── Dunnage_QuantityEntryViewModel.cs    🆕 TO CREATE
│       ├── Dunnage_DetailsEntryViewModel.cs     🆕 TO CREATE
│       ├── Dunnage_ReviewViewModel.cs           🆕 TO CREATE
│       ├── Dunnage_ManualEntryViewModel.cs      🆕 TO CREATE
│       └── Dunnage_EditModeViewModel.cs         🆕 TO CREATE
├── Services/                                    ✅ ALL EXIST (NO CHANGES)
│   ├── Database/Service_MySQL_Dunnage.cs
│   └── Receiving/
│       ├── Service_DunnageWorkflow.cs
│       └── Service_DunnageCSVWriter.cs
├── Data/Dunnage/                                ✅ ALL EXIST (NO CHANGES)
│   ├── Dao_DunnageType.cs
│   ├── Dao_DunnagePart.cs
│   └── Dao_DunnageLoad.cs
├── Models/Dunnage/                              ✅ ALL EXIST (NO CHANGES)
│   ├── Model_DunnageType.cs
│   ├── Model_DunnagePart.cs
│   └── Model_DunnageLoad.cs
└── App.xaml.cs                                  🔧 MODIFY - Register new ViewModels in DI
```

---

## Implementation Workflow

### Phase 1: Create ViewModels (8 new files)

**Order**: Create ViewModels BEFORE Views (enables IntelliSense in XAML)

1. `Dunnage_ModeSelectionViewModel.cs`
2. `Dunnage_TypeSelectionViewModel.cs`
3. `Dunnage_PartSelectionViewModel.cs`
4. `Dunnage_QuantityEntryViewModel.cs`
5. `Dunnage_DetailsEntryViewModel.cs`
6. `Dunnage_ReviewViewModel.cs`
7. `Dunnage_ManualEntryViewModel.cs`
8. `Dunnage_EditModeViewModel.cs`

**Template**: See [contracts/viewmodel-contracts.md](contracts/viewmodel-contracts.md) for property/command contracts.

**Register in DI** (App.xaml.cs):
```csharp
// Add to ConfigureServices method
services.AddTransient<Dunnage_ModeSelectionViewModel>();
services.AddTransient<Dunnage_TypeSelectionViewModel>();
services.AddTransient<Dunnage_PartSelectionViewModel>();
services.AddTransient<Dunnage_QuantityEntryViewModel>();
services.AddTransient<Dunnage_DetailsEntryViewModel>();
services.AddTransient<Dunnage_ReviewViewModel>();
services.AddTransient<Dunnage_ManualEntryViewModel>();
services.AddTransient<Dunnage_EditModeViewModel>();
```

### Phase 2: Create Views (16 new files - XAML + code-behind)

**Order**: Follow wizard step sequence

1. `Dunnage_ModeSelectionView.xaml` + `.xaml.cs`
2. `Dunnage_TypeSelectionView.xaml` + `.xaml.cs`
3. `Dunnage_PartSelectionView.xaml` + `.xaml.cs`
4. `Dunnage_QuantityEntryView.xaml` + `.xaml.cs`
5. `Dunnage_DetailsEntryView.xaml` + `.xaml.cs`
6. `Dunnage_ReviewView.xaml` + `.xaml.cs`
7. `Dunnage_ManualEntryView.xaml` + `.xaml.cs`
8. `Dunnage_EditModeView.xaml` + `.xaml.cs`

**Code-Behind Pattern** (minimal, UI-only):
```csharp
public sealed partial class Dunnage_ModeSelectionView : UserControl
{
    public Dunnage_ModeSelectionViewModel ViewModel { get; }

    public Dunnage_ModeSelectionView()
    {
        ViewModel = App.GetService<Dunnage_ModeSelectionViewModel>();
        InitializeComponent();
    }
}
```

### Phase 3: Update Main Orchestrator Page

**Modify**: `Views/Main/Main_DunnageLabelPage.xaml`

Add step views to Grid with visibility bindings:
```xml
<Grid>
    <views:Dunnage_ModeSelectionView 
        Visibility="{x:Bind ViewModel.IsModeSelectionVisible, Mode=OneWay, Converter={StaticResource BoolToVisibilityConverter}}" />
    
    <views:Dunnage_TypeSelectionView 
        Visibility="{x:Bind ViewModel.IsTypeSelectionVisible, Mode=OneWay, Converter={StaticResource BoolToVisibilityConverter}}" />
    
    <!-- Add all 8 step views -->
</Grid>
```

### Phase 4: Update Main Orchestrator ViewModel

**Modify**: `ViewModels/Main/Main_DunnageLabelViewModel.cs`

Add step visibility properties and StepChanged event handler (see [contracts/viewmodel-contracts.md](contracts/viewmodel-contracts.md#main_dunnagelabelviewmodel-orchestrator)).

### Phase 5: Create Unit Tests

**Create**: `MTM_Receiving_Application.Tests/Unit/ViewModels/Dunnage/` folder

1. `Dunnage_ModeSelectionViewModel_Tests.cs`
2. `Dunnage_TypeSelectionViewModel_Tests.cs`
3. ... (one test class per ViewModel)

**Test Pattern**:
```csharp
public class Dunnage_TypeSelectionViewModel_Tests
{
    private readonly Mock<IService_MySQL_Dunnage> _mockService;
    private readonly Dunnage_TypeSelectionViewModel _viewModel;

    public Dunnage_TypeSelectionViewModel_Tests()
    {
        _mockService = new Mock<IService_MySQL_Dunnage>();
        _viewModel = new Dunnage_TypeSelectionViewModel(
            _mockService.Object,
            Mock.Of<IService_Pagination>(),
            Mock.Of<IService_DunnageWorkflow>(),
            Mock.Of<IService_ErrorHandler>(),
            Mock.Of<ILoggingService>()
        );
    }

    [Fact]
    public async Task LoadTypesCommand_ShouldPopulateDisplayedTypes()
    {
        // Arrange, Act, Assert
    }
}
```

---

## Testing the Implementation

### Manual Testing Checklist

**Mode Selection**:
- [ ] Three mode cards display (Guided, Manual, Edit)
- [ ] Clicking "Guided Wizard" navigates to Type Selection
- [ ] "Set as default mode" checkbox saves preference
- [ ] Default mode auto-navigates on next launch

**Type Selection**:
- [ ] Types display in 3x3 grid (9 per page)
- [ ] Pagination works (Next/Previous buttons)
- [ ] Page indicator shows "Page X of Y"
- [ ] Clicking type navigates to Part Selection

**Part Selection**:
- [ ] Parts filtered by selected type
- [ ] InfoBar displays for inventoried parts
- [ ] Inventory method shows "Adjust In" initially
- [ ] Clicking part navigates to Quantity Entry

**Quantity Entry**:
- [ ] NumberBox defaults to 1
- [ ] Validation prevents Quantity ≤ 0
- [ ] Next button disabled until valid quantity
- [ ] Back returns to Part Selection with data intact

**Details Entry**:
- [ ] PO Number and Location fields display
- [ ] Dynamic spec inputs generated based on type
- [ ] Number specs use NumberBox
- [ ] Boolean specs use CheckBox
- [ ] Text specs use TextBox
- [ ] Inventory method updates to "Receive In" when PO entered
- [ ] Next navigates to Review

**Review**:
- [ ] DataGrid shows all session loads
- [ ] "Add Another" returns to Type Selection without clearing session
- [ ] "Save All" inserts to database and exports CSV
- [ ] Success message displays with load count
- [ ] Session clears after save

**Manual Entry**:
- [ ] Editable DataGrid displays
- [ ] "Add Row" adds empty row
- [ ] "Remove Row" deletes selected rows
- [ ] "Save All" validates and persists
- [ ] "Fill Blank Spaces" auto-fills from last row
- [ ] "Sort for Printing" reorders by Part ID → PO
- [ ] "Save to History" stores with "In Progress" status

**Edit Mode**:
- [ ] Date range filters work
- [ ] "Load from History" queries database
- [ ] Pagination displays 50 rows per page
- [ ] "Remove Row" deletes from database
- [ ] "Save All" updates modified rows

### Unit Test Execution

```powershell
# Run all tests
dotnet test

# Run only Dunnage ViewModel tests
dotnet test --filter "FullyQualifiedName~Dunnage"

# Run with verbose output
dotnet test --logger "console;verbosity=detailed"
```

### Integration Testing

1. Seed database with test data:
   ```sql
   -- Insert test types
   CALL sp_dunnage_types_insert('Pallet', 'Standard shipping pallet', '[{"name":"Width","data_type":"number","unit":"inches","default_value":48}]', 1);
   
   -- Insert test parts
   CALL sp_dunnage_parts_insert('PALLET-48X40', 1, '48x40 Pallet', 'Standard 48x40 inch pallet', 1);
   ```

2. Run application and complete full wizard workflow
3. Verify database records:
   ```sql
   SELECT * FROM dunnage_loads ORDER BY created_date DESC LIMIT 10;
   ```

4. Verify CSV export:
   - Check `C:\MTM_Receiving\Labels\Dunnage\[date]\` for generated CSV
   - Verify column count and data accuracy

---

## Debugging Tips

### XAML Compilation Errors

If you encounter "XamlCompiler.exe exited with code 1" without details:

```powershell
# Use Visual Studio build system for detailed errors
$vsPath = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -property installationPath
& "$vsPath\Common7\IDE\devenv.com" MTM_Receiving_Application.slnx /Rebuild "Debug|x64"
```

See [.github/instructions/xaml-troubleshooting.instructions.md](../.github/instructions/xaml-troubleshooting.instructions.md) for common XAML errors.

### ViewModel Not Resolving

If `App.GetService<Dunnage_TypeSelectionViewModel>()` returns null:
1. Verify ViewModel is registered in `App.xaml.cs` ConfigureServices
2. Check ViewModel constructor matches DI registration
3. Ensure all service dependencies are also registered

### Binding Errors

Enable XAML binding failure tracking:
1. Debug → Windows → Output
2. Filter by "Data binding"
3. Look for binding path errors (e.g., property not found)

### Performance Issues

If UI is sluggish:
- Check async operations are awaited (no blocking calls)
- Verify `IsBusy` flag is set/unset correctly
- Use profiler: Debug → Performance Profiler → CPU Usage

---

## Common Issues and Solutions

| Issue | Solution |
|-------|----------|
| **Build fails with "Type or namespace not found"** | Restore NuGet packages: `dotnet restore` |
| **ViewModels not in DI container** | Register in `App.xaml.cs` ConfigureServices |
| **XAML binding errors at runtime** | Use `x:Bind` with correct `Mode` (OneWay/TwoWay) |
| **Database connection fails** | Update connection strings in `Helper_Database_Variables.cs` |
| **CSV export path not found** | Ensure `C:\MTM_Receiving\Labels\Dunnage\` exists or create via `IService_DunnageCSVWriter` |
| **Pagination not working** | Verify `IService_Pagination` is injected and items set correctly |
| **Dynamic specs not displaying** | Check `SpecsJson` deserialization and `DataTemplateSelector` logic |

---

## Next Steps After Implementation

1. **Code Review**: Submit PR with all files, await review
2. **Documentation**: Update `Documentation/README.md` with Dunnage workflow user guide
3. **Deployment**: Merge to master after approval
4. **User Training**: Provide training on wizard workflow vs manual/edit modes
5. **Monitoring**: Track usage metrics and error rates in production logs

---

## Reference Documentation

- **Constitution**: [.specify/memory/constitution.md](../../.specify/memory/constitution.md) - Architecture rules
- **MVVM Guide**: [.github/instructions/mvvm-viewmodels.instructions.md](../../.github/instructions/mvvm-viewmodels.instructions.md)
- **View Guide**: [.github/instructions/mvvm-views.instructions.md](../../.github/instructions/mvvm-views.instructions.md)
- **DAO Pattern**: [.github/instructions/dao-instance-pattern.instructions.md](../../.github/instructions/dao-instance-pattern.instructions.md)
- **ViewModel Contracts**: [contracts/viewmodel-contracts.md](contracts/viewmodel-contracts.md)
- **Data Model**: [data-model.md](data-model.md)
- **Research**: [research.md](research.md)

---

**Ready to start? Create your first ViewModel following the contracts in `contracts/viewmodel-contracts.md`!**
